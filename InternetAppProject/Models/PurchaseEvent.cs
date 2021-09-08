using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class PurchaseEvent
    {
        public int Id { get; set;}
        public User UserID { get; set; }

        public DateTime Time { get; set; }

        public int Amount { get; set; }
    }
}
