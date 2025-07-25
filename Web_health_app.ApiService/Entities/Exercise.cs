using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Exercise
{
    public string Id { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public byte? DurationMinutes { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string? Calories { get; set; }

    public string? DistanceM { get; set; }

    public string? Steps { get; set; }

    public virtual Student Student { get; set; } = null!;
}
