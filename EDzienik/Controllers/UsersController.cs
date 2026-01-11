using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using EDzienik.Models;

namespace EDzienik.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString, string roleFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRole"] = roleFilter;

            ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, roleFilter);

            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.Email.Contains(searchString) ||
                                                   u.FirstName.Contains(searchString) ||
                                                   u.LastName.Contains(searchString));
            }

            var users = await usersQuery.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

       
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    if (!roles.Contains(roleFilter))
                    {
                        continue; 
                    }
                }

                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                thisViewModel.Roles = roles;
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" });
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        if (!await _roleManager.RoleExistsAsync(model.Role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(model.Role));
                        }
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" });
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = userRoles.FirstOrDefault()
            };

            ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, model.Role);
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, model.Role);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var currentUserId = _userManager.GetUserId(User);
            var isSelf = currentUserId == user.Id;

            var currentRoles = await _userManager.GetRolesAsync(user);

            var wantsRoleChange =
                !string.IsNullOrEmpty(model.Role) &&
                !currentRoles.Any(r => string.Equals(r, model.Role, StringComparison.OrdinalIgnoreCase));

            if (isSelf && wantsRoleChange)
            {
                ModelState.AddModelError(string.Empty, "Nie możesz zmienić swojej roli.");
                ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, model.Role);
                return View(model);
            }

            if (wantsRoleChange)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            var result = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, model.Role);
                    return View(model);
                }
            }

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = new SelectList(new List<string> { "Student", "Teacher", "Admin" }, model.Role);
            return View(model);
        }

        // POST: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (_userManager.GetUserId(User) == user.Id)
                {
                    TempData["Error"] = "Nie możesz usunąć swojego konta.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    TempData["Success"] = "Usunięto użytkownika.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}