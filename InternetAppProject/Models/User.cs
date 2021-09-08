﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Password { get; set; }

        public int Zip { get; set; }
        
        public string Credit_card { get; set; }

        public bool Visual_mode { get; set; }

        public DateTime Create_time { get; set; }

        public Drive D { get; set; }

        public IEnumerator<PurchaseEvent> Purchases { get; set; }
    }
}