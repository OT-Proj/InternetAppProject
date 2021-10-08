using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Models;

namespace InternetAppProject.Data
{
    public class InternetAppProjectContext : DbContext
    {
        public InternetAppProjectContext (DbContextOptions<InternetAppProjectContext> options)
            : base(options)
        {
        }
        public DbSet<InternetAppProject.Models.DriveType> DriveType { get; set; }
        public DbSet<InternetAppProject.Models.Tag> Tag { get; set; }
        public DbSet<InternetAppProject.Models.Image> Image { get; set; }
        public DbSet<InternetAppProject.Models.PurchaseEvent> PurchaseEvent { get; set; }
        public DbSet<InternetAppProject.Models.User> User { get; set; }
        public DbSet<InternetAppProject.Models.Drive> Drive { get; set; }
        public DbSet<InternetAppProject.Models.Workplace> Workplace { get; set; }
    }
}
