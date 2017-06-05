using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HrApp.Controllers
{
    public class UserFunctionsController : Controller
    {
        private UserManager<AppUser> userManager;

        public UserFunctionsController(UserManager<AppUser> _userManager)
        {
            userManager = _userManager;
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View("ResetPassword");
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);

            if (model.Password.Length<8)
            {
                ViewBag.Message = "Password is too short";
            }
            else if (await userManager.CheckPasswordAsync(user,model.OldPassword))
            {
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Password changed!";
                }
                else
                {
                    ViewBag.Message = "Password change failed!";
                }
            }
            else
            {
                ViewBag.Message = "Incorrect password =o(";
            }



            return View("ResetPassword");
        }


    }



}
