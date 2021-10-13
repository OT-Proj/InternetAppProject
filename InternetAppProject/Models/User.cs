using Microsoft.EntityFrameworkCore;
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

        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(32, ErrorMessage = "Must be between 3 and 32 characters", MinimumLength = 3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Role")]
        public UserType Type { get; set; }

        [RegularExpression(@"^([0-9]{7})$", ErrorMessage = "Invalid Zip Code, must be exactly 7 digits.")]
        public int Zip { get; set; }

        [Required(ErrorMessage = "Credit card is required")]
        [DataType(DataType.CreditCard)]
        [StringLength(16, ErrorMessage = "Invaild credit card, must be 8-16 characters.", MinimumLength = 8)]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        [Display(Name = "Credit card")]
        public string Credit_card { get; set; }

        [Display(Name = "Night mode")]
        public bool Visual_mode { get; set; }

        [Display(Name = "Created")]
        public DateTime Create_time { get; set; }

        [Display(Name = "Drive")]
        [ForeignKey("DId")]
        public Drive D { get; set; }

        [Display(Name = "Purchases")]
        //one to many
        public IEnumerable<PurchaseEvent> PurchaseEvents  { get; set; }
    }
}
