using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;

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
                return NotFound();
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
                    driveType.Last_change = DateTime.Now;
                    _context.Update(driveType);
                    await _context.SaveChangesAsync();
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
            _context.DriveType.Remove(driveType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriveTypeExists(int id)
        {
            return _context.DriveType.Any(e => e.Id == id);
        }
    }
}
