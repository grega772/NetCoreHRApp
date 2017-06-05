using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HrApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HrApp.Controllers
{
    public class AdminController : Controller
    {
        private Random random = new Random();
        private UserManager<AppUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private IUserValidator<AppUser> userValidator;
        private IPasswordValidator<AppUser> passwordValidator;
        private IPasswordHasher<AppUser> passwordHasher;
        private AppIdentityDbContext dbContext;


        public AdminController(UserManager<AppUser> _userManager,
                                IUserValidator<AppUser> userValid,
                                 AppIdentityDbContext _dbContext,
                                RoleManager<IdentityRole> _roleManager,
                                IPasswordValidator<AppUser> passValid,
                                IPasswordHasher<AppUser> passwordHash)
        {
            userManager = _userManager;
            userValidator = userValid;
            passwordValidator = passValid;
            passwordHasher = passwordHash;
            roleManager = _roleManager;
            dbContext = _dbContext;
        }



        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            ViewBag.Title = "Users List";
            return View(userManager.Users);
        }


        [Authorize(Roles = "Admin")]
        public ViewResult Create()
        {
            ViewBag.Title = "Create new user";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.FirstName.Substring(0, 1).ToLower() + model.LastName.ToLower() + (random.Next(100).ToString()),
                    FirstName=model.FirstName,
                    LastName=model.LastName,
                    JobTitle=model.JobTitle,
                    DateOfBirth=model.DateOfBirth,
                    Email = model.Email
                };
                
                IdentityResult result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");

                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

       [HttpPost]
       [Authorize(Roles = "Admin")]
        public async  Task<IActionResult> Delete(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null&&!user.Email.Equals("admin@1-stop.biz"))
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                     foreach(var usersUnder in dbContext.UnderRole.ToList())
                    {
                        if (usersUnder.UsersUnder!=null&&usersUnder.UsersUnder.Length>0)
                        {
                            List<String> roles = usersUnder.UsersUnder.Split(',').ToList();

                            if (roles.Contains(user.UserName))
                            {
                                roles.Remove(user.UserName);
                                StringBuilder sb = new StringBuilder();

                                foreach (var role in roles)
                                {
                                    sb.Append(role);
                                    sb.Append(',');
                                }
                                string usersList = sb.ToString();
                                if (usersList.Length>0)
                                {
                                    usersList = usersList.Substring(0, usersList.Length - 1);
                                }
                                usersUnder.UsersUnder = usersList;
                                dbContext.Update(usersUnder);
                                dbContext.SaveChanges();
                            }



                        }
                    }


                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found :(");
            }
            return View("Index", userManager.Users);
        }




        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }


        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user!=null)
            {
                ViewBag.Title = "Edit User";
                return View(user);
            }
            else
            {
                ViewBag.Title = "Users List";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(AppUser _user,string password)
        {

            AppUser user = await userManager.FindByIdAsync(_user.Id);
            if (user != null)
            {
                user.Email = _user.Email;
                IdentityResult validEmail = await userValidator.ValidateAsync(userManager, user);
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }
                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await passwordValidator.ValidateAsync(userManager, user, password);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }
                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded && password != string.Empty && validPass.Succeeded))
                {
                    user.FirstName = _user.FirstName;
                    user.LastName = _user.LastName;
                    user.JobTitle = _user.JobTitle;
                    user.DateOfBirth = _user.DateOfBirth;
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View();
        }


        [HttpGet]
        public async Task<ViewResult> ManageUsers()
        {

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);
            List<AppUser> users = new List<AppUser>();

            if (await userManager.IsInRoleAsync(user,"Admin"))
            {
                users = userManager.Users.ToList();
            }
            else if (await userManager.IsInRoleAsync(user,"User"))
            {

            }
            else
            {
                foreach (var role in roleManager.Roles.ToList())
                {
                    if(await userManager.IsInRoleAsync(user,role.Name))
                    {
                        ViewBag.Message = "User is in role other than user";
                        UsersUnderRole theUsers = dbContext.UnderRole.SingleOrDefault(e => e.RoleName.Equals(role.Name));

                        if (theUsers == null || theUsers.UsersUnder == null || theUsers.UsersUnder.Length == 0)
                        {
                            continue;
                        }
                        string usersUnder = theUsers.UsersUnder;
                        foreach (var thing in usersUnder.Split(',').ToList())
                        {

                            var finalUser = userManager.Users.SingleOrDefault(f => f.UserName.Equals(thing));
                            if (finalUser != null)
                            {
                                users.Add(finalUser);
                            }

                        }


                    }
                }
            }

            return View(users);
        }
    }
}
