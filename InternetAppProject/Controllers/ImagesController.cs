﻿using System;
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
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
                    image.DId = q.First(); // q.First() is the logged in user's drive
                }
                else
                {
                    return NotFound();
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
                return RedirectToAction(nameof(Index));
            }
            return View(image);
        }

        // GET: Images/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }
            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageFile,IsPublic,Description")] Image image)
        {
            if (id != image.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing_image = _context.Image.Where(img => img.Id == id).First();
                    if (existing_image == null)
                    {
                        return NotFound();
                    }
                    // first check if the user uploaded a new image to replace to old one
                    if (image.ImageFile != null)
                    {
                        // converting image from user to format stored in the DB
                        using (MemoryStream ms = new MemoryStream())
                        {
                            image.ImageFile.CopyTo(ms);
                            image.Data = ms.ToArray();
                        }
                    }
                    else
                    {
                        // user did not update the image file, save the old one instead
                        image.ImageFile = existing_image.ImageFile;
                        image.Data = existing_image.Data;
                    }
                    image.EditTime = DateTime.Now;
                    image.UploadTime = existing_image.UploadTime;
                    _context.Remove(existing_image);
                    _context.Update(image);
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
                return RedirectToAction(nameof(Index));
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

            var image = await _context.Image
                .FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = _context.Image.Include(i => i.DId).Where(i => i.Id == id).FirstOrDefault();
            if(image == null)
            {
                return NotFound();
            }
            if (image.DId != null)
            {
                image.DId.Current_usage--;
                image.DId.Current_usage = Math.Max(image.DId.Current_usage, 0); // prevent negatives
            }
            _context.Image.Remove(image);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
                var UserType = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "Type");
                images = images.FindAll(img =>
                {
                    Drive d = img.DId;
                    if (UserId != null && UserType != null)
                    {
                        if (UserId.Value != d.Id.ToString() && UserType.Value != "Admin" && !img.IsPublic)
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
                var UserType = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "Type");
                images = images.FindAll(img =>
                {
                    //Drive d = _context.Image.Include(o => o.DId).Where(o => o.Id == img.Id).FirstOrDefault().DId;
                    Drive d = img.drive;
                    if (UserId != null && UserType != null)
                    {
                        if (UserId.Value != d.Id.ToString() && UserType.Value != "Admin" && !img.IsPublic)
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
