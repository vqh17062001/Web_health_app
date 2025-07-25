using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Role
{
    public string RoleId { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
