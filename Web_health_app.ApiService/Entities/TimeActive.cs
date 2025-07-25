using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class TimeActive
{
    public string TimeActiveId { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public byte? ScheduleDayOfWeek { get; set; }

    public byte? ScheduleDayOfMonth { get; set; }

    public DateOnly? ScheduleDay { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
