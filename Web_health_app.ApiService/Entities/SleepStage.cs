using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class SleepStage
{
    public string Id { get; set; } = null!;

    public string SleepSessionId { get; set; } = null!;

    public byte? Stage { get; set; }

    public DateTime? StartTime { get; set; }

    public byte? DurationMinutes { get; set; }

    public virtual SleepSession SleepSession { get; set; } = null!;
}
