using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class DriveType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Range(1, 1000, ErrorMessage = "Value must be between 1 and 1000")]
        public int Max_Capacity { get; set; }

        public int Price { get; set; }

        public DateTime Last_change { get; set; }
    }
}
