using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /*[RegularExpression(@"^([0-9]{0-3})$", ErrorMessage = "Invalid Capacity")] please check it*/
        public int Max_Capacity { get; set; }

        [DataType(DataType.Currency)]
        public int Price { get; set; }

        public DateTime Last_change { get; set; }
    }
}
