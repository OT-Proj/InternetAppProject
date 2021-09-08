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
    }
}
