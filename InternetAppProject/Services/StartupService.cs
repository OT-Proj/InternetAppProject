using InternetAppProject.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Services
{
    public class StartupService
    {
        private readonly InternetAppProjectContext _context;
        public StartupService(InternetAppProjectContext context)
        {
            _context = context;
        }
        public async Task InitDB()
        {
            int DrivetypeID;
            var typeQuery = _context.DriveType.Where(dt => dt.Name.Equals("Free"));
            if (typeQuery.Count() < 1)
            {
                Models.DriveType free = new Models.DriveType();
                free.Name = "Free";
                free.Price = 0;
                free.Max_Capacity = 10;
                free.Last_change = DateTime.Now;
                _context.Add(free);
                await _context.SaveChangesAsync();
                DrivetypeID = free.Id;
            }
            else
            {
                DrivetypeID = typeQuery.FirstOrDefault().Id;
            }
            var adminq = _context.User.Where(u => u.Name.Equals("Admin"));
            if (adminq.Count() < 1)
            {
                Models.User admin = new Models.User();
                admin.Name = "Admin";
                admin.Password = "Admin";
                admin.Type = Models.User.UserType.Admin;
                admin.Visual_mode = false;
                admin.Zip = 1234567;
                admin.Credit_card = "12345678";
                admin.D = new Models.Drive();
                admin.D.Current_usage = 0;
                admin.D.Description = "Welcome to MoodleDrive! This is the Admin's drive.";
                admin.D.TypeId = typeQuery.FirstOrDefault();
                admin.Create_time = DateTime.Now;
                //_context.Add(admin.D);
                _context.Add(admin);
                await _context.SaveChangesAsync();
            }
            else
            {
                Models.User admin = adminq.FirstOrDefault();
                if(admin != null && admin.Type != Models.User.UserType.Admin)
                {
                    admin.Type = Models.User.UserType.Admin;
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                }
            }
            if (_context.Tag.Where(u => u.Name.Equals("Pixabay")).Count() < 1)
            {
                Models.Tag pixabay = new Models.Tag();
                pixabay.Name = "Pixabay";
                _context.Add(pixabay);
                await _context.SaveChangesAsync();
            }
        }
    }
}
