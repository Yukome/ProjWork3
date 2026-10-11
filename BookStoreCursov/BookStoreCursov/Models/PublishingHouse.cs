using System;
using System.Collections.Generic;

namespace BookStoreCursov.Models;

public partial class PublishingHouse
{
    public int IdPublishingHouse { get; set; }

    public string Title { get; set; } = null!;

    public string PublishingHouseAddress { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }
}
