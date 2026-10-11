using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Review
{
    public int IdReview { get; set; }

    public int BookId { get; set; }

    public int UsersId { get; set; }

    public int Rating { get; set; }

    public string TextReview { get; set; } = null!;

    public DateOnly AddDate { get; set; }

    [JsonIgnore]
    public virtual Book? Book { get; set; }

    [JsonIgnore]
    public virtual User? Users { get; set; }
}
