using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class Image
    {
        public int Id { get; set; }

        [Display(Name = "Image")]
        public byte[] Data { get; set; }

        [NotMapped] //prevents creation of this property in the DB
        public IFormFile ImageFile { get; set; }
         
        public IEnumerable<Tag> Tags { get; set; }

        [Display(Name = "Uploaded")]
        public DateTime UploadTime { get; set; }

        [Display(Name = "Edited")]
        public DateTime EditTime { get; set; }

        [Display(Name = "Public")]
        public bool IsPublic { get; set; }

        //one to many(Image->Drive)
        [Display(Name = "Drive")]
        public Drive DId { get; set; }

        [StringLength(75, ErrorMessage = "Must be between under 75 charcters", MinimumLength = 0)]
        public string Description { get; set; }
    }
}
