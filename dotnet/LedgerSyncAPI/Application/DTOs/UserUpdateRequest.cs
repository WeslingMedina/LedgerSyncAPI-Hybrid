using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UserUpdateRequest
    {
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? About { get; set; }

        public string? Country { get; set; }
    }
}
