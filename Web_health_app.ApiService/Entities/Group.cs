using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Group
{
    public string GroupId { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string? TimeActiveId { get; set; }

    public bool IsActive { get; set; }

    public virtual TimeActive? TimeActive { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
