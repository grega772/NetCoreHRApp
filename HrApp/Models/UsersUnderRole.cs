using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace HrApp.Models
{
    public class UsersUnderRole 
    {
        public int Id { get; set; }

        public string RoleName { get; set; }

        //usernames stored in delimitered string
        public string UsersUnder { get; set; }

    }
}
