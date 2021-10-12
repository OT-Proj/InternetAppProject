using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class Workplace
    {
        public int Id { get; set; }
        [Display(Name = "Facility Name")]
        public string Name { get; set; }
        [Display(Name = "Latitude")]
        public float P_lat { get; set; }
        [Display(Name = "Longitude")]
        public float P_long { get; set; }
    }
}
