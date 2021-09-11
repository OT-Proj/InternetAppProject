﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace InternetAppProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public UsersController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Password,Zip,Credit_card,Visual_mode,Create_time")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Create_time = DateTime.Now;
                user.Type = Models.User.UserType.Client;
                if(user.Name.Equals("admin") && user.Password.Equals("admin"))
                    user.Type = Models.User.UserType.Admin;
                _context.Add(user);

                // create a new drive for the user
                Drive d = new Drive();
                d.UserId = user;
                d.Current_usage = 0;
                _context.Add(d);

                await _context.SaveChangesAsync();
                LoginUser(user.Name.ToString(), user.Type, user.Id, user.D.Id);

                if (user.Type == Models.User.UserType.Client)
                    return RedirectToAction("Details","Drives", new { id = user.D.Id });
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // make select list out of the user types enum
            var types = Enum.GetValues(typeof(InternetAppProject.Models.User.UserType)).Cast<InternetAppProject.Models.User.UserType>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();
            ViewData["Types"] = new SelectList(types, "Value", "Text");
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Password,Type,Zip,Credit_card,Visual_mode")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // fetch old user data to keep the create_time correct
                    var existing_Data = _context.User.Where(U => U.Id == id).First();
                    user.Create_time = existing_Data.Create_time;
                    _context.Remove(existing_Data);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            /*  //previous version: worse beacuse 3 seperate queries are slower
            var user = await _context.User.FindAsync(id);
            var drive = _context.Drive.Where(d => d.UserId.Id == id).First();
            var images = _context.Image.Where(i => i.DId.Id == drive.Id).ToList();
            images.ForEach(i => _context.Image.Remove(i));
            _context.Drive.Remove(drive);
            _context.User.Remove(user);
            */

            var q = from u in _context.User
                    join d in _context.Drive on u.D.Id equals d.Id
                    join im in _context.Image on d.Id equals im.DId.Id into sub
                    from subq in sub.DefaultIfEmpty() // left outer join (for empty drives)
                    where u.Id == id
                    select new { userObj = u, driveObj = d, image = subq };

            if (q.Count() < 1)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.User.Remove(q.First().userObj);
            _context.Drive.Remove(q.First().driveObj);
            q.ToList().ForEach(i => { if (i.image != null) _context.Image.Remove(i.image); }); // iterate over results and delete each image
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        // GET: Users/Create
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Index", "Home");
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Name,Password")] User user)
        {
            LogoutUser();
            if (ModelState.IsValid)
            {
                // need do join users and drives in order to fetch drive.Id. 
                var q = from u in _context.User
                        join d in _context.Drive on u.D.Id equals d.Id
                        where u.Name == user.Name &&
                                u.Password == user.Password
                        select new { userObj = u, driveObj = d};

                if (q.Count() > 0)
                {
                    // user is found
                    LoginUser(q.First().userObj.Name, q.First().userObj.Type, q.First().userObj.Id, q.First().driveObj.Id);
                    return View("Index",await _context.User.ToListAsync());
                }
                else
                {
                    ViewData["Error"] = "Username/password is incorrect.";
                }
            }
            return View(user);
        }

        private async void LogoutUser()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        private async void LoginUser(string userName, InternetAppProject.Models.User.UserType userType, int id, int drive)
        {
            string type = Enum.GetName(typeof(InternetAppProject.Models.User.UserType),userType);
            string sid = id.ToString();
            string did = drive.ToString();
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, type),
                    new Claim("Type", type),
                    new Claim("id", sid), // user.Id
                    new Claim("drive", did), // user.D.Id
                };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // GET: Users/SearchSearch/5
        public async Task<IActionResult> Search(string id)
        {
            if (id == null)
            {
                return View();
            }
            var users = await _context.User.Where(u => u.Name.Contains(id)).Include(u => u.D).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> SearchJson(string id)
        {
            if (id == null)
            {
                return Json(new List<User>());
            }
            var q = from u in _context.User
                    join d in _context.Drive on u.D.Id equals d.Id
                    where u.Name.Contains(id)
                    orderby u.Name ascending
                    select new { name = u.Name, drive = d.Id };
                    

            return Json(await q.ToListAsync());
        }

    }
}
