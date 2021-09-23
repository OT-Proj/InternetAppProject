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
    public class PurchaseEventsController : Controller
    {
        private readonly InternetAppProjectContext _context;

        public PurchaseEventsController(InternetAppProjectContext context)
        {
            _context = context;
        }

        // GET: PurchaseEvents
        public async Task<IActionResult> Index()
        {
            var events = await _context.PurchaseEvent.Include(e => e.UserID).ToListAsync();
            return View(events);
        }

        // GET: PurchaseEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseEvent = await _context.PurchaseEvent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseEvent == null)
            {
                return NotFound();
            }

            return View(purchaseEvent);
        }

        // GET: PurchaseEvents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PurchaseEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Time,Amount")] PurchaseEvent purchaseEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(purchaseEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(purchaseEvent);
        }

        // GET: PurchaseEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseEvent = await _context.PurchaseEvent.FindAsync(id);
            if (purchaseEvent == null)
            {
                return NotFound();
            }
            return View(purchaseEvent);
        }

        // POST: PurchaseEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Time,Amount")] PurchaseEvent purchaseEvent)
        {
            if (id != purchaseEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseEventExists(purchaseEvent.Id))
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
            return View(purchaseEvent);
        }

        // GET: PurchaseEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseEvent = await _context.PurchaseEvent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseEvent == null)
            {
                return NotFound();
            }

            return View(purchaseEvent);
        }

        // POST: PurchaseEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchaseEvent = await _context.PurchaseEvent.FindAsync(id);
            _context.PurchaseEvent.Remove(purchaseEvent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseEventExists(int id)
        {
            return _context.PurchaseEvent.Any(e => e.Id == id);
        }

        public IActionResult Graphs()
        {
            return View();
        }

        public async Task<IActionResult> ByDayJson(int? id)
        {
            if(id == null)
            {
                id = 31;
            }
            DateTime start = DateTime.Now.AddDays(-1 * (double)id);
            var q1 = from u in _context.PurchaseEvent
                     where u.Time.CompareTo(start) > 0
                     orderby u.Amount
                     select new GraphByDayData{ value = u.Amount, date = null, date_fixed = u.Time};
            var p = await q1.ToListAsync();
            p.ForEach(x => x.date = x.date_fixed.ToString("yyyy/MM/dd"));
            var q2 = p.GroupBy(x => x.date).Select(x => new { value = x.Sum(x => x.value), date = x.Key, date_fixed = DateTime.Parse(x.Key) }).ToList();
            q2.Sort((x, y) => x.date_fixed.CompareTo(y.date_fixed));
            return Json(q2);
        }


        public async Task<IActionResult> CountByDayJson(int? id)
        {
            if (id == null)
            {
                id = 31;
            }
            DateTime start = DateTime.Now.AddDays(-1 * (double)id);
            var q1 = from u in _context.PurchaseEvent
                     where u.Time.CompareTo(start) > 0
                     orderby u.Amount
                     select new GraphByDayData { value = u.Amount, date = null, date_fixed = u.Time, userID = u.UserID.Id };

            var p = await q1.ToListAsync();
            p.ForEach(x => x.date = x.date_fixed.ToString("yyyy/MM/dd"));

            // two groupBy calls - one is to distinguish users + date, the other to count only by date
            var q2 = p.Distinct().GroupBy(x => new { x.userID, x.date}).GroupBy(x => x.Key.date).Select(x => new { value = x.Count(), date = x.Key, date_fixed = DateTime.Parse(x.Key) }).ToList();
            q2.Sort((x, y) => x.date_fixed.CompareTo(y.date_fixed));
            return Json(q2);
        }
    }
}
