using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class Author
{
    public int IdAuthor { get; set; }

    public string AuthorName { get; set; } = null!;

    public string AuthorSurname { get; set; } = null!;

    public string? AuthorPatronymic { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int CountryId { get; set; }

    public string? Biography { get; set; }

    [JsonIgnore]
    public virtual Country? Country { get; set; }
}
