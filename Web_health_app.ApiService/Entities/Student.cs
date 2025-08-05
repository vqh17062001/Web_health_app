using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public short Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ManageBy { get; set; }

    public string? Department { get; set; }

    public virtual ICollection<AssessmentBatchStudent> AssessmentBatchStudents { get; set; } = new List<AssessmentBatchStudent>();

    public virtual ICollection<BodyMetric> BodyMetrics { get; set; } = new List<BodyMetric>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<DailyActivity> DailyActivities { get; set; } = new List<DailyActivity>();

    public virtual ICollection<Environment> Environments { get; set; } = new List<Environment>();

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public virtual User? ManageByNavigation { get; set; }

    public virtual ICollection<PhysiologicalMetric> PhysiologicalMetrics { get; set; } = new List<PhysiologicalMetric>();

    public virtual ICollection<SleepSession> SleepSessions { get; set; } = new List<SleepSession>();

    public string GetStudentStatusString()
    {
        return Status switch
        {
            1 => "Có dữ liệu đồng bộ với Atlas",
            10 => "offline",
            11 => "online",
            0 => "Do user tạo ra",
            _ => "Không xác định"
        };
    }
}
