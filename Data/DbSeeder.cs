using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Data
{
    public static class DbSeeder
    {
        public static void Seed(EduConnectDbContext db)
        {
            db.Database.EnsureCreated();

            User admin = GetOrCreateUser(
                db,
                "admin@educonnect.local",
                "EduConnect Administrator",
                "0100000000",
                "Admin",
                "Admin123!");

            User teacher = GetOrCreateUser(
                db,
                "teacher@educonnect.local",
                "Demo Teacher",
                "0110000000",
                "Teacher",
                "Demo123!");

            User parent = GetOrCreateUser(
                db,
                "parent@educonnect.local",
                "Demo Parent",
                "0120000000",
                "Parent",
                "Demo123!");

            User student = GetOrCreateUser(
                db,
                "student@educonnect.local",
                "Demo Student",
                "0130000000",
                "Student",
                "Demo123!");

            db.SaveChanges();

            Teacher? teacherProfile = db.Teachers
                .FirstOrDefault(x => x.UserId == teacher.UserId);

            if (teacherProfile == null)
            {
                teacherProfile = new Teacher
                {
                    StaffNumber = "T001",
                    UserId = teacher.UserId,
                    SubjectSpecialization = "Mathematics"
                };

                db.Teachers.Add(teacherProfile);
                db.SaveChanges();
            }

            Parent? parentProfile = db.Parents
                .FirstOrDefault(x => x.UserId == parent.UserId);

            if (parentProfile == null)
            {
                parentProfile = new Parent
                {
                    UserId = parent.UserId,
                    Preference = "Both",
                    ResidentialAddress = "Demo address"
                };

                db.Parents.Add(parentProfile);
                db.SaveChanges();
            }

            Subject? mathematics = db.Subjects
                .FirstOrDefault(x => x.SubjectCode == "MATH");

            if (mathematics == null)
            {
                mathematics = new Subject
                {
                    SubjectName = "Mathematics",
                    SubjectCode = "MATH"
                };

                db.Subjects.Add(mathematics);
                db.SaveChanges();
            }

            Subject? english = db.Subjects
                .FirstOrDefault(x => x.SubjectCode == "ENG");

            if (english == null)
            {
                english = new Subject
                {
                    SubjectName = "English",
                    SubjectCode = "ENG"
                };

                db.Subjects.Add(english);
                db.SaveChanges();
            }

            Classroom? classroom = db.Classrooms
                .FirstOrDefault(x => x.Name == "Grade 8A");

            if (classroom == null)
            {
                classroom = new Classroom
                {
                    Name = "Grade 8A",
                    GradeLevel = 8,
                    AcademicYear = DateTime.Now.Year.ToString(),
                    ClassTeacherId = teacherProfile.StaffNumber
                };

                db.Classrooms.Add(classroom);
                db.SaveChanges();
            }

            Student? studentProfile = db.Students
                .FirstOrDefault(x => x.UserId == student.UserId);

            if (studentProfile == null)
            {
                studentProfile = new Student
                {
                    StudentNumber = "S001",
                    UserId = student.UserId,
                    GradeLevel = 8,
                    ClassroomId = classroom.ClassroomId,
                    ParentId = parentProfile.UserId
                };

                db.Students.Add(studentProfile);
                db.SaveChanges();
            }

            if (!db.TimetableEntries.Any(x =>
                x.ClassroomId == classroom.ClassroomId &&
                x.SubjectId == mathematics.SubjectId &&
                x.Day == "Monday"))
            {
                db.TimetableEntries.Add(new TimetableEntry
                {
                    ClassroomId = classroom.ClassroomId,
                    SubjectId = mathematics.SubjectId,
                    TeacherId = teacherProfile.StaffNumber,
                    Day = "Monday",
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(9, 0, 0),
                    RoomNumber = "A1"
                });
            }

            if (!db.TimetableEntries.Any(x =>
                x.ClassroomId == classroom.ClassroomId &&
                x.SubjectId == english.SubjectId &&
                x.Day == "Tuesday"))
            {
                db.TimetableEntries.Add(new TimetableEntry
                {
                    ClassroomId = classroom.ClassroomId,
                    SubjectId = english.SubjectId,
                    TeacherId = teacherProfile.StaffNumber,
                    Day = "Tuesday",
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 0, 0),
                    RoomNumber = "A1"
                });
            }

            if (!db.TestMarks.Any(x =>
                x.StudentId == studentProfile.UserId &&
                x.SubjectId == mathematics.SubjectId))
            {
                db.TestMarks.Add(new TestMark
                {
                    StudentId = studentProfile.UserId,
                    SubjectId = mathematics.SubjectId,
                    Score = 82,
                    MaxScore = 100,
                    AssessmentDate = DateTime.Now.AddDays(-7)
                });
            }

            if (!db.TestMarks.Any(x =>
                x.StudentId == studentProfile.UserId &&
                x.SubjectId == english.SubjectId))
            {
                db.TestMarks.Add(new TestMark
                {
                    StudentId = studentProfile.UserId,
                    SubjectId = english.SubjectId,
                    Score = 76,
                    MaxScore = 100,
                    AssessmentDate = DateTime.Now.AddDays(-3)
                });
            }

            if (!db.AttendanceRecords.Any(x => x.StudentId == studentProfile.UserId))
            {
                db.AttendanceRecords.AddRange(
                    new AttendanceRecord
                    {
                        StudentId = studentProfile.UserId,
                        Date = DateTime.Now.Date.AddDays(-1),
                        Status = "Present"
                    },
                    new AttendanceRecord
                    {
                        StudentId = studentProfile.UserId,
                        Date = DateTime.Now.Date.AddDays(-2),
                        Status = "Absent",
                        ReasonForAbsence = "Sick"
                    });
            }

            if (!db.Homeworks.Any(x =>
                x.ClassroomId == classroom.ClassroomId &&
                x.SubjectId == mathematics.SubjectId &&
                x.Title == "Algebra Practice"))
            {
                db.Homeworks.Add(new Homework
                {
                    ClassroomId = classroom.ClassroomId,
                    SubjectId = mathematics.SubjectId,
                    TeacherId = teacherProfile.UserId,
                    Title = "Algebra Practice",
                    DueDate = DateTime.Now.AddDays(5)
                });
            }

            if (!db.CourseMaterials.Any(x =>
                x.SubjectId == mathematics.SubjectId &&
                x.Title == "Algebra Notes"))
            {
                db.CourseMaterials.Add(new CourseMaterial
                {
                    SubjectId = mathematics.SubjectId,
                    TeacherId = teacherProfile.UserId,
                    Title = "Algebra Notes",
                    Description = "Class notes for the algebra section.",
                    FilePath = "algebra-notes.pdf"
                });
            }

            if (!db.Announcements.Any(x => x.Title == "Parent Meeting"))
            {
                db.Announcements.Add(new Announcement
                {
                    TeacherId = teacherProfile.UserId,
                    Title = "Parent Meeting",
                    Content = "Parent meeting in the school hall.",
                    DatePosted = DateTime.Now,
                    IsMeeting = true,
                    SendSms = true
                });
            }

            db.SaveChanges();
        }

        private static User GetOrCreateUser(
            EduConnectDbContext db,
            string email,
            string fullName,
            string phoneNumber,
            string role,
            string password)
        {
            User? user = db.Users.FirstOrDefault(x => x.Email == email);

            // Only create the demo account when it does not exist yet. Existing users
            // are left alone so restarting the app does not undo changes to them.
            if (user == null)
            {
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Role = role,
                    IsActive = true
                };

                user.SetPassword(password);
                db.Users.Add(user);
            }

            return user;
        }
    }
}
