﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class User
    {
        public enum UserType
        {
            Client,
            Admin
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(32, ErrorMessage = "Must be between 3 and 32 characters", MinimumLength = 3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public UserType Type { get; set; }

        [RegularExpression(@"^([0-9]{7})$", ErrorMessage = "Invalid Zip Code")]
        public int Zip { get; set; }

        [Required(ErrorMessage = "Credit card is required")]
        [DataType(DataType.CreditCard)]
        [StringLength(16, ErrorMessage = "Please enter vaild credit card", MinimumLength = 8)]
        public string Credit_card { get; set; }

        public bool Visual_mode { get; set; }

        public DateTime Create_time { get; set; }

        [ForeignKey("DId")]
        public Drive D { get; set; }

        //one to many
        public IEnumerable<PurchaseEvent> PurchaseEvents  { get; set; }
    }
}
