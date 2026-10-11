from faker import Faker
from locust import HttpUser, task, SequentialTaskSet, TaskSet, constant_throughput
import random
import datetime

fake = Faker('ru_RU')

class AdminTests(TaskSet):
    def on_start(self):
        self.client.post('/api/Auth/login', json={
            'login': 'admin',
            'password': 'admin'
        })

        self.author_ids = [a['idAuthor'] for a in self.client.get('/api/Authors').json()]
        self.house_ids = [h['idPublishingHouse'] for h in self.client.get('/api/PublishingHouses').json()]
        self.genre_ids = [g['idGenre'] for g in self.client.get('/api/Genres').json()]
        self.book_ids = [b['idBook'] for b in self.client.get('/api/Books').json()]
        self.country_ids = [c['idCountry'] for c in self.client.get('/api/Countries').json()]
        self.cart_ids = [ca['idCart'] for ca in self.client.get('/api/Carts').json()]
        self.notification_ids = [n['idNotification'] for n in self.client.get('/api/Notifications').json()]
        self.order_position_ids = [op['idOrderPosition'] for op in self.client.get('/api/OrderPositions').json()]
        self.order_ids = [o['idOrder'] for o in self.client.get('/api/Orders').json()]
        self.order_status_ids = [os['idOrderStatus'] for os in self.client.get('/api/OrderStatus').json()]
        self.review_ids = [re['idReview'] for re in self.client.get('/api/Reviews').json()]
        self.role_ids = [ro['idRole'] for ro in self.client.get('/api/Roles').json()]
        self.user_ids = [u['idUser'] for u in self.client.get('/api/Users').json()]

    def book_data_func(self):
        return {
            'title': fake.sentence(nb_words=2)[:255],
            'authorId': random.choice(self.author_ids),
            'publishingHouseId': random.choice(self.house_ids),
            'genreId': random.choice(self.genre_ids),
            'isbn': fake.unique.isbn13(separator=""),
            'yearOfPublication': fake.random_int(min=1000, max=datetime.date.today().year),
            'price': round(fake.random_int(min=100, max=999999) / 100, 2),
            'quantity': fake.random_int(min=0, max=9999),
            'bookDescription': fake.paragraph(nb_sentences=3),
            'coverPath': None
        }

    @task(5)
    def get_books(self):
        self.client.get('/api/Books')
        self.client.get(f'/api/Books/{random.choice(self.book_ids)}')
    
    @task(1)
    def add_book(self):
        self.client.post('/api/Books', json=self.book_data_func())

    @task(1)
    def edit_book(self):
        book_id = random.choice(self.book_ids)
        data = self.book_data_func()
        data['idBook'] = book_id
        self.client.put(f'/api/Books/{book_id}', json=data)

    def author_data_func(self):
        return {
            'authorName': fake.first_name(),
            'authorSurname': fake.last_name(),
            'authorPatronymic': fake.middle_name(),
            'dateOfBirth': fake.date_of_birth(minimum_age=12).isoformat(),
            'countryId': random.choice(self.country_ids),
            'biography': fake.paragraph(nb_sentences=3),
        }

    @task(5)
    def get_authors(self):
        self.client.get('/api/Authors')
        self.client.get(f'/api/Authors/{random.choice(self.author_ids)}')
    
    @task(1)
    def add_author(self):
        self.client.post('/api/Authors', json=self.author_data_func())

    @task(1)
    def edit_author(self):
        author_id = random.choice(self.author_ids)
        data = self.author_data_func()
        data['idAuthor'] = author_id
        self.client.put(f'/api/Authors/{author_id}', json=data)

    @task(5)
    def get_carts(self):
        self.client.get('/api/Carts')
        self.client.get(f'/api/Carts/{random.choice(self.cart_ids)}')

    @task(5)
    def get_countries(self):
        self.client.get('/api/Countries')
        self.client.get(f'/api/Countries/{random.choice(self.country_ids)}')

    @task(5)
    def get_genres(self):
        self.client.get('/api/Genres')
        self.client.get(f'/api/Genres/{random.choice(self.genre_ids)}')

    @task(5)
    def get_notifications(self):
        self.client.get('/api/Notifications')
        self.client.get(f'/api/Notifications/{random.choice(self.notification_ids)}')

    @task(5)
    def get_order_positions(self):
        self.client.get('/api/OrderPositions')
        self.client.get(f'/api/OrderPositions/{random.choice(self.order_position_ids)}')

    @task(5)
    def get_orders(self):
        self.client.get('/api/Orders')
        self.client.get(f'/api/Orders/{random.choice(self.order_ids)}')

    @task(5)
    def get_order_status(self):
        self.client.get('/api/OrderStatus')
        self.client.get(f'/api/OrderStatus/{random.choice(self.order_status_ids)}')

    @task(5)
    def get_publishing_houses(self):
        self.client.get('/api/PublishingHouses')
        self.client.get(f'/api/PublishingHouses/{random.choice(self.house_ids)}')

    @task(5)
    def get_reviews(self):
        self.client.get('/api/Reviews')
        self.client.get(f'/api/Reviews/{random.choice(self.review_ids)}')

    @task(5)
    def get_roles(self):
        self.client.get('/api/Roles')
        self.client.get(f'/api/Roles/{random.choice(self.role_ids)}')

    @task(5)
    def get_users(self):
        self.client.get('/api/Users')
        self.client.get(f'/api/Users/{random.choice(self.user_ids)}')

class ManagerTests(TaskSet):
    def on_start(self):
        self.client.post('/api/Auth/login', json={
            'login': 'manager',
            'password': 'manager'
        })

        self.order_position_ids = [op['idOrderPosition'] for op in self.client.get('/api/OrderPositions').json()]
        self.order_ids = [o['idOrder'] for o in self.client.get('/api/Orders').json()]
        self.review_ids = [re['idReview'] for re in self.client.get('/api/Reviews').json()]
        self.role_ids = [ro['idRole'] for ro in self.client.get('/api/Roles').json()]
        self.user_ids = [u['idUser'] for u in self.client.get('/api/Users').json()]
        self.book_ids = [b['idBook'] for b in self.client.get('/api/Books').json()]

    def review_data_func(self):
        return {
            'bookId': random.choice(self.book_ids),
            'usersId': random.choice(self.user_ids),
            'rating': fake.random_int(min=1, max=5),
            'textReview': fake.paragraph(nb_sentences=2),
        }

    @task(1)
    def add_review(self):
        self.client.post('/api/Reviews', json=self.review_data_func())

    @task(1)
    def edit_review(self):
        review_id = random.choice(self.review_ids)
        data = self.review_data_func()
        data['idReview'] = review_id
        self.client.put(f'/api/Reviews/{review_id}', json=data)
    
    @task(5)
    def get_users(self):
        self.client.get('/api/Users')
        self.client.get(f'/api/Users/{random.choice(self.user_ids)}')

    @task(5)
    def get_roles(self):
        self.client.get('/api/Roles')
        self.client.get(f'/api/Roles/{random.choice(self.role_ids)}')
    
    @task(5)
    def get_reviews(self):
        self.client.get('/api/Reviews')
        self.client.get(f'/api/Reviews/{random.choice(self.review_ids)}')

    @task(5)
    def get_order_positions(self):
        self.client.get('/api/OrderPositions')
        self.client.get(f'/api/OrderPositions/{random.choice(self.order_position_ids)}')

    @task(5)
    def get_orders(self):
        self.client.get('/api/Orders')
        self.client.get(f'/api/Orders/{random.choice(self.order_ids)}')


class WebsiteUser(HttpUser):
    wait_time = constant_throughput(2)

    tasks = [
        AdminTests,
        ManagerTests
    ]