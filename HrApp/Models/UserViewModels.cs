using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HrApp.Models
{
    public class CreateModel
    {
        [Required]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Can only contain letters")]
        public string FirstName { get; set; }
        [Required]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Can only contain letters")]
        public string LastName { get; set; }
        [Required]
        public string JobTitle { get; set; }
        [Required]
        [RegularExpression("[a-z A-Z 0-9]+@1-stop.biz",ErrorMessage ="This doesn't seem like a 1-Stop email address")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "The Password needs to be at least 8 characters long")]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }


    }
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }

    public class RoleEditModel
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }

    public class UserUnderRolesViewModel
    {
        public IdentityRole Role { get; set; }
        public List<AppUser> Members { get; set; }
        public List<AppUser> NonMembers { get; set; }
    }

}
