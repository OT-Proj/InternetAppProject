using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class DriveType
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter capacity"), MaxLength(30)]
        [Range(0, 9999, ErrorMessage = "Invalid Capacity - must be between 0 and 9999")]
         public int Max_Capacity { get; set; }

        [Required(ErrorMessage = "Please enter price"), MaxLength(30)]
        [DataType(DataType.Currency)]
        [RegularExpression(@"^([0-9]{0,10})$", ErrorMessage = "Invalid Price")]
        public int Price { get; set; }

        public DateTime Last_change { get; set; }
    }
}
