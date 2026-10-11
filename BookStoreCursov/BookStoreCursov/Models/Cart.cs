using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Cart
{
    public int IdCart { get; set; }

    public int UsersId { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; }

    public DateOnly AddDate { get; set; }

    [JsonIgnore]
    public virtual Book? Book { get; set; }

    [JsonIgnore]
    public virtual User? Users { get; set; }
}
