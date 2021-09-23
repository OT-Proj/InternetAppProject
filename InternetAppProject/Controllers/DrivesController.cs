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
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IO;
using DriveType = InternetAppProject.Models.DriveType;
using System.Net;

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

        // GET: Upgrade
        public async Task<IActionResult> Upgrade(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Drive d = _context.Drive.Include(d => d.UserId).Include(d => d.TypeId).Where(d => d.Id == id).FirstOrDefault();
            if (d == null)
            {
                return NotFound();
            }
            int current_max = d.TypeId.Max_Capacity;
            var q = from dt in _context.DriveType
                    where dt.Max_Capacity > current_max
                    orderby dt.Max_Capacity ascending
                    select dt;

            List<DriveType> lst = await q.ToListAsync();
            ViewData["Types"] = new SelectList(lst, "Id", nameof(DriveType.Name));

            // calculate initial payment amount for the first option presented to the user
            ViewData["Amount"] = 0;
            if (lst.FirstOrDefault() != null)
            {
                ViewData["Amount"] = Math.Max(lst.FirstOrDefault().Price - UserPaid(d.UserId), 0);
            }

            return View();
        }

        // POST: Drives/PaymentAmountJson/5
        [HttpPost]
        public IActionResult PaymentAmountJson(int? id)
        {
            if (id == null)
            {
                return Json(null);
            }

            // find the relevant drive type
            DriveType dt = _context.DriveType.Where(dt => dt.Id == id).FirstOrDefault();
            if (dt == null)
            {
                return Json(null);
            }

            // find how much the user already paid
            if (((ClaimsIdentity)User.Identity) == null)
            {
                return Json(null);

            }
            int UserId = Int32.Parse(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id").Value);
            User user = _context.User.Where(u => u.Id == UserId).FirstOrDefault();

            // calculate final debt; if negative, default is 0 (free upgrade)
            int debt = Math.Max(dt.Price - UserPaid(user), 0);

            // return valid Json
            var result = new PriceResult{ amount = debt };
            List<PriceResult> resultList = new List<PriceResult>();
            resultList.Add(result);

            return Json(resultList);
        }

        // POST: Drives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoUpgrade(int? TypeId)
        {
            if (TypeId == null)
            {
                return NotFound();
            }
            // verify the designated drive type actually exists in our DB:
            DriveType dt = _context.DriveType.Where(t => t.Id == TypeId).FirstOrDefault();

            // verify user is logged in and has a drive:
            var DriveClaim = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "drive");
            if(DriveClaim == null)
            {
                return NotFound();
            }

            // update existing drive
            int UserDriveId = Int32.Parse(DriveClaim.Value);
            var q = from d in _context.Drive
                    join u in _context.User on d.Id equals u.D.Id
                    join im in _context.Image on d.Id equals im.DId.Id into sub
                    from subq in sub.DefaultIfEmpty() // left outer join (for empty drives)
                    where d.Id == UserDriveId
                    select new { drive = d, user = u };

            Drive UserDrive = q.FirstOrDefault().drive;
            User Owner = q.FirstOrDefault().user;
            UserDrive.TypeId = dt;
            _context.Update(UserDrive);

            // Record the purchase event in the DB:
            PurchaseEvent pe = new PurchaseEvent { Time = DateTime.Now,
                UserID = Owner,
                Amount = Math.Max(dt.Price - UserPaid(Owner), 0),
            };
            _context.PurchaseEvent.Add(pe);
            await _context.SaveChangesAsync();
            return View("Details", UserDrive);
        }

        // GET: Drives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drive = await _context.Drive.Include(d => d.TypeId).Include(d => d.UserId).Include(d => d.Images).ThenInclude(img => img.Tags)
                 .FirstOrDefaultAsync(m => m.Id == id);

            if (drive == null)
            {
                return NotFound();
            }

            if(drive.UserId == null)
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
                var q = from t in _context.DriveType
                        where t.Name.Equals("Free")
                        select t;
                DriveType dt = q.FirstOrDefault();
                drive.TypeId = dt;

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

        [HttpPost]
        public async Task<IActionResult> Fill(int? id, string word)
        {
            int max_allowed = 3;

            if (id == null)
            {
                return NotFound();
            }
            if(word == null)
            {
                word = "Technology";
            }
            var drive = await _context.Drive.Include(d => d.UserId).Include(d => d.TypeId).Include(d => d.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drive == null)
            {
                return NotFound();
            }
            var UserId = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
            if(UserId == null)
            {
                return NotFound(); // user not logged in.
            }
            if (drive.UserId.Id != Int32.Parse(UserId.Value))
            {
                return NotFound(); // no permission
            }


            // fill drive with web service: Art Institute of Chicago API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://pixabay.com/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // our key is 23510481-c5bf39e0b210f2fe4301098dd
                String key = "23510481-c5bf39e0b210f2fe4301098dd";
                HttpResponseMessage Res = await client.GetAsync("api/?key=" + key + "&q=" + word + "&image_type=photo&pretty=true");
                if (Res.IsSuccessStatusCode)
                {
                    var Response = Res.Content.ReadAsStringAsync().Result;
                    JObject parsed = JObject.Parse(Response);

                    // create new image object from recieved data

                    int freeSlots = drive.TypeId.Max_Capacity - drive.Current_usage; // if negative nothing will be added
                    freeSlots = Math.Min(freeSlots, parsed["hits"].Count()); // limit the number of images to the number of results
                    freeSlots = Math.Min(freeSlots, max_allowed);
                    for (int i = 0; i < freeSlots; i++) {
                        Image I = new Image() { DId = drive, UploadTime = DateTime.Now, IsPublic = true, EditTime = DateTime.Now };
                        using (WebClient c = new WebClient())
                        {
                            byte[] image = c.DownloadData(parsed["hits"][i]["webformatURL"].ToString());
                            I.Data = image;
                        }
                        I.Description = "this image was originally uploaded by Pixabay/" + parsed["hits"][i]["user"].ToString();
                        _context.Image.Update(I);
                        drive.Current_usage++;
                    }
                    _context.Drive.Update(drive);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", "Drives", new { id = id });
        }

        private bool DriveExists(int id)
        {
            return _context.Drive.Any(e => e.Id == id);
        }

        private int UserPaid(User user)
        {
            if(user == null)
            {
                return 0;
            }
            var q = from pe in _context.PurchaseEvent
                    join u in _context.User on pe.UserID.Id equals u.Id
                    where pe.UserID.Id == user.Id
                    group pe by pe.UserID.Id into user_purchases
                    select new
                    {
                        sum = user_purchases.Sum(x => x.Amount)
                    };
            if (q.Count() > 0)
            {
                return q.FirstOrDefault().sum;
            }
            return 0;
        }
    }
}
