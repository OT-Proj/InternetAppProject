using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternetAppProject.Models
{
    public class PurchaseEvent
    {
        public int Id { get; set;}

        public User UserID { get; set; }

        [Required(ErrorMessage = "You must enter a date")]
        public DateTime Time { get; set; }

        [Required(ErrorMessage = "You must enter purchase amount")]
        [Range(0, 9999, ErrorMessage = "Invalid amount - must be between 0 and 9999")]
        public int Amount { get; set; }

    }
}
