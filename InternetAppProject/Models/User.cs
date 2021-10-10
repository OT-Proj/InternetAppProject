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

        /* to do: uniqe*/

        [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public UserType Type { get; set; }

        [DataType(DataType.PostalCode)]
        [StringLength(8, ErrorMessage = "Zip code contains 7 digits", MinimumLength = 7)]
        public int Zip { get; set; }

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
