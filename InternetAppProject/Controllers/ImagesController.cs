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
            var image = await _context.Image.FindAsync(id);
            _context.Image.Remove(image);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImageExists(int id)
        {
            return _context.Image.Any(e => e.Id == id);
        }
    }
}
