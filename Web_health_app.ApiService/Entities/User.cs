using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Department { get; set; }

    public byte UserStatus { get; set; }

    public Guid? ManageBy { get; set; }

    public byte LevelSecurity { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? GroupId { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<User> InverseManageByNavigation { get; set; } = new List<User>();

    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    public virtual User? ManageByNavigation { get; set; }

    public virtual ICollection<Student> StudentCreatedByNavigations { get; set; } = new List<Student>();

    public virtual ICollection<Student> StudentManageByNavigations { get; set; } = new List<Student>();
}
