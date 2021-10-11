using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class Tag
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter tag name"), MaxLength(30)]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        public IEnumerable<Image> Images { get; set; }
    }
}
