using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InternetAppProject.Controllers
{
    public class ImagesController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public ImagesController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: Images
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var images = _context.Image.Include(x => x.Tags);
            return View(await images.ToListAsync());
        }

        // GET: Images/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image.Include(x => x.Tags)
                .Include(x => x.DId).ThenInclude(x => x.UserId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                ViewData["ErrorMsg"] = "The Image you are trying to view is not found.";
                return View("~/Views/Home/ShowError.cshtml"); // orphaned drive - error
            }

            var userDrive = User.Claims.Where(c => c.Type == "drive").FirstOrDefault();
            var userType = User.Claims.Where(c => c.Type == "Type").FirstOrDefault();
            if (image.IsPublic)
            {
                return View(image);
            }
            if (userDrive != null && Int32.Parse(userDrive.Value) == image.DId.Id) {
                return View(image);
            }
            if (userType != null && userType.Value == "Admin")
            {
                return View(image);
            }
            ViewData["ErrorMsg"] = "You do not have premissions to see this image.";
            return View("~/Views/Home/ShowError.cshtml"); // not your image + not admin = no permissions to see image
        }

        // GET: Images/Create
        public IActionResult Create()
        {
            ViewData["Tags"] = new SelectList(_context.Tag, "Id", nameof(Tag.Name));
            return View();
        }

        // POST: Images/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ImageFile,IsPublic,Description")] Image image, int[] tags)
        {
            if(image.ImageFile == null)
            {
                ViewData["imageError"] = "Please select a file.";
                ViewData["Tags"] = new SelectList(_context.Tag, "Id", nameof(Tag.Name));
                return View("Create");
            }
            if (ModelState.IsValid)
            {
                image.UploadTime = DateTime.Now;
                image.EditTime = image.UploadTime;

                // converting image from user to format stored in the DB
                using (MemoryStream ms = new MemoryStream())
                {
                    image.ImageFile.CopyTo(ms);
                    image.Data = ms.ToArray();
                }

                // add selected tags: get tag objects from DB than add to the new image
                var t = _context.Tag.Where(tag => tags.Contains(tag.Id));
                image.Tags = new List<Tag>();
                ((List<Tag>)image.Tags).AddRange(t); // casting IEnumerable as list


                // connect image to the currently logged in user's drive
                var q = from u in _context.User
                        where u.Name == User.FindFirstValue(ClaimTypes.Name)
                        select u.D;

                if (q.Count() > 0)
                {
                    image.DId = q.FirstOrDefault(); // q.First() is the logged in user's drive
                }
                else
                {
                    ViewData["ErrorMsg"] = "Oops! We could not find your account (maybe it was deleted?). Please try to logout and then login again.";
                    return View("~/Views/Home/ShowError.cshtml");// no such user
                }
                if (image.DId == null)
                {
                    ViewData["ErrorMsg"] = "Oops! It seems that you do not have a drive (maybe it was deleted?).";
                    return View("~/Views/Home/ShowError.cshtml"); // user without drive trying to upload an image
                }
                image.DId.TypeId = _context.Drive.Include(d => d.TypeId)
                    .Where(d => d.Id == image.DId.Id).FirstOrDefault().TypeId;

                // check if the drive didn't exceed max capacity
                if (image.DId.Current_usage >= image.DId.TypeId.Max_Capacity)
                {
                    return View("Error");
                }

                //drive has free capcity left
                image.DId.Current_usage++;

                _context.Update(image);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Drives", new { id = image.DId.Id});
            }
            return View(image);
        }

        // GET: Images/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                ViewData["ErrorMsg"] = "Oops! The page you are looking for is not found.";
                return View("~/Views/Home/ShowError.cshtml");
            }

            var image = _context.Image.Include(i => i.Tags).Include(i => i.DId)
                .Where(i => i.Id == id).FirstOrDefault();

            if (image == null)
            {
                ViewData["ErrorMsg"] = "Oops! Image not found (maybe it was deleted?).";
                return View("~/Views/Home/ShowError.cshtml"); // image does not exist
            }

            var userDrive = User.Claims.Where(c => c.Type == "drive").FirstOrDefault();
            var userType = User.Claims.Where(c => c.Type == "Type").FirstOrDefault();
            if(userDrive == null || userType == null)
            {
                ViewData["ErrorMsg"] = "Oops! You are not logged in. Please login and try again.";
                return View("~/Views/Home/ShowError.cshtml"); // user is not logged in and cannot edit images
            }
            bool permissions = false;
            if(Int32.Parse(userDrive.Value) == image.DId.Id || userType.Value.Equals("Admin"))
            {
                permissions = true;
            }
            if(!permissions)
            {
                ViewData["ErrorMsg"] = "Oops! You do not have permissions to edit this image.";
                return View("~/Views/Home/ShowError.cshtml"); // not your image and you are not an admin, can't edit
            }
            var tagSelectList = new SelectList(_context.Tag, "Id", nameof(Tag.Name));
            foreach(var s in tagSelectList)
            {
                foreach(Tag t in image.Tags)
                {
                    if(t.Name.Equals(s.Text))
                    {
                        s.Selected = true;
                    }
                }
            }
            ViewData["Tags"] = tagSelectList;

            var freeDrives = from d in _context.Drive
                          join t in _context.DriveType on d.TypeId.Id equals t.Id
                          join u in _context.User on d.UserId.Id equals u.Id
                          where d.Current_usage < d.TypeId.Max_Capacity
                          select new { Id = d.Id, name = d.UserId.Name };

            SelectList driveSelectList = new SelectList(freeDrives, "Id", "name");
            foreach (var s in driveSelectList)
            {
                if(Int32.Parse(s.Value) == image.DId.Id)
                {
                    s.Selected = true;
                }
            }
            ViewData["Drives"] = driveSelectList;

            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageFile,IsPublic,Description")] Image image, int[] tags, int? Drive)
        {
            if (id != image.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing_image = _context.Image.Where(img => img.Id == id).Include(x => x.DId)
                        .Include(x=>x.Tags).FirstOrDefault();
                    if (existing_image == null)
                    {
                        ViewData["ErrorMsg"] = "Oops! The image you are trying to edit does not exist (maybe it was deleted?).";
                        return View("~/Views/Home/ShowError.cshtml"); // image you are trying to edit does not exist
                    }
                    bool permissions = false;
                    bool isAdmin = false;
                    var userDrive = User.Claims.Where(c => c.Type == "drive").FirstOrDefault();
                    var userType = User.Claims.Where(c => c.Type == "Type").FirstOrDefault();
                    if (userDrive != null && Int32.Parse(userDrive.Value) == existing_image.DId.Id)
                    {
                        permissions = true;
                    }
                    if (userType != null && userType.Value == "Admin")
                    {
                        isAdmin = true;
                        permissions = true;
                    }
                    if(!permissions)
                    {
                        ViewData["ErrorMsg"] = "Oops! You do not have permissions to edit this image.";
                        return View("~/Views/Home/ShowError.cshtml"); // no permissions to edit this image
                    }

                    // check if the user uploaded a new image to replace to old one
                    if (image.ImageFile != null)
                    {
                        // converting image from user to format stored in the DB
                        using (MemoryStream ms = new MemoryStream())
                        {
                            image.ImageFile.CopyTo(ms);
                            existing_image.Data = ms.ToArray();
                        }
                    }

                    // add selected tags: get tag objects from DB than add to the new image
                    var t = _context.Tag.Where(tag => tags.Contains(tag.Id));
                    List<Tag> tags_list = new List<Tag>();
                    tags_list.AddRange(t.ToList());
                    existing_image.Tags = tags_list; // casting IEnumerable as list
                    existing_image.EditTime = DateTime.Now;
                    existing_image.UploadTime = existing_image.UploadTime;
                    existing_image.IsPublic = image.IsPublic;
                    existing_image.Description = image.Description;
                    
                    if(Drive != null && isAdmin)
                    {
                        Drive targetDrive = _context.Drive.Where(d => d.Id == Drive).FirstOrDefault();
                        if(targetDrive != null)
                        {
                            existing_image.DId = targetDrive;
                        }
                    }

                    //_context.Remove(existing_image);
                    _context.Update(existing_image);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImageExists(image.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = image.Id });
            }
            return View(image);
        }

        // GET: Images/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image.Include(d => d.DId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            var userDrive = User.Claims.Where(c => c.Type == "drive").FirstOrDefault();
            var userType = User.Claims.Where(c => c.Type == "Type").FirstOrDefault();
            if (userDrive == null || userType == null)
            {
                ViewData["ErrorMsg"] = "Oops! You are not logged in. Please login and try again.";
                return View("~/Views/Home/ShowError.cshtml"); // user is not logged in and cannot edit images
            }
            bool permissions = false;
            if (Int32.Parse(userDrive.Value) == image.DId.Id || userType.Value.Equals("Admin"))
            {
                permissions = true;
            }
            if (!permissions)
            {
                ViewData["ErrorMsg"] = "Oops! You do not have permissions to delete this image.";
                return View("~/Views/Home/ShowError.cshtml"); // not your image and you are not an admin, can't edit
            }
            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = _context.Image.Include(i => i.DId).Where(i => i.Id == id).FirstOrDefault();
            int drive = -1;
            if(image == null)
            {
                return NotFound();
            }
            if (image.DId != null)
            {
                var userDrive = User.Claims.Where(c => c.Type == "drive").FirstOrDefault();
                var userType = User.Claims.Where(c => c.Type == "Type").FirstOrDefault();
                if (userDrive != null && (image.DId.Id == Int32.Parse(userDrive.Value) || userType.Value == "Admin"))
                {
                    image.DId.Current_usage--;
                    image.DId.Current_usage = Math.Max(image.DId.Current_usage, 0); // prevent negatives
                    drive = image.DId.Id;
                }
                else
                {
                    ViewData["ErrorMsg"] = "Oops! You do not have permissions to delete this image.";
                    return View("~/Views/Home/ShowError.cshtml"); // user is trying to delete image that's not theirs
                }
            }

            _context.Image.Remove(image);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Drives", new { id = drive });
        }

        private bool ImageExists(int id)
        {
            return _context.Image.Any(e => e.Id == id);
        }

        // GET: Images/Search/5
        public IActionResult Search()
        {
            return View();
        }

        // POST: Images/Search/5
        [HttpPost]
        public async Task<IActionResult> Search(string desc, DateTime start, DateTime end)
        {
            DateTime DTdefault = new DateTime();
            if (desc == null && start.CompareTo(DTdefault) == 0 && end.CompareTo(DTdefault) == 0)
            {
                return View();
            }
            if (desc == null)
            {
                //return Json(new List<Image>());
                desc = "";
            }
            if (start.CompareTo(DTdefault) == 0) // start was left empty
            {
                start = DateTime.MinValue;
            }
            if (end.CompareTo(DTdefault) == 0) // end was left empty
            {
                end = DateTime.MaxValue;
            }

            var q = from i in _context.Image
                    join d in _context.Drive on i.DId.Id equals d.Id
                    where i.Description.Contains(desc) &&
                          i.UploadTime.CompareTo(start) >= 0 &&
                          i.UploadTime.CompareTo(end) <= 0
                    select i;

            List<Image> images = await q.ToListAsync();

            if (((ClaimsIdentity)User.Identity) != null)
            {
                var UserId = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
                var UserDrive = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "drive");
                var UserType = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "Type");
                images = images.FindAll(img =>
                {
                    Drive d = img.DId;
                    if (UserId != null && UserType != null && UserDrive != null)
                    {
                        if (Int32.Parse(UserDrive.Value) != d.UserId.Id && UserType.Value != "Admin" && !img.IsPublic)
                        {
                            return false; // if user has no authority we remove the image
                        }
                        return true;
                    }
                    return img.IsPublic;
                }
                );
            }
            else
            {
                images = images.FindAll(img => img.IsPublic); // user is not logged in and can only view public images
            }

            return View(images);
        }


        [HttpPost]
        public async Task<IActionResult> SearchJson(string desc, DateTime start, DateTime end)
        {
            DateTime DTdefault = new DateTime();
            if (desc == null && start.CompareTo(DTdefault) == 0 && end.CompareTo(DTdefault) == 0)
            {
                return Json(null);
            }
            if (desc == null) {
                //return Json(new List<Image>());
                desc = "";
            }
            if (start.CompareTo(DTdefault) == 0) // start was left empty
            {
                start = DateTime.MinValue;
            }
            if (end.CompareTo(DTdefault) == 0) // end was left empty
            {
                end = DateTime.MaxValue;
            }

            var q = from i in _context.Image
                    join d in _context.Drive on i.DId.Id equals d.Id
                    where i.Description.Contains(desc) &&
                          i.UploadTime.CompareTo(start) >= 0 &&
                          i.UploadTime.CompareTo(end) <= 0
                    select new { id = i.Id, 
                                data = i.Data, 
                                description = i.Description, 
                                uploadtime = i.UploadTime.ToString("MM/dd/yyyy HH:mm"), 
                                IsPublic = i.IsPublic, 
                                drive = d };

            var images = await q.ToListAsync();

            // permission check
            if (((ClaimsIdentity)User.Identity) != null)
            {
                var UserId = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
                var UserDrive = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "drive");
                var UserType = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "Type");
                images = images.FindAll(img =>
                {
                    //Drive d = _context.Image.Include(o => o.DId).Where(o => o.Id == img.Id).FirstOrDefault().DId;
                    Drive d = img.drive;
                    if (UserId != null && UserType != null && UserDrive != null)
                    {
                        if (Int32.Parse(UserDrive.Value) != d.Id && UserType.Value != "Admin" && !img.IsPublic)
                        {
                            return false; // if user has no authority we remove the image
                        }
                        return true;
                    }
                    return img.IsPublic;
                }
                );
            }
            else
            {
                images = images.FindAll(img => img.IsPublic); // user is not logged in and can only view public images
            }

            return Json(images);
        }
    }
}
