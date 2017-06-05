using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HrApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HrApp.Controllers
{
    [Authorize]
    public class RoleAdminController : Controller
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AppUser> userManager;
        private AppIdentityDbContext dbContext;

        public RoleAdminController(RoleManager<IdentityRole> roleMgr, UserManager<AppUser> userMgr, AppIdentityDbContext _context)
        {
            roleManager = roleMgr;
            userManager = userMgr;
            dbContext = _context;
        }

        // GET: /<controller>/
        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public ViewResult Index()
        {
            ViewBag.Title = "Roles List";
            List<IdentityRole> roles = roleManager.Roles.ToList();
            return View(roles);
        }


        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public IActionResult Create()
        {

            ViewBag.Title = "Create new Role";
            return View();
         }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Required]string name)
        {
            if (roleManager.Roles.Any(e=>e.Name==name))
            {

                ViewBag.ErrorMessage = "This role already exists";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    UsersUnderRole usersUnder = new UsersUnderRole { RoleName = name };
                    await dbContext.UnderRole.AddAsync(usersUnder);
                    await dbContext.SaveChangesAsync();
                    return RedirectToAction("Index", roleManager.Roles.ToList());
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }

            return View(name);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null&&role.Name!="Admin")
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {

                    UsersUnderRole underRole = dbContext.UnderRole.SingleOrDefault(e => e.RoleName.Equals(role.Name));

                    dbContext.UnderRole.Remove(underRole);

                    return RedirectToAction("Index", roleManager.Roles.ToList());
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "No Role found");
            }
            return View("Index", roleManager.Roles.ToList());

        }


        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }


        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();

            foreach (AppUser user in userManager.Users.ToList())
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            ViewBag.Title = "Manage Role";
            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }

        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddUsersUnderRole(RoleModificationModel model)
        {
            IdentityResult result;
            List<String> usersUnderRole = new List<String>();

            UsersUnderRole underRole = dbContext.UnderRole.SingleOrDefault(e=>e.RoleName.Equals(model.RoleName));
            if (underRole==null)
            {
                return View("Index",roleManager.Roles.ToList());
            }

            if (underRole.UsersUnder!=null)
            {
                var currentUsers = underRole.UsersUnder.Split(',');

                foreach (var user in currentUsers)
                {
                    usersUnderRole.Add(user);
                }
            }

                foreach (string userId in model.IdsToAdd ?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        if (usersUnderRole.Contains(user.UserName))
                        {
                            continue;
                        }
                        else
                        {
                        usersUnderRole.Add(user.UserName);
                        }
                    }
                }

                foreach (string userId in model.IdsToDelete ?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                    if (usersUnderRole.Contains(user.UserName))
                    {
                        usersUnderRole.Remove(user.UserName);
                    }
                }
                }

            StringBuilder sb = new StringBuilder();

            foreach (var user in usersUnderRole)
            {
                sb.Append(user);
                sb.Append(",");
            }

            if (sb.ToString().Length>0)
            {
                underRole.UsersUnder = sb.ToString().Substring(0, sb.ToString().Length - 1);
            }
            else
            {
                underRole.UsersUnder = sb.ToString();
            }
            

            dbContext.UnderRole.Update(underRole);
            await dbContext.SaveChangesAsync();


            return View("Index", roleManager.Roles.ToList());
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddUsersUnderRole(string id)
        {
            string[] userNames = null;
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();


            var tempUser = dbContext.UnderRole.SingleOrDefault(e => e.RoleName.Equals(role.Name));

            if (tempUser == null)
            {
                List<IdentityRole> roles = roleManager.Roles.ToList();
                return View("index", roles);
            }
                
            if( tempUser.UsersUnder!=null)
            {
                userNames = tempUser.UsersUnder.Split(',');
            }

            

            if (userNames!=null)
            {
                foreach (var userName in userNames)
                {
                    AppUser user = userManager.Users.SingleOrDefault(e => e.UserName.Equals(userName));
                    if (user != null)
                    {
                        members.Add(user);
                    }
                }
            }
            foreach (var user in userManager.Users)
            {
                if (!members.Contains(user))
                {
                    nonMembers.Add(user);
                }
            }
            ViewBag.Title = "Add users under role";
            return View("AddUsersUnderRole",new UserUnderRolesViewModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }




        [HttpPost]
        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.IdsToAdd ?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                        {
                            AddErrorsFromResult(result);
                        }
                        await userManager.RemoveFromRoleAsync(user, "User");
                    }
                }
                foreach (string userId in model.IdsToDelete ?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                        {
                            AddErrorsFromResult(result);
                        }
                    }
                }
            }
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return await Edit(model.RoleId);
            }
        }
    }
}

















