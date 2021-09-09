using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Models
{
    public class Image
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }

        [NotMapped] //prevents creation of this property in the DB
        public IFormFile ImageFile { get; set; }
         

        public IEnumerable<Tag> Tags { get; set; }

        public DateTime UploadTime { get; set; }

        public DateTime EditTime { get; set; }

        public bool IsPublic { get; set; }

        //one to many(Image->Drive)
        public Drive DId { get; set; }

        public string Description { get; set; }
    }
}
