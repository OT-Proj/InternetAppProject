using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace InternetAppProject.Controllers
{
    public class DriveTypesController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public DriveTypesController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: DriveTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.DriveType.ToListAsync());
        }

        // GET: DriveTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driveType = await _context.DriveType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driveType == null)
            {
                return NotFound();
            }

            return View(driveType);
        }

        // GET: DriveTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DriveTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Level,Name,Max_Capacity,Price")] DriveType driveType)
        {
            if (ModelState.IsValid)
            {
                if(driveType.Name.Equals("Free"))
                {
                    return NotFound(); // The free drive type should only be created automatically by the startup service.
                }
                driveType.Last_change = DateTime.Now;
                _context.Add(driveType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(driveType);
        }

        // GET: DriveTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driveType = await _context.DriveType.FindAsync(id);
            if (driveType == null)
            {
                return NotFound(); // drivetype does not exist
            }
            return View(driveType);
        }

        // POST: DriveTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Level,Name,Max_Capacity,Price")] DriveType driveType)
        {
            if (id != driveType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    DriveType existing = _context.DriveType.Where(t => t.Id == id).FirstOrDefault();
                    if(existing != null)
                    {
                        if(existing.Name.Equals("Free"))
                        {
                            return NotFound(); // you cannot change the names of "Free" business plan
                        }
                        existing.Last_change = DateTime.Now;
                        existing.Name = driveType.Name;
                        existing.Max_Capacity = driveType.Max_Capacity;
                        existing.Price = driveType.Price;
                        _context.Update(existing);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound(); // the business plan you are trying to edit does not exist.
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriveTypeExists(driveType.Id))
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
            return View(driveType);
        }

        // GET: DriveTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driveType = await _context.DriveType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driveType == null)
            {
                return NotFound();
            }

            return View(driveType);
        }

        // POST: DriveTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driveType = await _context.DriveType.FindAsync(id);
            if (driveType == null)
            {
                return NotFound(); // no such drivetype exists
            }
            if(driveType.Name.Equals("Free"))
            {
                return NotFound(); // cannot delete "free" drivetype, it is required to stay
            }
            var drives = await _context.Drive.Include(d => d.TypeId).Where(d => d.TypeId.Id == id).ToListAsync();
            if(drives.Count() > 0)
            {
                return NotFound(); // cannot delete drivetype, drives are depending on it.
            }
            _context.DriveType.Remove(driveType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriveTypeExists(int id)
        {
            return _context.DriveType.Any(e => e.Id == id);
        }

        // GET: DriveTypes/Search/5
        public IActionResult Search()
        {
            return View(new List<DriveType>());
        }

        // POST: DriveTypes/Search/5
        [HttpPost]
        public async Task<IActionResult> Search(string name, int? minCapacity, int? maxPrice)
        {
            if(name == null & minCapacity == null && maxPrice == null)
            {
                return View(new List<DriveType>());
            }
            if(name == null)
            {
                name = "";
            }
            if(minCapacity == null)
            {
                minCapacity = 0;
            }
            if(maxPrice == null)
            {
                maxPrice = int.MaxValue;
            }
            var q = from dt in _context.DriveType
                    where dt.Name.Contains(name) &&
                          dt.Max_Capacity >= minCapacity &&
                          dt.Price <= maxPrice
                    select dt;
            return View(await q.ToListAsync());
        }

       // POST: DriveTypes/SearchJson/5
       [HttpPost]
       public async Task<IActionResult> SearchJson(string name, int? minCapacity, int? maxPrice)
       {
            if (name == null & minCapacity == null && maxPrice == null)
            {
                return Json(new List<DriveType>());
            }
            if (name == null)
            {
                name = "";
            }
            if (minCapacity == null)
            {
                minCapacity = 0;
            }
            if (maxPrice == null)
            {
                maxPrice = int.MaxValue;
            }
            var q = from dt in _context.DriveType
                    where dt.Name.Contains(name) &&
                          dt.Max_Capacity >= minCapacity &&
                          dt.Price <= maxPrice
                    select new
                    {
                        name = dt.Name,
                        capacity = dt.Max_Capacity,
                        price = dt.Price,
                        changed = dt.Last_change.ToString("MM/dd/yyyy HH:mm"),
                    };
            return Json(await q.ToListAsync());
       }
    }
}
