using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class LoginHistory
{
    public string LoginHisId { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? MacDevice { get; set; }

    public string? IpAddress { get; set; }

    public byte StatusLogin { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual User User { get; set; } = null!;
}
