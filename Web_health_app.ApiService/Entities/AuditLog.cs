using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class AuditLog
{
    public string AuditLogId { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string LoginHisId { get; set; } = null!;

    public DateTime TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public string? DataBefore { get; set; }

    public string? DataAfter { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual LoginHistory LoginHis { get; set; } = null!;
}
