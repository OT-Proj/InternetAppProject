using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace InternetAppProject.Models
{
    public class Drive
    {

        public int Id { get; set; }

        [Display(Name = "Username")]
        public User UserId { get; set; }

        public IEnumerable<Image> Images { get; set; }

        [Display(Name = "Current usage")]
        [Range(0, 9999, ErrorMessage = "Invalid Current usage - must be between 0 and 9999")]
        public int Current_usage { get; set; }

        [Display(Name = "Business plan")]
        public DriveType TypeId { get; set; }

        [StringLength(60, ErrorMessage = "Must be between under 50 charcters", MinimumLength = 0)]
        public string Description { get; set; }
    }
}
