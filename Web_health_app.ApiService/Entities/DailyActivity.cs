using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class DailyActivity
{
    public string Id { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateOnly ActivityDate { get; set; }

    public string? ActivityMinutes { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string? Calories { get; set; }

    public string? Steps { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;
}
