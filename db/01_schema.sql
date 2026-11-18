-- =====================================================================
--  Проект: ООО «КушайВкусно» — информационная система для сотрудников зала
--  Демонстрационный экзамен, Сессия 1
--  СУБД: MySQL 8.0
--  Схема: isp-45_dolgih_ka_dolgih_schema2
--  Структура приведена к третьей нормальной форме (3НФ)
-- =====================================================================

DROP DATABASE IF EXISTS `isp-45_dolgih_ka_dolgih_schema2`;
CREATE DATABASE `isp-45_dolgih_ka_dolgih_schema2`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE `isp-45_dolgih_ka_dolgih_schema2`;

-- ---------------------------------------------------------------------
-- Справочник ролей сотрудников (Официант, Администратор)
-- ---------------------------------------------------------------------
CREATE TABLE role (
    id_role   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name      VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_role PRIMARY KEY (id_role),
    CONSTRAINT uq_role_name UNIQUE (name)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Сотрудники (пользователи системы)
-- ---------------------------------------------------------------------
CREATE TABLE employee (
    id_employee INT UNSIGNED NOT NULL AUTO_INCREMENT,
    full_name   VARCHAR(150) NOT NULL,
    login       VARCHAR(100) NOT NULL,
    password    VARCHAR(100) NOT NULL,
    id_role     INT UNSIGNED NOT NULL,
    CONSTRAINT pk_employee PRIMARY KEY (id_employee),
    CONSTRAINT uq_employee_login UNIQUE (login),
    CONSTRAINT fk_employee_role FOREIGN KEY (id_role)
        REFERENCES role (id_role) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Справочник категорий блюд
-- ---------------------------------------------------------------------
CREATE TABLE category (
    id_category INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name        VARCHAR(80)  NOT NULL,
    CONSTRAINT pk_category PRIMARY KEY (id_category),
    CONSTRAINT uq_category_name UNIQUE (name)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Блюда
-- ---------------------------------------------------------------------
CREATE TABLE dish (
    id_dish     INT UNSIGNED   NOT NULL AUTO_INCREMENT,
    name        VARCHAR(150)   NOT NULL,
    price       DECIMAL(10, 2) NOT NULL,
    description TEXT           NULL,
    image_path  VARCHAR(255)   NULL,
    id_category INT UNSIGNED   NOT NULL,
    CONSTRAINT pk_dish PRIMARY KEY (id_dish),
    CONSTRAINT uq_dish_name UNIQUE (name),
    CONSTRAINT chk_dish_price CHECK (price >= 0),
    CONSTRAINT fk_dish_category FOREIGN KEY (id_category)
        REFERENCES category (id_category) ON UPDATE CASCADE ON DELETE RESTRICT,
    INDEX ix_dish_category (id_category)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Справочник статусов заказа
-- ---------------------------------------------------------------------
CREATE TABLE order_status (
    id_status INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name      VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_order_status PRIMARY KEY (id_status),
    CONSTRAINT uq_order_status_name UNIQUE (name)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Заказы (шапка заказа)
-- ---------------------------------------------------------------------
CREATE TABLE orders (
    id_order     INT UNSIGNED NOT NULL AUTO_INCREMENT,
    id_employee  INT UNSIGNED NOT NULL,
    order_date   DATE         NOT NULL,
    order_time   TIME         NOT NULL,
    table_number INT UNSIGNED NOT NULL,
    comment      VARCHAR(255) NULL,
    id_status    INT UNSIGNED NOT NULL,
    CONSTRAINT pk_orders PRIMARY KEY (id_order),
    CONSTRAINT chk_orders_table CHECK (table_number > 0),
    CONSTRAINT fk_orders_employee FOREIGN KEY (id_employee)
        REFERENCES employee (id_employee) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_orders_status FOREIGN KEY (id_status)
        REFERENCES order_status (id_status) ON UPDATE CASCADE ON DELETE RESTRICT,
    INDEX ix_orders_employee (id_employee),
    INDEX ix_orders_status (id_status),
    INDEX ix_orders_date (order_date)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Позиции заказа (связь «заказ — блюдо» с количеством)
-- ---------------------------------------------------------------------
CREATE TABLE order_item (
    id_order_item INT UNSIGNED NOT NULL AUTO_INCREMENT,
    id_order      INT UNSIGNED NOT NULL,
    id_dish       INT UNSIGNED NOT NULL,
    quantity      INT UNSIGNED NOT NULL,
    CONSTRAINT pk_order_item PRIMARY KEY (id_order_item),
    CONSTRAINT uq_order_item UNIQUE (id_order, id_dish),
    CONSTRAINT chk_order_item_quantity CHECK (quantity > 0),
    CONSTRAINT fk_order_item_orders FOREIGN KEY (id_order)
        REFERENCES orders (id_order) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_order_item_dish FOREIGN KEY (id_dish)
        REFERENCES dish (id_dish) ON UPDATE CASCADE ON DELETE RESTRICT,
    INDEX ix_order_item_dish (id_dish)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Представление: заказы с суммой и составом (используется приложением)
-- ---------------------------------------------------------------------
CREATE VIEW v_order_summary AS
SELECT o.id_order,
       o.order_date,
       o.order_time,
       o.table_number,
       o.comment,
       s.name                                   AS status_name,
       e.full_name                              AS employee_name,
       COUNT(oi.id_order_item)                  AS position_count,
       COALESCE(SUM(oi.quantity), 0)            AS items_count,
       COALESCE(SUM(oi.quantity * d.price), 0)  AS total_cost
FROM orders o
         JOIN order_status s ON s.id_status = o.id_status
         JOIN employee e ON e.id_employee = o.id_employee
         LEFT JOIN order_item oi ON oi.id_order = o.id_order
         LEFT JOIN dish d ON d.id_dish = oi.id_dish
GROUP BY o.id_order, o.order_date, o.order_time, o.table_number,
         o.comment, s.name, e.full_name;

-- ---------------------------------------------------------------------
-- Представление: состав заказа (позиции с ценами и стоимостью строки)
-- ---------------------------------------------------------------------
CREATE VIEW v_order_item_detail AS
SELECT oi.id_order_item,
       oi.id_order,
       oi.id_dish,
       d.name                AS dish_name,
       c.name                AS category_name,
       oi.quantity,
       d.price,
       (oi.quantity * d.price) AS line_cost
FROM order_item oi
         JOIN dish d ON d.id_dish = oi.id_dish
         JOIN category c ON c.id_category = d.id_category;
