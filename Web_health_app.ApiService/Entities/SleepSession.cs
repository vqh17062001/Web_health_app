using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class SleepSession
{
    public string SleepSessionId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public byte? TotalMinutes { get; set; }

    public virtual ICollection<SleepStage> SleepStages { get; set; } = new List<SleepStage>();

    public virtual Student Student { get; set; } = null!;
}
