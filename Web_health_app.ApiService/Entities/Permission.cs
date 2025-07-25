using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Permission
{
    public string PermissionId { get; set; } = null!;

    public string PermissionName { get; set; } = null!;

    public string ActionId { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? TimeActiveId { get; set; }

    public string? RoleId { get; set; }

    public bool IsActive { get; set; }

    public virtual Action Action { get; set; } = null!;

    public virtual Entity Entity { get; set; } = null!;

    public virtual Role? Role { get; set; }

    public virtual TimeActive? TimeActive { get; set; }
}
