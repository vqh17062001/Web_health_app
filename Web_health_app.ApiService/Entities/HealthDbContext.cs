using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Web_health_app.ApiService.Entities;

public partial class HealthDbContext : DbContext
{
    public HealthDbContext()
    {
    }

    public HealthDbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<AssessmentBatch> AssessmentBatches { get; set; }

    public virtual DbSet<AssessmentBatchStudent> AssessmentBatchStudents { get; set; }

    public virtual DbSet<AssessmentTest> AssessmentTests { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BodyMetric> BodyMetrics { get; set; }

    public virtual DbSet<DailyActivity> DailyActivities { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<Environment> Environments { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupRole> GroupRoles { get; set; }

    public virtual DbSet<LoginHistory> LoginHistorys { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PhysiologicalMetric> PhysiologicalMetrics { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleUser> RoleUsers { get; set; }

    public virtual DbSet<SleepSession> SleepSessions { get; set; }

    public virtual DbSet<SleepStage> SleepStages { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }

    public virtual DbSet<TimeActive> TimeActives { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=VO-QUOC-HUY-ZEP;Initial Catalog=HealthWebDB;User ID=sa;Password=Qnvn16062001@;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__ACTIONS__74E69F1F62E469D7");

            entity.ToTable("ACTIONS");

            entity.Property(e => e.ActionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action_ID");
            entity.Property(e => e.ActionName)
                .HasMaxLength(100)
                .HasColumnName("action_name");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
        });

        modelBuilder.Entity<AssessmentBatch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__Assessme__DBFF182980643B66");

            entity.ToTable("AssessmentBatch", tb => tb.HasTrigger("trg_batchID_before_insert"));

            entity.Property(e => e.BatchId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("batch_ID");
            entity.Property(e => e.CodeName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ManagerBy).HasColumnName("manager_by");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)0)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AssessmentBatchStudent>(entity =>
        {
            entity.HasKey(e => e.AbsId).HasName("PK__Assessme__0CDC908C3A23E92E");

            entity.ToTable("AssessmentBatchStudent", tb => tb.HasTrigger("trg_absID_before_insert"));

            entity.Property(e => e.AbsId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ABS_ID");
            entity.Property(e => e.BatchId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("batch_ID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");

            entity.HasOne(d => d.Batch).WithMany(p => p.AssessmentBatchStudents)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AssessmentBatchStudent_Batch");

            entity.HasOne(d => d.Student).WithMany(p => p.AssessmentBatchStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Assessmen__stude__2CF2ADDF");
        });

        modelBuilder.Entity<AssessmentTest>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.AbsId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ABS_ID");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("recorded_at");
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by");
            entity.Property(e => e.ResultValue)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("result_value");
            entity.Property(e => e.TesttypeId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("testtype_ID");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("unit");

            entity.HasOne(d => d.Abs).WithMany()
                .HasForeignKey(d => d.AbsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AssessmentTests_AssessmentBatchStudent");

            entity.HasOne(d => d.Testtype).WithMany()
                .HasForeignKey(d => d.TesttypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AssessmentTests_TestTypes");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AUDIT_LO__603E9CC0206DA37C");

            entity.ToTable("AUDIT_LOGs");

            entity.Property(e => e.AuditLogId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("audit_log_ID");
            entity.Property(e => e.DataAfter).HasColumnName("data_after");
            entity.Property(e => e.DataBefore).HasColumnName("data_before");
            entity.Property(e => e.EntityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("entity_ID");
            entity.Property(e => e.LoginHisId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("login_his_ID");
            entity.Property(e => e.TimeEnd)
                .HasColumnType("datetime")
                .HasColumnName("time_end");
            entity.Property(e => e.TimeStart)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("time_start");

            entity.HasOne(d => d.Entity).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AUDIT_Entity");

            entity.HasOne(d => d.LoginHis).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.LoginHisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AUDIT_LoginHistory");
        });

        modelBuilder.Entity<BodyMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BodyMetr__3214EC27A9176964");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.HeightCm)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("height_cm");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.TemperatureC)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("temperature_C");
            entity.Property(e => e.TimeStamp)
                .HasColumnType("datetime")
                .HasColumnName("time_stamp");
            entity.Property(e => e.WeightKg)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.Student).WithMany(p => p.BodyMetrics)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BodyMetrics_Students");
        });

        modelBuilder.Entity<DailyActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DailyAct__3214EC2776194638");

            entity.ToTable("DailyActivity");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.ActivityDate).HasColumnName("activity_date");
            entity.Property(e => e.ActivityMinutes)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("activity_minutes");
            entity.Property(e => e.Calories)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("calories");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Steps)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("steps");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Student).WithMany(p => p.DailyActivities)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyActivity_Students");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentCode).HasName("PK__Departme__EBC3495F3F5CB27A");

            entity.ToTable("Department", tb => tb.HasTrigger("trg_departmentCode_before_insert_safe"));

            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("department_code");
            entity.Property(e => e.Battalion)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("battalion");
            entity.Property(e => e.CharacterCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("character_code");
            entity.Property(e => e.Course)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("course");
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.EntityId).HasName("PK__ENTITY__AF9891AF3D997598");

            entity.ToTable("ENTITY");

            entity.Property(e => e.EntityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("entity_ID");
            entity.Property(e => e.LevelSecurity).HasColumnName("level_security");
            entity.Property(e => e.NameEntity)
                .HasMaxLength(100)
                .HasColumnName("name_entity");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Environment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Environm__3214EC27C4284534");

            entity.ToTable("Environment");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.HumidityRh)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("humidity_RH");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.TemperatureC)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("temperature_C");
            entity.Property(e => e.TimeStamp)
                .HasColumnType("datetime")
                .HasColumnName("time_stamp");
            entity.Property(e => e.UvIndexMwCm2)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("uv_index_mw_cm2");

            entity.HasOne(d => d.Student).WithMany(p => p.Environments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Environment_Students");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exercise__3214EC2737CE30D8");

            entity.ToTable("Exercise");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.Calories)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("calories");
            entity.Property(e => e.DistanceM)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("distance_m");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.Steps)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("steps");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Student).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exercise_Students");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__GROUPS__D5709198AAA5232E");

            entity.ToTable("GROUPS", tb => tb.HasTrigger("trg_groupID_before_insert"));

            entity.Property(e => e.GroupId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("group_ID");
            entity.Property(e => e.GroupName)
                .HasMaxLength(100)
                .HasColumnName("group_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.TimeActiveId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("time_active_ID");

            entity.HasOne(d => d.TimeActive).WithMany(p => p.Groups)
                .HasForeignKey(d => d.TimeActiveId)
                .HasConstraintName("FK_GROUPS_TimeActive");
        });

        modelBuilder.Entity<GroupRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("GROUP_ROLES");

            entity.Property(e => e.GroupId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("group_ID");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_ID");

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GROUPROLES_Groups");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GROUPROLES_Roles");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.LoginHisId).HasName("PK__LOGIN_HI__470472865763E137");

            entity.ToTable("LOGIN_HISTORYs");

            entity.Property(e => e.LoginHisId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("login_his_ID");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.LoginTime)
                .HasColumnType("datetime")
                .HasColumnName("login_time");
            entity.Property(e => e.LogoutTime)
                .HasColumnType("datetime")
                .HasColumnName("logout_time");
            entity.Property(e => e.MacDevice)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("mac_device");
            entity.Property(e => e.StatusLogin)
                .HasDefaultValue((byte)1)
                .HasColumnName("status_login");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LOGINH_Users");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__PERMISSI__E53067620495D364");

            entity.ToTable("PERMISSIONS", tb => tb.HasTrigger("trg_permissionsID_before_insert"));

            entity.Property(e => e.PermissionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("permission_ID");
            entity.Property(e => e.ActionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action_ID");
            entity.Property(e => e.EntityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("entity_ID");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .HasColumnName("permission_name");
            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_ID");
            entity.Property(e => e.TimeActiveId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("time_active_ID");

            entity.HasOne(d => d.Action).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PERMS_Actions");

            entity.HasOne(d => d.Entity).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PERMS_Entity");

            entity.HasOne(d => d.Role).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_PERMS_Roles");

            entity.HasOne(d => d.TimeActive).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.TimeActiveId)
                .HasConstraintName("FK_PERMS_TimeActive");
        });

        modelBuilder.Entity<PhysiologicalMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Physiolo__3214EC27E40999F8");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MeasuredAt)
                .HasColumnType("datetime")
                .HasColumnName("measured_at");
            entity.Property(e => e.MetricType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metric_type");
            entity.Property(e => e.MetricUnit)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("metric_unit");
            entity.Property(e => e.MetricValue)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metric_value");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");

            entity.HasOne(d => d.Student).WithMany(p => p.PhysiologicalMetrics)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhysiologicalMetrics_Students");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLES__760F99849CB45EDC");

            entity.ToTable("ROLES", tb => tb.HasTrigger("trg_roleID_before_insert"));

            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_ID");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<RoleUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ROLE_USERS");

            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_ID");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ROLEUSERS_Roles");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ROLEUSERS_Users");
        });

        modelBuilder.Entity<SleepSession>(entity =>
        {
            entity.HasKey(e => e.SleepSessionId).HasName("PK__SleepSes__9773C4E9150C09AB");

            entity.ToTable("SleepSession");

            entity.Property(e => e.SleepSessionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sleep_session_ID");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.TotalMinutes).HasColumnName("total_minutes");

            entity.HasOne(d => d.Student).WithMany(p => p.SleepSessions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SleepSession_Students");
        });

        modelBuilder.Entity<SleepStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SleepSta__3214EC27ACDDCB0F");

            entity.ToTable("SleepStage");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.SleepSessionId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sleep_session_ID");
            entity.Property(e => e.Stage).HasColumnName("stage");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");

            entity.HasOne(d => d.SleepSession).WithMany(p => p.SleepStages)
                .HasForeignKey(d => d.SleepSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SleepStage_Session");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__2A082B22619858A2");

            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Department).HasColumnName("department");
            entity.Property(e => e.Dob)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.ManageBy).HasColumnName("manage_by");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)1)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StudentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Students_CreatedBy");

            entity.HasOne(d => d.ManageByNavigation).WithMany(p => p.StudentManageByNavigations)
                .HasForeignKey(d => d.ManageBy)
                .HasConstraintName("FK_Students_ManageBy");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TesttypeId).HasName("PK__TestType__8544C8EAB948182B");

            entity.Property(e => e.TesttypeId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("testtype_ID");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<TimeActive>(entity =>
        {
            entity.HasKey(e => e.TimeActiveId).HasName("PK__TIME_ACT__5B6622816BBAEB19");

            entity.ToTable("TIME_ACTIVE");

            entity.Property(e => e.TimeActiveId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("time_active_ID");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.ScheduleDay).HasColumnName("schedule_day");
            entity.Property(e => e.ScheduleDayOfMonth).HasColumnName("schedule_day_of_month");
            entity.Property(e => e.ScheduleDayOfWeek).HasColumnName("schedule_day_of_week");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("user_ID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.GroupId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("group_ID");
            entity.Property(e => e.LevelSecurity)
                .HasDefaultValue((byte)1)
                .HasColumnName("level_security");
            entity.Property(e => e.ManageBy).HasColumnName("manage_by");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UserName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("user_name");
            entity.Property(e => e.UserStatus)
                .HasDefaultValue((byte)1)
                .HasColumnName("user_status");

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_USERS_Groups");

            entity.HasOne(d => d.ManageByNavigation).WithMany(p => p.InverseManageByNavigation)
                .HasForeignKey(d => d.ManageBy)
                .HasConstraintName("FK_USERS_Manager");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
