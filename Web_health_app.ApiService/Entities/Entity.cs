using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Entity
{
    public string EntityId { get; set; } = null!;

    public string NameEntity { get; set; } = null!;

    public byte LevelSecurity { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
