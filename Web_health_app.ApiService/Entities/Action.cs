using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Action
{
    public string ActionId { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
