using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NG_Core_Auth.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name ="UserName")]
        public string UserName { get; set; }
        [Required]
        [Display(Name ="Password")]
        public string Password { get; set; }

    }
}
