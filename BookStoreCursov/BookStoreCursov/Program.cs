using BookStoreCursov.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace BookStoreCursov
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped<IExcelService, ExcelService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddDbContext<BookStoreCursov.Models.BookStoreCursContext>(option =>
                option.UseSqlServer(builder.Configuration.GetConnectionString("con")));
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("AllowSpecific", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",    // Веб-приложение (Next.js)
                            "https://localhost:3000",
                            "http://10.0.2.2:5229",     // Android эмулятор
                            "http://Anastasia.local:5229",
                            "http://Anastasia:5229",
                            "http://26.141.109.202:5229",
                            "http://10.230.44.26:5229"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookStoreCursov.Models.BookStoreCursContext>();
                AppMetrics.OrdersCreatedTotal.Inc(db.Orders.Count());
                AppMetrics.ReviewsCreatedTotal.Inc(db.Reviews.Count());
                AppMetrics.BooksInCatalog.Set(db.Books.Count());
            }

            app.UseHttpMetrics();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowSpecific");

            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthorization();


            app.MapControllers();

            app.MapMetrics();

            app.Run();
        }
    }
}
