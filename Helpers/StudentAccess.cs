using EduConnect.Data;
using System.Security.Claims;

namespace EduConnect.Helpers
{
    public static class StudentAccess
    {
        public const string SelectedChildKey = "SelectedStudentId";

        // Returns the user ids of the students the signed-in user may see.
        // Returns null when the user is not restricted (teachers and admins).
        public static List<string>? VisibleStudentUserIds(
            EduConnectDbContext db,
            ClaimsPrincipal user,
            ISession session)
        {
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (user.IsInRole("Student"))
            {
                return new List<string> { userId };
            }

            if (user.IsInRole("Parent"))
            {
                List<string> children = db.Students
                    .Where(x => x.ParentId == userId)
                    .Select(x => x.UserId)
                    .ToList();

                // If the parent picked a child on the dashboard, only show that child.
                string? selected = session.GetString(SelectedChildKey);

                if (selected != null && children.Contains(selected))
                {
                    return new List<string> { selected };
                }

                return children;
            }

            return null;
        }

        // Classrooms that belong to the students in the list.
        public static List<int> ClassroomIds(EduConnectDbContext db, List<string> studentUserIds)
        {
            return db.Students
                .Where(x => studentUserIds.Contains(x.UserId))
                .Select(x => x.ClassroomId)
                .Distinct()
                .ToList();
        }
    }
}
