USE `isp-45_dolgih_ka_dolgih_schema2`;

SELECT '--- ТАБЛИЦЫ СХЕМЫ ---' AS section;
SELECT TABLE_NAME, TABLE_TYPE, TABLE_ROWS
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'isp-45_dolgih_ka_dolgih_schema2'
ORDER BY TABLE_TYPE, TABLE_NAME;

SELECT '--- СОТРУДНИКИ ---' AS section;
SELECT e.id_employee, e.full_name, r.name AS role_name, e.login, e.password
FROM employee e JOIN role r ON r.id_role = e.id_role ORDER BY e.id_employee;

SELECT '--- БЛЮДА ---' AS section;
SELECT d.id_dish, d.name, c.name AS category, d.price, d.image_path
FROM dish d JOIN category c ON c.id_category = d.id_category ORDER BY d.id_dish;

SELECT '--- ЗАКАЗЫ (СУММА) ---' AS section;
SELECT id_order, order_date, order_time, table_number, employee_name, status_name,
       position_count, items_count, total_cost, comment
FROM v_order_summary ORDER BY id_order;

SELECT '--- СОСТАВ ЗАКАЗОВ ---' AS section;
SELECT id_order, dish_name, quantity, price, line_cost FROM v_order_item_detail ORDER BY id_order, id_order_item;

SELECT '--- ПРОВЕРКА ЦЕЛОСТНОСТИ ---' AS section;
SELECT (SELECT COUNT(*) FROM employee e LEFT JOIN role r ON r.id_role = e.id_role WHERE r.id_role IS NULL) AS employees_without_role,
       (SELECT COUNT(*) FROM dish d LEFT JOIN category c ON c.id_category = d.id_category WHERE c.id_category IS NULL) AS dishes_without_category,
       (SELECT COUNT(*) FROM orders o LEFT JOIN employee e ON e.id_employee = o.id_employee WHERE e.id_employee IS NULL) AS orders_without_employee,
       (SELECT COUNT(*) FROM orders o LEFT JOIN order_status s ON s.id_status = o.id_status WHERE s.id_status IS NULL) AS orders_without_status,
       (SELECT COUNT(*) FROM order_item oi LEFT JOIN orders o ON o.id_order = oi.id_order WHERE o.id_order IS NULL) AS items_without_order,
       (SELECT COUNT(*) FROM order_item oi LEFT JOIN dish d ON d.id_dish = oi.id_dish WHERE d.id_dish IS NULL) AS items_without_dish,
       (SELECT COUNT(*) FROM orders o WHERE o.order_time IS NULL OR o.order_date IS NULL) AS orders_without_datetime;

SELECT '--- ОБЩАЯ ВЫРУЧКА И СТАТИСТИКА ---' AS section;
SELECT COUNT(*) AS orders_count, SUM(total_cost) AS revenue, ROUND(AVG(total_cost), 2) AS avg_check, SUM(items_count) AS dishes_sold
FROM v_order_summary;

SELECT '--- ПРОВЕРКА ОГРАНИЧЕНИЙ ---' AS section;
SELECT CONSTRAINT_NAME, TABLE_NAME, REFERENCED_TABLE_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'isp-45_dolgih_ka_dolgih_schema2' AND REFERENCED_TABLE_NAME IS NOT NULL
ORDER BY TABLE_NAME, CONSTRAINT_NAME;
