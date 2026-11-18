-- =====================================================================
--  Импорт данных заказчика в схему isp-45_dolgih_ka_dolgih_schema2
--  Источники: user_import.xlsx, Блюда_import.xlsx, Заказ_import.xlsx
--  Сформировано: 18.11.2026 15:25
-- =====================================================================
USE `isp-45_dolgih_ka_dolgih_schema2`;
START TRANSACTION;

-- Роли сотрудников
INSERT INTO role (id_role, name) VALUES
    (1, 'Администратор'),
    (2, 'Официант');

-- Сотрудники
INSERT INTO employee (id_employee, full_name, login, password, id_role) VALUES
    (1, 'Данилова Лина Адольфовна', 'm4ic8j5qgstw@gmail.com', '2L6KZG', 2),
    (2, 'Иванов Павел Макарович', 'd43zfg9tlsyv@gmail.com', 'uzWC67', 2),
    (3, 'Поляков Наум Борисович', '8ohgisf6k45w@outlook.com', '8ntwUp', 2),
    (4, 'Фадеев Антон Владимирович', 'hi1brwj46czx@mail.com', 'YOyhfR', 2),
    (5, 'Гаврилов Корнелий Давидович', 'fvkbcamhlj52@gmail.com', 'RSbvHv', 1),
    (6, 'Соловьёва Полина Парфеньевна', '9qxnce8jwruv@gmail.com', 'rwVDh9', 2),
    (7, 'Костина Алина Станиславовна', 'dotiex942p1r@gmail.com', 'LdNyos', 2),
    (8, 'Егоров Марк Тихонович', 'n0bmi2h1xral@tutanota.com', 'gynQMT', 2),
    (9, 'Григорьева Мелиана Георгиевна', 'sfm3t278kdvz@yahoo.com', 'AtnDjr', 2),
    (10, 'Михеева Валерия Филипповна', 'ilb8rdut0v7e@mail.com', 'JlFRCZ', 1);

-- Категории блюд
INSERT INTO category (id_category, name) VALUES
    (1, 'Десерты'),
    (2, 'Пицца'),
    (3, 'Первые блюда'),
    (4, 'Паста'),
    (5, 'Вторые блюда');

-- Блюда
INSERT INTO dish (id_dish, name, price, description, image_path, id_category) VALUES
    (1, 'Фруктовое ассорти', 395.00, 'Ассорти из апельсина, тропического киви, яблока, винограда, банана.', 'Fruit platter.jpg', 1),
    (2, 'Пицца Цезарь', 440.00, 'Грудка копчёная, томаты черри, салат листовой, соус Цезарь, сыр.', 'Pizza Caesar.jpg', 2),
    (3, 'Ароматная Уха', 250.00, 'Рыбный бульон, филе сёмги, филе судака, картофель, лук, помидор', 'Fish soup.jpg', 3),
    (4, 'Феттучини с тигровыми креветками', 305.00, 'Паста Феттучини, тигровые креветки, сливочный соус, сыр Пармезан', 'Fettuccine.jpg', 4),
    (5, 'Картофельные шарики', 115.00, 'Картофель, яйца, панировочные сухари, растительное масло, соль', NULL, 4),
    (6, 'Лосось со спаржей в сливочно-икорном соусе', 620.00, 'Филе сёмги, сливочный соус, спаржа.', 'Salmon.jpg', 5);

-- Статусы заказов
INSERT INTO order_status (id_status, name) VALUES
    (1, 'Новый'),
    (2, 'Готовится'),
    (3, 'Завершен'),
    (4, 'Отказ');

-- Заказы
INSERT INTO orders (id_order, id_employee, order_date, order_time, table_number, comment, id_status) VALUES
    (1, 2, '2022-11-30', '13:42:00', 2, NULL, 1),
    (2, 3, '2022-12-04', '14:34:00', 1, NULL, 1),
    (3, 4, '2022-12-11', '16:58:00', 4, NULL, 3),
    (4, 1, '2022-12-27', '10:27:00', 2, 'Меньше растительного масла', 3),
    (5, 8, '2022-12-27', '16:29:00', 3, NULL, 4),
    (6, 6, '2023-01-15', '17:18:00', 7, NULL, 2),
    (7, 7, '2023-01-15', '17:22:00', 7, NULL, 1),
    (8, 3, '2023-01-21', '18:58:00', 8, 'Без цитрусов', 2),
    (9, 8, '2023-01-27', '14:01:00', 2, 'Уха без лука', 3),
    (10, 9, '2023-02-05', '10:04:00', 6, NULL, 3),
    (11, 1, '2023-02-17', '09:33:00', 8, NULL, 4);

-- Позиции заказов
INSERT INTO order_item (id_order, id_dish, quantity) VALUES
    (1, 2, 1),
    (2, 5, 4),
    (3, 2, 1),
    (3, 1, 2),
    (4, 5, 1),
    (5, 6, 2),
    (6, 4, 5),
    (7, 3, 3),
    (8, 1, 1),
    (9, 3, 2),
    (9, 5, 1),
    (10, 4, 2),
    (11, 3, 1);

COMMIT;

-- Контрольные суммы импорта
SELECT 'role' AS entity, COUNT(*) AS rows_loaded FROM role
UNION ALL SELECT 'employee', COUNT(*) FROM employee
UNION ALL SELECT 'category', COUNT(*) FROM category
UNION ALL SELECT 'dish', COUNT(*) FROM dish
UNION ALL SELECT 'order_status', COUNT(*) FROM order_status
UNION ALL SELECT 'orders', COUNT(*) FROM orders
UNION ALL SELECT 'order_item', COUNT(*) FROM order_item;
