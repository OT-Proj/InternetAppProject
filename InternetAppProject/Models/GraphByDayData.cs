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
        public DateTime date { get; set; }
        public int value { get; set; }
    }
}
