using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    [NotMapped]
    public class GraphByDayData
    {
        public String date { get; set; }
        public int userID { get; set; }
        public DateTime date_fixed { get; set; }
        public int value { get; set; }
    }
}
