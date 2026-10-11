using System;
using System.Collections.Generic;

namespace BookStoreCursov.Models;

public partial class Genre
{
    public int IdGenre { get; set; }

    public string Title { get; set; } = null!;

    public string? GenreDescription { get; set; }
}
