using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int UsersId { get; set; }

    public string NotificationText { get; set; } = null!;

    public DateOnly AddDate { get; set; }

    public bool StatusN { get; set; }

    [JsonIgnore]
    public virtual User? Users { get; set; }
}
