using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int UsersId { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateOnly AddDate { get; set; }

    public decimal TotalSum { get; set; }

    public int OrderStatusId { get; set; }

    [JsonIgnore]
    public virtual OrderStatus? OrderStatus { get; set; }

    [JsonIgnore]
    public virtual User? Users { get; set; }
}
