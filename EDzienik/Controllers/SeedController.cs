//DO TESTÓW I ROZWOJU - KONTROLER DO SIEWU DANYCH POCZĄTKOWYCH
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EDzienik.Data;
using EDzienik.Entities;
using EDzienik.Entities.Enums; 
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Controllers
{
    public class SeedController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var schoolClass = await _context.SchoolClasses.FirstOrDefaultAsync(c => c.Name == "1A");
            if (schoolClass == null)
            {
           
                schoolClass = new SchoolClass { Name = "1A", SchoolYear = 2023 };
                _context.SchoolClasses.Add(schoolClass);
            }

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Informatyka");
            if (subject == null)
            {
                subject = new Subject { Name = "Informatyka" };
                _context.Subjects.Add(subject);
            }

            await _context.SaveChangesAsync();

            string teacherEmail = "nauczyciel@test.pl";
            var teacherUser = await _userManager.FindByEmailAsync(teacherEmail);

            if (teacherUser == null)
            {
                teacherUser = new User
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "Jan",
                    LastName = "Kowalski"
                };
                var result = await _userManager.CreateAsync(teacherUser, "Haslo123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(teacherUser, "Teacher");

                    var teacherEntity = new Teacher { UserId = teacherUser.Id };
                    _context.Teachers.Add(teacherEntity);
                    await _context.SaveChangesAsync();
                }
            }

            string studentEmail = "uczen@test.pl";
            var studentUser = await _userManager.FindByEmailAsync(studentEmail);

            if (studentUser == null)
            {
                studentUser = new User
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FirstName = "Piotr",
                    LastName = "Nowak"
                };
                var result = await _userManager.CreateAsync(studentUser, "Haslo123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(studentUser, "Student");

                    var studentEntity = new Student
                    {
                        UserId = studentUser.Id,
                        SchoolClassId = schoolClass.Id 
                    };
                    _context.Students.Add(studentEntity);
                    await _context.SaveChangesAsync();
                }
            }

            var dbTeacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.User.Email == teacherEmail);

            bool assignmentExists = await _context.SubjectAssignments.AnyAsync(sa =>
                sa.TeacherId == dbTeacher.Id &&
                sa.SubjectId == subject.Id &&
                sa.SchoolClassId == schoolClass.Id);

            if (!assignmentExists && dbTeacher != null)
            {
                var assignment = new SubjectAssignment
                {
                    TeacherId = dbTeacher.Id,
                    SubjectId = subject.Id,
                    SchoolClassId = schoolClass.Id
                };
                _context.SubjectAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }

            return Content($"Gotowe! \nZaloguj się jako Nauczyciel: {teacherEmail} / Haslo123! \nZaloguj się jako Uczeń: {studentEmail} / Haslo123!");
        }
    }
}