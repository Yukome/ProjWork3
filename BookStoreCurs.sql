CREATE DATABASE BookStoreCurs;
GO

USE BookStoreCurs;
GO

CREATE TABLE countries (
	ID_country INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(100) NOT NULL UNIQUE,
);
GO

CREATE TABLE genres (
	ID_genre INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(100) NOT NULL UNIQUE,
	genre_description TEXT
);
GO

CREATE TABLE authors (
	ID_author INT PRIMARY KEY IDENTITY(1, 1),
	author_name VARCHAR(50) NOT NULL,
	author_surname VARCHAR(50) NOT NULL,
	author_patronymic VARCHAR(50),
	date_of_birth DATE CHECK (date_of_birth <= GETDATE()),
	country_ID INT NOT NULL,
	FOREIGN KEY (country_ID) REFERENCES countries(ID_country),
	biography TEXT
);
GO

CREATE TABLE roles (
	ID_role INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE publishing_houses (
	ID_publishing_house INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(200) NOT NULL UNIQUE,
	publishing_house_address VARCHAR(300) NOT NULL,
	phone VARCHAR(20) NOT NULL CHECK (phone LIKE '+%' OR phone LIKE '8%'),
	email VARCHAR(100) CHECK (email LIKE '%_@__%.__%')
);
GO

CREATE TABLE users (
	ID_user INT PRIMARY KEY IDENTITY(1, 1),
	user_login VARCHAR(50) NOT NULL UNIQUE,
	password_hash VARCHAR(256) NOT NULL,
	email VARCHAR(100) NOT NULL UNIQUE CHECK (email LIKE '%_@__%.__%'),
	users_name VARCHAR(50) NOT NULL,
	users_surname VARCHAR(50) NOT NULL,
	users_patronymic VARCHAR(50),
	date_of_birth DATE CHECK (date_of_birth <= GETDATE()),
	avatar_path VARCHAR(500),
	role_ID INT NOT NULL,
	FOREIGN KEY (role_ID) REFERENCES roles(ID_role)
);
GO

CREATE TABLE books (
	ID_book INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(255) NOT NULL,
	author_ID INT NOT NULL,
	FOREIGN KEY (author_ID) REFERENCES authors(ID_author),
	publishing_house_ID INT NOT NULL,
	FOREIGN KEY (publishing_house_ID) REFERENCES publishing_houses(ID_publishing_house),
	genre_ID INT NOT NULL,
	FOREIGN KEY (genre_ID) REFERENCES genres(ID_genre),
	isbn VARCHAR(13) NOT NULL UNIQUE,
	year_of_publication INT NOT NULL CHECK(year_of_publication >= 1000 AND year_of_publication <= YEAR(GETDATE())),
	price DECIMAL(6,2) NOT NULL CHECK(price > 0),
	quantity INT NOT NULL CHECK(quantity >= 0 AND quantity <= 9999),
	book_description TEXT NOT NULL,
	cover_path VARCHAR(500),
	add_date DATE NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE reviews (
	ID_review INT PRIMARY KEY IDENTITY(1, 1),
	book_ID INT NOT NULL,
	FOREIGN KEY (book_ID) REFERENCES books(ID_book),
	users_ID INT NOT NULL,
	FOREIGN KEY (users_ID) REFERENCES users(ID_user),
	rating INT NOT NULL CHECK(rating >= 1 AND rating <= 5),
	text_review TEXT NOT NULL,
	add_date DATE NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE carts (
	ID_cart INT PRIMARY KEY IDENTITY(1, 1),
	users_ID INT NOT NULL,
	FOREIGN KEY (users_ID) REFERENCES users(ID_user),
	book_ID INT NOT NULL,
	FOREIGN KEY (book_ID) REFERENCES books(ID_book),
	quantity INT NOT NULL CHECK(quantity > 0 AND quantity <= 99) DEFAULT 1,
	add_date DATE NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE order_statuses (
	ID_order_status INT PRIMARY KEY IDENTITY(1, 1),
	title VARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE orders (
	ID_order INT PRIMARY KEY IDENTITY(1, 1),
	users_ID INT NOT NULL,
	FOREIGN KEY (users_ID) REFERENCES users(ID_user),
	delivery_address TEXT NOT NULL,
	payment_method VARCHAR(50) NOT NULL CHECK(payment_method IN ('Наличные', 'Карта', 'Перевод')),
	phone VARCHAR(20) NOT NULL CHECK (phone LIKE '+%' OR phone LIKE '8%'),
	add_date DATE NOT NULL DEFAULT GETDATE(),
	total_sum DECIMAL(6,2) NOT NULL CHECK(total_sum > 0),
	order_status_ID INT NOT NULL,
	FOREIGN KEY (order_status_ID) REFERENCES order_statuses(ID_order_status),
);
GO

CREATE TABLE order_positions (
	ID_order_position INT PRIMARY KEY IDENTITY(1, 1),
	book_ID INT NOT NULL,
	FOREIGN KEY (book_ID) REFERENCES books(ID_book),
	quantity INT NOT NULL CHECK(quantity > 0 AND quantity <= 99) DEFAULT 1,
	purchase_price DECIMAL(6,2) NOT NULL CHECK(purchase_price > 0),
	order_ID INT NOT NULL,
	FOREIGN KEY (order_ID) REFERENCES orders(ID_order),
);
GO

CREATE TABLE notifications (
	ID_notification INT PRIMARY KEY IDENTITY(1, 1),
	users_ID INT NOT NULL,
	FOREIGN KEY (users_ID) REFERENCES users(ID_user),
	notification_text VARCHAR(255) NOT NULL,
	add_date DATE NOT NULL DEFAULT GETDATE(),
	status_n BIT NOT NULL DEFAULT 0
);
GO

INSERT INTO countries (title) VALUES
	('Австралия'),
	('Австрия'),
	('Азербайджан'),
	('Албания'),
	('Алжир'),
	('Ангола'),
	('Андорра'),
	('Антигуа и Барбуда'),
	('Аргентина'),
	('Армения'),
	('Афганистан'),
	('Багамские Острова'),
	('Бангладеш'),
	('Барбадос'),
	('Бахрейн'),
	('Беларусь'),
	('Белиз'),
	('Бельгия'),
	('Бенин'),
	('Болгария'),
	('Боливия'),
	('Босния и Герцеговина'),
	('Ботсвана'),
	('Бразилия'),
	('Бруней-Даруссалам'),
	('Буркина-Фасо'),
	('Бурунди'),
	('Бутан'),
	('Вануату'),
	('Ватикан'),
	('Великобритания'),
	('Венгрия'),
	('Венесуэла'),
	('Восточный Тимор'),
	('Вьетнам'),
	('Габон'),
	('Гаити'),
	('Гайана'),
	('Гамбия'),
	('Гана'),
	('Гватемала'),
	('Гвинея'),
	('Гвинея-Бисау'),
	('Германия'),
	('Гондурас'),
	('Гренада'),
	('Греция'),
	('Грузия'),
	('Дания'),
	('Джибути'),
	('Доминика'),
	('Доминиканская Республика'),
	('Египет'),
	('Замбия'),
	('Зимбабве'),
	('Израиль'),
	('Индия'),
	('Индонезия'),
	('Иордания'),
	('Ирак'),
	('Иран'),
	('Ирландия'),
	('Исландия'),
	('Испания'),
	('Италия'),
	('Йемен'),
	('Кабо-Верде'),
	('Казахстан'),
	('Камбоджа'),
	('Камерун'),
	('Канада'),
	('Катар'),
	('Кения'),
	('Кипр'),
	('Кирибати'),
	('Китай'),
	('Колумбия'),
	('Коморские Острова'),
	('Конго'),
	('Демократическая Республика Конго'),
	('Корейская Народно-Демократическая Республика'),
	('Республика Корея'),
	('Коста-Рика'),
	('Кот-д''Ивуар'),
	('Куба'),
	('Кувейт'),
	('Кыргызстан'),
	('Лаос'),
	('Латвия'),
	('Лесото'),
	('Либерия'),
	('Ливан'),
	('Ливия'),
	('Литва'),
	('Лихтенштейн'),
	('Люксембург'),
	('Маврикий'),
	('Мавритания'),
	('Мадагаскар'),
	('Малави'),
	('Малайзия'),
	('Мали'),
	('Мальдивы'),
	('Мальта'),
	('Марокко'),
	('Маршалловы Острова'),
	('Мексика'),
	('Микронезия'),
	('Мозамбик'),
	('Молдова'),
	('Монако'),
	('Монголия'),
	('Мьянма'),
	('Намибия'),
	('Науру'),
	('Непал'),
	('Нигер'),
	('Нигерия'),
	('Нидерланды'),
	('Никарагуа'),
	('Новая Зеландия'),
	('Норвегия'),
	('Объединенные Арабские Эмираты'),
	('Оман'),
	('Пакистан'),
	('Палау'),
	('Панама'),
	('Папуа-Новая Гвинея'),
	('Парагвай'),
	('Перу'),
	('Польша'),
	('Португалия'),
	('Россия'),
	('Руанда'),
	('Румыния'),
	('Сальвадор'),
	('Самоа'),
	('Сан-Марино'),
	('Сан-Томе и Принсипи'),
	('Саудовская Аравия'),
	('Северная Македония'),
	('Сейшельские Острова'),
	('Сенегал'),
	('Сент-Винсент и Гренадины'),
	('Сент-Китс и Невис'),
	('Сент-Люсия'),
	('Сербия'),
	('Сингапур'),
	('Сирия'),
	('Словакия'),
	('Словения'),
	('Соединенные Штаты Америки'),
	('Соломоновы Острова'),
	('Сомали'),
	('Судан'),
	('Суринам'),
	('Сьерра-Леоне'),
	('Таджикистан'),
	('Таиланд'),
	('Танзания'),
	('Того'),
	('Тонга'),
	('Тринидад и Тобаго'),
	('Тувалу'),
	('Тунис'),
	('Туркменистан'),
	('Турция'),
	('Уганда'),
	('Узбекистан'),
	('Украина'),
	('Уругвай'),
	('Фиджи'),
	('Филиппины'),
	('Финляндия'),
	('Франция'),
	('Хорватия'),
	('Центральноафриканская Республика'),
	('Чад'),
	('Черногория'),
	('Чехия'),
	('Чили'),
	('Швейцария'),
	('Швеция'),
	('Шри-Ланка'),
	('Эквадор'),
	('Экваториальная Гвинея'),
	('Эритрея'),
	('Эсватини'),
	('Эстония'),
	('Эфиопия'),
	('Южно-Африканская Республика'),
	('Южный Судан'),
	('Ямайка'),
	('Япония');
GO

INSERT INTO genres (title, genre_description) VALUES 
('Роман', 'Литературный жанр, эпическое повествование о жизни и развитии личности'),
('Фантастика', 'Жанр, основанный на фантастических допущениях'),
('Детектив', 'Жанр, описывающий расследование преступлений');
GO

INSERT INTO authors (author_name, author_surname, author_patronymic, date_of_birth, country_ID, biography) VALUES 
('Лев', 'Толстой', 'Николаевич', '1828-09-09', (SELECT ID_country FROM countries WHERE title = 'Россия'), 'Великий русский писатель, автор "Войны и мира"'),
('Фёдор', 'Достоевский', 'Михайлович', '1821-11-11', (SELECT ID_country FROM countries WHERE title = 'Россия'), 'Русский писатель, мыслитель, философ'),
('Агата', 'Кристи', NULL, '1890-09-15', (SELECT ID_country FROM countries WHERE title = 'Великобритания'), 'Английская писательница, мастер детективного жанра');
GO

INSERT INTO roles (title) VALUES 
('Администратор'),
('Менеджер'),
('Покупатель');
GO

INSERT INTO publishing_houses (title, publishing_house_address, phone, email) VALUES 
('Эксмо', 'Москва, ул. Зорге, д. 1', '+74951111111', 'info@eksmo.ru'),
('АСТ', 'Москва, ул. Пресненская наб., д. 6', '+74952222222', 'info@ast.ru'),
('Питер', 'Санкт-Петербург, пр. Обуховской обороны, д. 123', '+78123333333', 'info@piter.com');
GO

INSERT INTO users (user_login, password_hash, email, users_name, users_surname, users_patronymic, date_of_birth, avatar_path, role_ID) VALUES 
('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@bookstore.ru', 'Иван', 'Иванов', 'Иванович', '1985-03-15', '/images/avatars/admin.jpg', 1),
('manager', '6ee4a469cd4e91053847f5d3fcb61dbcc91e8f0ef10be7748da4c4a1ba382d17', 'manager@bookstore.ru', 'Петр', 'Петров', 'Петрович', '1990-07-22', '/images/avatars/manager.jpg', 2),
('user1', '0a041b9462caa4a31bac3567e0b6e6fd9100787db2ab433d96f6d178cabfce90', 'user1@mail.ru', 'Анна', 'Сидорова', 'Сергеевна', '1995-11-03', '/images/avatars/user1.jpg', 3);
GO

INSERT INTO books (title, author_ID, publishing_house_ID, genre_ID, isbn, year_of_publication, price, quantity, book_description, cover_path, add_date) VALUES 
('Война и мир', 1, 1, 1, '9785170900015', 1869, 1500.00, 10, 'Роман-эпопея, описывающий русское общество в эпоху войн против Наполеона', '/images/covers/war_and_peace.jpg', GETDATE()),
('Преступление и наказание', 2, 2, 1, '9785389022258', 1866, 1200.00, 8, 'Роман о студенте Раскольникове и его теории', '/images/covers/crime_and_punishment.jpg', GETDATE()),
('Убийство в "Восточном экспрессе"', 3, 3, 3, '9785699256961', 1934, 800.00, 15, 'Детектив об убийстве в поезде', '/images/covers/orient_express.jpg', GETDATE());
GO

INSERT INTO reviews (book_ID, users_ID, rating, text_review, add_date) VALUES 
(1, 3, 5, 'Великое произведение! Обязательно к прочтению', GETDATE()),
(2, 3, 4, 'Очень глубокий роман, но тяжелый для восприятия', GETDATE()),
(1, 2, 5, 'Классика мировой литературы', GETDATE());
GO

INSERT INTO carts (users_ID, book_ID, quantity, add_date) VALUES 
(3, 1, 1, GETDATE()),
(3, 3, 2, GETDATE()),
(2, 2, 1, GETDATE());
GO

INSERT INTO order_statuses (title) VALUES 
('Оформлен'),
('Оплачен'),
('Доставлен'),
('Отменен');
GO

INSERT INTO orders (users_ID, delivery_address, payment_method, phone, add_date, total_sum, order_status_ID) VALUES 
(3, 'г. Москва, ул. Ленина, д. 10, кв. 5', 'Карта', '+79991112233', GETDATE(), 2300.00, 1),
(3, 'г. Москва, ул. Ленина, д. 10, кв. 5', 'Наличные', '+79991112233', GETDATE(), 800.00, 2),
(2, 'г. Москва, ул. Гагарина, д. 15, офис 302', 'Перевод', '+79991114455', GETDATE(), 1200.00, 3);
GO

INSERT INTO order_positions (book_ID, quantity, purchase_price, order_ID) VALUES 
(1, 1, 1500.00, 1),
(3, 1, 800.00, 1),
(3, 1, 800.00, 2),
(2, 1, 1200.00, 3);
GO

INSERT INTO notifications (users_ID, notification_text, add_date, status_n) VALUES 
(3, 'Заказ №1 оформлен и готов к обработке', GETDATE(), 0),
(3, 'Заказ №2 оплачен', GETDATE(), 0),
(2, 'Заказ №3 доставлен', GETDATE(), 1);
GO
