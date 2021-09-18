using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    [NotMapped]
    public class PriceResult
    {
        public int amount { get; set; }
    }
}
