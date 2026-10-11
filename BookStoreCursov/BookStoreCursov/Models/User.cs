using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookStoreCursov.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string UserLogin { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UsersName { get; set; } = null!;

    public string UsersSurname { get; set; } = null!;

    public string? UsersPatronymic { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? AvatarPath { get; set; }

    public int RoleId { get; set; }

    [JsonIgnore]
    public virtual Role? Role { get; set; }
}
