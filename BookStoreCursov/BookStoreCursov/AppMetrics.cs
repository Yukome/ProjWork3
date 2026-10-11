using Prometheus;

namespace BookStoreCursov
{
    public static class AppMetrics
    {
        public static readonly Counter OrdersCreatedTotal = Metrics
            .CreateCounter("app_orders_created_total", "Всего созданных заказов за всё время");

        public static readonly Counter ReviewsCreatedTotal = Metrics
            .CreateCounter("app_reviews_created_total", "Всего созданных отзывов за всё время");

        public static readonly Gauge BooksInCatalog = Metrics
            .CreateGauge("app_books_in_catalog", "Текущее количество книг в каталоге");
    }
}