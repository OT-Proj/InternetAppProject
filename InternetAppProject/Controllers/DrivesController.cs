using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace InternetAppProject.Controllers
{
    public class DrivesController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public DrivesController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: Drives
        public async Task<IActionResult> Index()
        {
            var data = _context.Drive.Include(x => x.UserId).Include(x => x.Images);
            return View(await data.ToListAsync());
        }

        // GET: Drives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drive = await _context.Drive.Include(d => d.UserId).Include(d => d.Images).ThenInclude(img => img.Tags)
                 .FirstOrDefaultAsync(m => m.Id == id);

            if (drive == null)
            {
                return NotFound();
            }

            if (((ClaimsIdentity)User.Identity) != null)
            {
                var UserId = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
                var UserType = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "Type");
                if (UserId != null && UserType != null)
                {
                    // user is logged in
                    if (drive.UserId.Id.ToString() != UserId.Value && UserType.Value != "Admin")
                    {
                        // user is neither owner nor an admin therefore not authorized to view private images
                        drive.Images = drive.Images.ToList().FindAll(img => img.IsPublic);
                    }
                }
                else
                {
                    // user is not logged in and therefore unautorized to view private images
                    drive.Images = drive.Images.ToList().FindAll(img => img.IsPublic);
                }
                
            }

            return View(drive);
        }

        // GET: Drives/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Current_usage")] Drive drive)
        {
            if (ModelState.IsValid)
            {
                _context.Add(drive);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(drive);
        }

        // GET: Drives/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drive = await _context.Drive.FindAsync(id);
            if (drive == null)
            {
                return NotFound();
            }
            return View(drive);
        }

        // POST: Drives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Current_usage")] Drive drive)
        {
            if (id != drive.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drive);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriveExists(drive.Id))
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
            return View(drive);
        }

        // GET: Drives/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drive = await _context.Drive.Include(d => d.UserId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drive == null)
            {
                return NotFound();
            }

            return View(drive);
        }

        // POST: Drives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var q = from d in _context.Drive
                    join u in _context.User on d.Id equals u.D.Id
                    join im in _context.Image on d.Id equals im.DId.Id into sub
                    from subq in sub.DefaultIfEmpty() // left outer join (for empty drives)
                    where d.Id == id
                    select new { userObj = u, driveObj = d, image = subq };
            
            if(q.Count() < 1)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.User.Remove(q.First().userObj);
            _context.Drive.Remove(q.First().driveObj);
            q.ToList().ForEach(i =>{ if (i.image != null) _context.Image.Remove(i.image); }); // iterate over results and delete each image
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriveExists(int id)
        {
            return _context.Drive.Any(e => e.Id == id);
        }
    }
}
