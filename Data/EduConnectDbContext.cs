using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Data
{
    public class EduConnectDbContext : DbContext
    {
        public EduConnectDbContext(DbContextOptions<EduConnectDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<TimetableEntry> TimetableEntries { get; set; }
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<TestMark> TestMarks { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementRsvp> AnnouncementRsvps { get; set; }
        public DbSet<ReportCard> ReportCards { get; set; }
        public DbSet<PopiaAuditLog> PopiaAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<Student>().HasIndex(x => x.StudentNumber).IsUnique();
            modelBuilder.Entity<Subject>().HasIndex(x => x.SubjectCode).IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Teacher>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Parent>()
                .HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Parent>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Student>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(x => x.Parent)
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(x => x.Classroom)
                .WithMany()
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Classroom>()
                .HasOne(x => x.ClassTeacher)
                .WithMany()
                .HasForeignKey(x => x.ClassTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnnouncementRsvp>()
                .HasIndex(x => new { x.AnnouncementId, x.ParentId })
                .IsUnique();
        }
    }
}
