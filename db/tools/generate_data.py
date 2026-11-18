"""Формирование SQL-файла импорта данных из xlsx-файлов заказчика."""
import os
import re
import datetime
import openpyxl

BASE = r"C:\Users\Admin\Desktop\session_mysql\z1\Сессия 1"
OUT = r"C:\Users\Admin\Desktop\session_mysql\db\02_data.sql"


def q(value):
    """SQL-литерал строки."""
    if value is None:
        return "NULL"
    return "'" + str(value).replace("\\", "\\\\").replace("'", "''") + "'"


def s(value):
    """Нормализация строки: обрезка пробелов, схлопывание двойных пробелов."""
    if value is None:
        return None
    return re.sub(r"\s+", " ", str(value)).strip() or None


def read_rows(filename):
    wb = openpyxl.load_workbook(os.path.join(BASE, filename), data_only=True)
    ws = wb.worksheets[0]
    rows = []
    for row in ws.iter_rows(values_only=True):
        if all(c is None or str(c).strip() == "" for c in row):
            continue
        rows.append(row)
    wb.close()
    return rows[0], rows[1:]


# ---------------------------------------------------------------- сотрудники
head, rows = read_rows("user_import.xlsx")
roles, employees = [], []
for r in rows:
    fio, role, login, pwd = s(r[0]), s(r[1]), s(r[2]), s(r[3])
    if role not in roles:
        roles.append(role)
    employees.append((fio, role, login, pwd))
# Стабильный порядок ролей: сначала администратор
roles = sorted(roles, key=lambda x: (x != "Администратор", x))

# ---------------------------------------------------------------- блюда/категории
head, rows = read_rows("Блюда_import.xlsx")
categories, dishes = [], []
for r in rows:
    name, price, cat, descr, img = s(r[0]), r[1], s(r[2]), s(r[3]), s(r[4])
    if cat not in categories:
        categories.append(cat)
    dishes.append((name, float(price), cat, descr, img))

# ---------------------------------------------------------------- заказы
head, rows = read_rows("Заказ_import.xlsx")
statuses, orders = [], []
for r in rows:
    emp, d, t, table, items_raw, details, status = (
        s(r[0]), r[1], r[2], r[3], s(r[4]), s(r[5]), s(r[6])
    )
    if status not in statuses:
        statuses.append(status)
    items = []
    for part in str(items_raw).split(","):
        part = part.strip()
        m = re.match(r"^(.*?)\s+(\d+)$", part)
        if not m:
            raise ValueError(f"Не удалось разобрать позицию: {part!r}")
        items.append((m.group(1).strip(), int(m.group(2))))
    orders.append((emp, d, t, int(table), items, details, status))

# порядок статусов задаётся технологическим циклом заказа
STATUS_ORDER = ["Новый", "Готовится", "Завершен", "Отказ"]
statuses = [x for x in STATUS_ORDER if x in statuses] + [x for x in statuses if x not in STATUS_ORDER]

lines = []
add = lines.append
add("-- =====================================================================")
add("--  Импорт данных заказчика в схему isp-45_dolgih_ka_dolgih_schema2")
add("--  Источники: user_import.xlsx, Блюда_import.xlsx, Заказ_import.xlsx")
add(f"--  Сформировано: {datetime.datetime.now():%d.%m.%Y %H:%M}")
add("-- =====================================================================")
add("USE `isp-45_dolgih_ka_dolgih_schema2`;")
add("START TRANSACTION;")
add("")

add("-- Роли сотрудников")
add("INSERT INTO role (id_role, name) VALUES")
add(",\n".join(f"    ({i + 1}, {q(n)})" for i, n in enumerate(roles)) + ";")
add("")
add("-- Сотрудники")
add("INSERT INTO employee (id_employee, full_name, login, password, id_role) VALUES")
vals = []
for i, (fio, role, login, pwd) in enumerate(employees, 1):
    vals.append(f"    ({i}, {q(fio)}, {q(login)}, {q(pwd)}, {roles.index(role) + 1})")
add(",\n".join(vals) + ";")
add("")
add("-- Категории блюд")
add("INSERT INTO category (id_category, name) VALUES")
add(",\n".join(f"    ({i + 1}, {q(n)})" for i, n in enumerate(categories)) + ";")
add("")
add("-- Блюда")
add("INSERT INTO dish (id_dish, name, price, description, image_path, id_category) VALUES")
vals = []
for i, (name, price, cat, descr, img) in enumerate(dishes, 1):
    vals.append(
        f"    ({i}, {q(name)}, {price:.2f}, {q(descr)}, {q(img)}, {categories.index(cat) + 1})"
    )
add(",\n".join(vals) + ";")
add("")
add("-- Статусы заказов")
add("INSERT INTO order_status (id_status, name) VALUES")
add(",\n".join(f"    ({i + 1}, {q(n)})" for i, n in enumerate(statuses)) + ";")
add("")
add("-- Заказы")
add("INSERT INTO orders (id_order, id_employee, order_date, order_time, table_number, comment, id_status) VALUES")
vals = []
emp_names = {fio: i for i, (fio, *_rest) in enumerate(employees, 1)}
for i, (emp, d, t, table, items, details, status) in enumerate(orders, 1):
    if isinstance(d, (datetime.datetime, datetime.date)):
        dstr = d.strftime("%Y-%m-%d")
    else:
        dstr = str(d)[:10]
    if isinstance(t, (datetime.time, datetime.datetime)):
        tstr = t.strftime("%H:%M:%S")
    else:
        tstr = str(t)
    vals.append(
        f"    ({i}, {emp_names[emp]}, '{dstr}', '{tstr}', {table}, {q(details)}, {statuses.index(status) + 1})"
    )
add(",\n".join(vals) + ";")
add("")
add("-- Позиции заказов")
add("INSERT INTO order_item (id_order, id_dish, quantity) VALUES")
dish_names = {name: i for i, (name, *_rest) in enumerate(dishes, 1)}
vals = []
for i, (emp, d, t, table, items, details, status) in enumerate(orders, 1):
    for dish_name, qty in items:
        vals.append(f"    ({i}, {dish_names[dish_name]}, {qty})")
add(",\n".join(vals) + ";")
add("")
add("COMMIT;")
add("")
add("-- Контрольные суммы импорта")
add("SELECT 'role' AS entity, COUNT(*) AS rows_loaded FROM role")
add("UNION ALL SELECT 'employee', COUNT(*) FROM employee")
add("UNION ALL SELECT 'category', COUNT(*) FROM category")
add("UNION ALL SELECT 'dish', COUNT(*) FROM dish")
add("UNION ALL SELECT 'order_status', COUNT(*) FROM order_status")
add("UNION ALL SELECT 'orders', COUNT(*) FROM orders")
add("UNION ALL SELECT 'order_item', COUNT(*) FROM order_item;")

with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(lines) + "\n")

print("roles:", roles)
print("categories:", categories)
print("statuses:", statuses)
print("employees:", len(employees), "dishes:", len(dishes), "orders:", len(orders),
      "items:", sum(len(o[4]) for o in orders))
print("written:", OUT)
