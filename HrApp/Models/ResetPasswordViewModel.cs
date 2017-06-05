using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HrApp.Models
{
    public class ResetPasswordViewModel
    {
            [Required]
            [MinLength(8, ErrorMessage = "The password needs to be at least 8 characters long")]
            public string Password { get; set; }

            [Required]
            public string OldPassword { get; set; }
        
    }
}
