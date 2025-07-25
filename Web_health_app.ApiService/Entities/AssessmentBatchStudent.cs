using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class AssessmentBatchStudent
{
    public string AbsId { get; set; } = null!;

    public string? StudentId { get; set; }

    public string? BatchId { get; set; }

    public virtual AssessmentBatch? Batch { get; set; }

    public virtual Student? Student { get; set; }
}
