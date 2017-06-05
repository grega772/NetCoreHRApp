using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using HrApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HrApp.Controllers
{

    public class HomeController : Controller
    {
        private UserManager<AppUser> userManager;
        private IHostingEnvironment hostingEnv;
        private Random random;

        public HomeController(UserManager<AppUser> userMgr, IHostingEnvironment env)
        {
            userManager = userMgr;
            hostingEnv = env;
        }


        [Authorize]
        public async Task<ViewResult> Index()
        {

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);


            ViewBag.Title = "Profile";
            return View(user);
               
                
        }

        /* [Authorize(Roles = "User")]
         public IActionResult OtherAction() => View("Index", GetData(nameof(OtherAction)));

         private Dictionary<string, object> GetData(string actionName)
         {
             return new Dictionary<string, object>
             {

                 ["User"] = HttpContext.User,
                 ["Authentiated"] = HttpContext.User.Identity.IsAuthenticated,
                 ["Auth Type"] = HttpContext.User.Identity.AuthenticationType,
                 ["In Admin Role"] = HttpContext.User.IsInRole("Admin")
         };
         }*/


        [HttpPost]
        public async Task<IActionResult> UploadPicture(List<IFormFile> files)
        {
            ViewBag.Title = "Profile";
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);

            if (files.Count == 0)
            {
                ViewBag.FileMessage = "List doesn't contain files";
                return View("Index",user);
            }
            if (files.Count > 1)
            {
                ViewBag.FileMessage = "Do not upload more than one file";
                return View("Index",user);
            }

            long size = files.Sum(f => f.Length);
            if (size > 2000000)
            {
                ViewBag.FileMessage = "File size should not exceed 2MB";
                return View("Index",user);
            }

            String FileName = files.ToArray().First().FileName;

            if (!FileName.ToLower().Contains(".png") &&
                !FileName.ToLower().Contains(".jpeg") &&
                !FileName.ToLower().Contains(".jpg") &&
                !FileName.ToLower().Contains(".bmp"))
            {
                ViewBag.FileMessage = "Invalid File type, use an image";
                return View("Index", user);
            }

            IFormFile file = files.ToArray().First();

            //If user directory does not exist, create it
            var upload = Path.Combine(hostingEnv.WebRootPath, "images\\" + user.UserName);

            if (!Directory.Exists(upload))
            {
                Directory.CreateDirectory(upload);
            }

            //If Profile picture exists, delete it    
            if (user.ProfilePictureLink != null)
            {
                System.IO.File.Delete(user.ProfilePictureLink);
            }


            //Save file
            using (FileStream fs = System.IO.File.Create(upload + "\\" + file.FileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }


            user.ProfilePictureLink = upload + "\\" + file.FileName;

            await userManager.UpdateAsync(user);

            return View("Index",user);
        }



        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            ViewBag.Title = "Profile";
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);

            if (files.Count==0)
            {
                ViewBag.FileMessage = "No files selected";
                return View(user);
            }
            if (files.Count>1)
            {
                ViewBag.FileMessage = "Do not upload more than one file";
                return View(user);
            }

            long size = files.Sum(f => f.Length);
            if (size> 2000000)
            {
                ViewBag.FileMessage = "File size should not exceed 2MB";
                return View(user);
            }

            String FileName = files.ToArray().First().FileName;

            if (!FileName.ToLower().Contains(".doc")&&
                !FileName.ToLower().Contains(".pdf")&&
                !FileName.ToLower().Contains(".docx"))
            {
                ViewBag.FileMessage = "Invalid File type, use .doc, .docx or .pdf";
                return View(user);
            }



            IFormFile file = files.ToArray().First();

            var upload = Path.Combine(hostingEnv.WebRootPath, "Documents\\"+user.UserName);

              if (!Directory.Exists(upload))
            {
                Directory.CreateDirectory(upload);
            }

            if (user.DocumentLink!=null)
            {
                System.IO.File.Delete(user.DocumentLink);
            }



            using (FileStream fs = System.IO.File.Create(upload + "\\" + file.FileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            user.DocumentLink = upload+"\\"+file.FileName;

            await userManager.UpdateAsync(user);

            return View(user);
        }



        public async Task<FileResult> DownloadFile()
        {

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser user = await userManager.FindByIdAsync(userId);
            byte[] fileBytes = System.IO.File.ReadAllBytes(user.DocumentLink);
            String fileName = user.DocumentLink.Split('\\').Last();

            return File(fileBytes, "application/x-msdownload",fileName);
        }


        [HttpPost]
        public async Task<FileResult> DownloadUserCv(String id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            byte[] fileBytes = System.IO.File.ReadAllBytes(user.DocumentLink);
            String fileName = user.DocumentLink.Split('\\').Last();


            return File(fileBytes, "application/x-msdownload", fileName);
        }




    }
}