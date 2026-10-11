using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Book
{
    public int IdBook { get; set; }

    public string Title { get; set; } = null!;

    public int AuthorId { get; set; }

    public int PublishingHouseId { get; set; }

    public int GenreId { get; set; }

    public string Isbn { get; set; } = null!;

    public int YearOfPublication { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string BookDescription { get; set; } = null!;

    public string? CoverPath { get; set; }

    public DateOnly AddDate { get; set; }

    [JsonIgnore]
    public virtual Author? Author { get; set; }

    [JsonIgnore]
    public virtual Genre? Genre { get; set; }

    [JsonIgnore]
    public virtual PublishingHouse? PublishingHouse { get; set; }
}
