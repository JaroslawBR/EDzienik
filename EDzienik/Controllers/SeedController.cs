using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EDzienik.Data;
using EDzienik.Entities;
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
            // 1. ROLA
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. ADMINISTRATORZY
            var admins = new List<(string Email, string Imie, string Nazwisko)>
            {
                ("admin@edziennik.pl", "Admin", "Główny"),
                ("dyrektor@edziennik.pl", "Jan", "Dyrektor")
            };

            foreach (var a in admins)
            {
                if (await _userManager.FindByEmailAsync(a.Email) == null)
                {
                    var user = new User { UserName = a.Email, Email = a.Email, FirstName = a.Imie, LastName = a.Nazwisko, EmailConfirmed = true };
                    var res = await _userManager.CreateAsync(user, "Haslo123!");
                    if (res.Succeeded) await _userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // 3. KLASY
            var classNames = new[] { "1A", "2B", "3C" };
            var classes = new List<SchoolClass>();

            foreach (var name in classNames)
            {
                var cls = await _context.SchoolClasses.FirstOrDefaultAsync(c => c.Name == name);
                if (cls == null)
                {
                    cls = new SchoolClass { Name = name, SchoolYear = 2025 };
                    _context.SchoolClasses.Add(cls);
                    await _context.SaveChangesAsync();
                }
                classes.Add(cls);
            }

            // 4. PRZEDMIOTY
            var subjectNames = new[] { "Matematyka", "Język Polski", "Język Angielski", "Historia", "Fizyka", "Informatyka" };
            var subjects = new List<Subject>();

            foreach (var name in subjectNames)
            {
                var sub = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == name);
                if (sub == null)
                {
                    sub = new Subject { Name = name };
                    _context.Subjects.Add(sub);
                    await _context.SaveChangesAsync();
                }
                subjects.Add(sub);
            }

            // 5. NAUCZYCIELE
            var teachersData = new List<(string Email, string Imie, string Nazwisko)>
            {
                ("nauczyciel1@edziennik.pl", "Anna", "Matematyczna"),
                ("nauczyciel2@edziennik.pl", "Piotr", "Humanista"),
                ("nauczyciel3@edziennik.pl", "Ewa", "Angielska"),
                ("nauczyciel4@edziennik.pl", "Tomasz", "Techniczny")
            };

            var teachers = new List<Teacher>();

            foreach (var tData in teachersData)
            {
                var user = await _userManager.FindByEmailAsync(tData.Email);
                if (user == null)
                {
                    user = new User { UserName = tData.Email, Email = tData.Email, FirstName = tData.Imie, LastName = tData.Nazwisko, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user, "Haslo123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Teacher");
                        var teacherEntity = new Teacher { UserId = user.Id };
                        _context.Teachers.Add(teacherEntity);
                        await _context.SaveChangesAsync();
                        teachers.Add(teacherEntity);
                    }
                }
                else
                {
                    var existing = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
                    if (existing != null) teachers.Add(existing);
                }
            }

            // 6. PRZYPISANIA (SubjectAssignments)
            var teacherToSubjectIndices = new List<(int TIdx, int SIdx)>
            {
                (0, 0), // Nauczyciel 1 - Matematyka
                (0, 4), // Nauczyciel 1 - Fizyka
                (1, 1), // Nauczyciel 2 - Polski
                (1, 3), // Nauczyciel 2 - Historia
                (2, 2), // Nauczyciel 3 - Angielski
                (3, 5)  // Nauczyciel 4 - Informatyka
            };

            if (teachers.Count >= 4 && subjects.Count >= 6)
            {
                foreach (var schoolClass in classes)
                {
                    foreach (var map in teacherToSubjectIndices)
                    {
                        var teacher = teachers[map.TIdx];
                        var subject = subjects[map.SIdx];

                        if (!await _context.SubjectAssignments.AnyAsync(sa => sa.SchoolClassId == schoolClass.Id && sa.SubjectId == subject.Id))
                        {
                            _context.SubjectAssignments.Add(new SubjectAssignment
                            {
                                SchoolClassId = schoolClass.Id,
                                TeacherId = teacher.Id,
                                SubjectId = subject.Id
                            });
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            // 7. UCZNIOWIE
            int studentsTotal = 24;
            var createdStudents = new List<Student>();
            int currentClassIndex = 0;

            for (int i = 1; i <= studentsTotal; i++)
            {
                string email = $"uczen{i}@edziennik.pl";
                var schoolClass = classes[currentClassIndex];
                currentClassIndex = (currentClassIndex + 1) % classes.Count;

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User { UserName = email, Email = email, FirstName = "Uczeń", LastName = $"Nr {i}", EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user, "Haslo123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Student");
                        var studentEntity = new Student { UserId = user.Id, SchoolClassId = schoolClass.Id };
                        _context.Students.Add(studentEntity);
                        await _context.SaveChangesAsync();
                        createdStudents.Add(studentEntity);
                    }
                }
                else
                {
                    var existing = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (existing != null) createdStudents.Add(existing);
                }
            }

            // 8. OCENY
            var rand = new Random();
            string[] descriptions = { "Sprawdzian", "Kartkówka", "Odpowiedź ustna", "Aktywność", "Praca domowa" };

            // Mapa przedmiot -> nauczyciel
            var subjectTeacherMap = new Dictionary<int, int>();
            foreach (var map in teacherToSubjectIndices)
            {
                if (map.SIdx < subjects.Count && map.TIdx < teachers.Count)
                {
                    int sId = subjects[map.SIdx].Id;
                    int tId = teachers[map.TIdx].Id;
                    if (!subjectTeacherMap.ContainsKey(sId)) subjectTeacherMap.Add(sId, tId);
                }
            }

            foreach (var student in createdStudents)
            {
                if (!await _context.Grades.AnyAsync(g => g.StudentId == student.Id))
                {
                    foreach (var kvp in subjectTeacherMap)
                    {
                        // Losuj 2-5 ocen
                        int count = rand.Next(2, 6);
                        for (int g = 0; g < count; g++)
                        {
                            _context.Grades.Add(new Grade
                            {
                                StudentId = student.Id,
                                SubjectId = kvp.Key,
                                TeacherId = kvp.Value,
                                Value = rand.Next(1, 7),
                                Description = descriptions[rand.Next(descriptions.Length)],
                                CreatedUnix = DateTimeOffset.UtcNow.AddDays(-rand.Next(1, 60)).ToUnixTimeSeconds()
                            });
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();

            // ==========================================================
            // ZWRACANIE WIDOCZNYCH DANYCH LOGOWANIA
            // ==========================================================
            return Content(
                $"✅ GENEROWANIE DANYCH ZAKOŃCZONE!\n" +
                $"============================================================\n" +
                $"🔑 HASŁO DO WSZYSTKICH KONT: Haslo123!\n" +
                $"============================================================\n\n" +

                $"👤 ADMIN:\n" +
                $"   Login: admin@edziennik.pl\n\n" +

                $"🎓 NAUCZYCIELE:\n" +
                $"   1. nauczyciel1@edziennik.pl (Matematyka, Fizyka)\n" +
                $"   2. nauczyciel2@edziennik.pl (Polski, Historia)\n" +
                $"   3. nauczyciel3@edziennik.pl (Angielski)\n" +
                $"   4. nauczyciel4@edziennik.pl (Informatyka)\n\n" +

                $"🧑‍🎓 UCZNIOWIE (przykładowi):\n" +
                $"   - uczen1@edziennik.pl\n" +
                $"   - uczen2@edziennik.pl\n" +
                $"   - ...\n" +
                $"   - uczen24@edziennik.pl\n" +
                $"   (łącznie 24 uczniów, hasło to samo)\n\n" +

                $"📊 STATYSTYKI:\n" +
                $"   - Klasy: 3 (1A, 2B, 3C)\n" +
                $"   - Przedmioty: 6\n" +
                $"   - Oceny: Wygenerowano losowe oceny w bazie."
            );
        }
    }
}