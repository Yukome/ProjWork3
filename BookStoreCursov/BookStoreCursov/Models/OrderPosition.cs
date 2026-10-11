using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class OrderPosition
{
    public int IdOrderPosition { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; }

    public decimal PurchasePrice { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public virtual Book? Book { get; set; }

    [JsonIgnore]
    public virtual Order? Order { get; set; }
}
