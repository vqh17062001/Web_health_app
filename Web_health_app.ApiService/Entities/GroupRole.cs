using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class GroupRole
{
    public string GroupId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public string? Note { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
