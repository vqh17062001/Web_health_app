using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class RoleUser
{
    public string RoleId { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
