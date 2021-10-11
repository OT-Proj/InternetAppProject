using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class DriveType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Max_Capacity { get; set; }

        public int Price { get; set; }

        public DateTime Last_change { get; set; }
    }
}
