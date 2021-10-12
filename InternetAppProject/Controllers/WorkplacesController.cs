using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternetAppProject.Data;
using InternetAppProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace InternetAppProject.Controllers
{
    public class WorkplacesController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public WorkplacesController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: Workplaces
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Workplace.ToListAsync());
        }

        // GET: Workplaces/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplace = await _context.Workplace
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workplace == null)
            {
                return NotFound();
            }

            return View(workplace);
        }

        // GET: Workplaces/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Workplaces/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,P_lat,P_long")] Workplace workplace)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workplace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workplace);
        }

        // GET: Workplaces/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplace = await _context.Workplace.FindAsync(id);
            if (workplace == null)
            {
                return NotFound();
            }
            return View(workplace);
        }

        // POST: Workplaces/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,P_lat,P_long")] Workplace workplace)
        {
            if (id != workplace.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workplace);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkplaceExists(workplace.Id))
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
            return View(workplace);
        }

        // GET: Workplaces/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workplace = await _context.Workplace
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workplace == null)
            {
                return NotFound();
            }

            return View(workplace);
        }

        // POST: Workplaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workplace = await _context.Workplace.FindAsync(id);
            _context.Workplace.Remove(workplace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkplaceExists(int id)
        {
            return _context.Workplace.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Maps()
        {
            //var users = await _context.User.ToListAsync();
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FetchJson()
        {
            var q = from w in _context.Workplace
                    select new { name = w.Name, p_lat = w.P_lat, p_long = w.P_long };
            return Json(await q.ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search(string id)
        {
            if (id == null)
            {
                return View();
            }
            var places = await _context.Workplace.Where(p => p.Name.Contains(id)).ToListAsync();
            return View(places);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchJson(string id)
        {
            if (id == null)
            {
                return Json(new List<Workplace>());
            }
            var q = from w in _context.Workplace
                    where w.Name.Contains(id)
                    select new { name = w.Name, p_lat = w.P_lat, p_long = w.P_long};

            return Json(await q.ToListAsync());
        }
    }
}
