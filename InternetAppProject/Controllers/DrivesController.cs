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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var data = _context.Drive.Include(x => x.UserId);
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
            ViewData["Images"] = "N/A";
            if (lst.FirstOrDefault() != null)
            {
                ViewData["Amount"] = Math.Max(lst.FirstOrDefault().Price - UserPaid(d.UserId), 0);
                ViewData["Images"] = lst.FirstOrDefault().Max_Capacity;
            }
            else
            {
                return NotFound(); // no better business plan exists! congratulations, you already enjoy our best service!
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
            var result = new PriceResult{ amount = debt, images = dt.Max_Capacity};
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
                    join u in _context.User on d.Id equals u.D.Id into sub1
                    from subq1 in sub1.DefaultIfEmpty() // left outer join (for orphaned drives)
                    join im in _context.Image on d.Id equals im.DId.Id into sub2
                    from subq2 in sub2.DefaultIfEmpty() // left outer join (for empty drives)
                    where d.Id == UserDriveId
                    select new { drive = d, user = d.UserId };

            Drive UserDrive = q.FirstOrDefault().drive;
            User Owner = q.FirstOrDefault().user;
            UserDrive.TypeId = dt;
            _context.Update(UserDrive);

            // Record the purchase event in the DB:
            PurchaseEvent pe = new PurchaseEvent { Time = DateTime.Now,
                UserID = Owner,
                Amount = Math.Max(dt.Price - UserPaid(Owner), 0),
            };
            if(pe.Amount > 0)
            {
                _context.PurchaseEvent.Add(pe);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = UserDrive.Id });
        }

        // GET: Drives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ViewData["ErrorMsg"] = "The drive you are trying to view is not found.";
                return View("~/Views/Home/ShowError.cshtml"); // orphaned drive - error
            }

            // fetch drive from DB
            var drive = await _context.Drive.Include(d => d.TypeId)
                              .Include(d => d.UserId)
                              .Include(d => d.Images).ThenInclude(img => img.Tags)
                              .FirstOrDefaultAsync(m => m.Id == id);

            // verify user's permissions to view the images on this page
            var CookieDrive = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "drive");
            if (drive == null)
            {
                //drive not found, is it yours?
                if(CookieDrive != null && Int32.Parse(CookieDrive.Value) == id)
                {
                    // user has no drive, suggest making a new one
                    return RedirectToAction("Create", null);
                }
                if (CookieDrive != null && Int32.Parse(CookieDrive.Value) == -1)
                {
                    // user didn't have a drive when logging in
                    return RedirectToAction("Create", null);
                }
                ViewData["ErrorMsg"] = "The Drive you are trying to view is not found.";
                return View("~/Views/Home/ShowError.cshtml"); // orphaned drive - error
            }

            // dynamically fix counting errors with number of images
            if(drive.Current_usage != drive.Images.Count())
            {
                drive.Current_usage = drive.Images.Count();
                _context.Update(drive);
                await _context.SaveChangesAsync();
            }

            if(drive.UserId == null)
            {
                ViewData["ErrorMsg"] = " Oops! This device has no owner. Please create a new drive.";
                return View("~/Views/Home/ShowError.cshtml"); // orphaned drive - error
            }

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
        public async Task<IActionResult> Create([Bind("Id,Description")] Drive drive)
        {
            var UserClaim = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
            if (UserClaim == null)
            {
                return NotFound(); // redirect to whoops! you are not logged in.
            }
            int UserID = Int32.Parse(UserClaim.Value);
            var user_q = from u in _context.User
                         join d in _context.Drive on u.D.Id equals d.Id into subq
                         from q in subq.DefaultIfEmpty()
                         where u.Id == UserID
                         select new { User = u, Drive = q.UserId.D};

            if (user_q.Count() < 1)
            {
                ViewData["ErrorMsg"] = "Oops! user not found.";
                return View("~/Views/Home/ShowError.cshtml"); // redirect to whoops! user not found
            }

            Drive ExistingDrive = user_q.FirstOrDefault().Drive;

            if(ExistingDrive != null)
            {
                ViewData["ErrorMsg"] = "Oops! user already has a drive.";
                return View("~/Views/Home/ShowError.cshtml"); // redirect to whoops! user already has a drive
            }

            if (ModelState.IsValid)
            {
                // create the new drive and assign in the the logged in user
                var q = from t in _context.DriveType
                        where t.Name.Equals("Free")
                        select t;
                DriveType dt = q.FirstOrDefault();
                drive.TypeId = dt;
                drive.UserId = user_q.FirstOrDefault().User;
                drive.Current_usage = 0;
                _context.Add(drive);
                await _context.SaveChangesAsync();

                // update cookies to include current drive id
                updateCookies(drive.UserId.Name, drive.UserId.Type, drive.UserId.Id, drive.Id, drive.UserId.Visual_mode);

                return RedirectToAction("Details", new { id = drive.Id });
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

            var drive = _context.Drive.Include(d => d.TypeId).Where(d => d.Id == id).FirstOrDefault();
            if (drive == null)
            {
                return NotFound();
            }
            SelectList typeSelectList = new SelectList(_context.DriveType, "Id", nameof(DriveType.Name));
            foreach (var s in typeSelectList)
            {
                if (Int32.Parse(s.Value) == drive.TypeId.Id)
                {
                    s.Selected = true;
                }
            }
            ViewData["Types"] = typeSelectList;
            return View(drive);
        }

        // POST: Drives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description")] Drive drive, int? Types)
        {
            if (id != drive.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Drive existing = _context.Drive.Where(d => d.Id == id).Include(d => d.UserId).FirstOrDefault();
                    if(existing == null)
                    {
                        ViewData["ErrorMsg"] = "Oops! Drive you are trying to edit does not exist";
                        return View("~/Views/Home/ShowError.cshtml"); // drive you are trying to edit does not exist
                    }
                    var uID = User.Claims.FirstOrDefault(x => x.Type == "id");
                    var uType = User.Claims.FirstOrDefault(x => x.Type == "Type");
                    if (uID == null || uType == null)
                    {
                        ViewData["ErrorMsg"] = "Oops! You are not logged in. Please login.";
                        return View("~/Views/Home/ShowError.cshtml"); // user not logged in
                    }
                    if (Int32.Parse(uID.Value) != existing.UserId.Id && !uType.Value.Equals("Admin")) {
                        ViewData["ErrorMsg"] = "Oops! You are not the owner of the drive and not admin!";
                        return View("~/Views/Home/ShowError.cshtml"); // user is not the owner of the drive and not admin
                    }
                    if(uType.Value.Equals("Admin") && Types != null)
                    {
                        DriveType t = _context.DriveType.Where(dt => dt.Id == Types).FirstOrDefault();
                        if(t != null)
                        {
                            existing.TypeId = t;
                        }
                    }
                    
                    existing.Description = drive.Description;
                    _context.Update(existing);
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
                return RedirectToAction("Details", new { id = id });
            }
            return View(drive);
        }

        // GET: Drives/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var q = from d in _context.Drive
                    join u in _context.User on d.Id equals u.D.Id into sub1
                    from s in sub1.DefaultIfEmpty()
                    join im in _context.Image on d.Id equals im.DId.Id into sub2
                    from subq in sub2.DefaultIfEmpty() // left outer join (for empty drives)
                    where d.Id == id
                    select new {userObj = d.UserId, driveObj = d, image = subq };
            
            if(q.Count() < 1)
            {
                ViewData["ErrorMsg"] = "Oops! You cannot delete because the drive does not exist.";
                return View("~/Views/Home/ShowError.cshtml"); // drive does not exist. can't delete
            }
            _context.Drive.Remove(q.First().driveObj);
            q.ToList().ForEach(i =>{ if (i.image != null) _context.Image.Remove(i.image); }); // iterate over results and delete each image
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Fetch drive (including user to verify permissions, and images to count current capacity)
            var drive = await _context.Drive.Include(d => d.UserId).Include(d => d.TypeId).Include(d => d.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (drive == null)
            {
                return NotFound();
            }
            var UserId = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "id");
            if(UserId == null)
            {
                ViewData["ErrorMsg"] = "Oops! You are not logged in. Please login.";
                return View("~/Views/Home/ShowError.cshtml"); // user not logged in.
            }

            // convert id string to int and compare drive's owner to the current user
            if (drive.UserId.Id != Int32.Parse(UserId.Value))
            {
                ViewData["ErrorMsg"] = "Oops! You do not have permissions to fill this drive!";
                return View("~/Views/Home/ShowError.cshtml");// no permission
            }


            // fill drive with web service: Pixabay API
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
                    // parse response text into JSON
                    var Response = Res.Content.ReadAsStringAsync().Result;
                    JObject parsed = JObject.Parse(Response);

                    // create new image object from recieved data

                    int freeSlots = drive.TypeId.Max_Capacity - drive.Current_usage; // if negative nothing will be added
                    freeSlots = Math.Min(freeSlots, parsed["hits"].Count()); // limit the number of images to the number of results
                    freeSlots = Math.Min(freeSlots, max_allowed);

                    Tag pixabay_tag = _context.Tag.Where(t => t.Name.Equals("Pixabay")).FirstOrDefault();
                    for (int i = 0; i < freeSlots; i++) {
                        Image I = new Image() { DId = drive, UploadTime = DateTime.Now, IsPublic = true, EditTime = DateTime.Now };
                        using (WebClient c = new WebClient())
                        {
                            byte[] image = c.DownloadData(parsed["hits"][i]["webformatURL"].ToString());
                            I.Data = image;
                        }
                        I.Description = "this image was originally uploaded by Pixabay/" + parsed["hits"][i]["user"].ToString();
                        if (pixabay_tag != null)
                        {
                            I.Tags = new List<Tag>();
                            ((List<Tag>)I.Tags).Add(pixabay_tag);
                        }
                        _context.Image.Update(I);
                        drive.Current_usage++;
                    }
                    drive.Current_usage = drive.Images.Count();
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
        public async void updateCookies(string userName, InternetAppProject.Models.User.UserType userType, int id, int drive, bool mode)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            string type = Enum.GetName(typeof(InternetAppProject.Models.User.UserType), userType);
            string sid = id.ToString();
            string did = drive.ToString();
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, type),
                    new Claim("Type", type),
                    new Claim("id", sid), // user.Id
                    new Claim("drive", did), // user.D.Id
                    new Claim("nightMode", mode.ToString())
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

        public async Task<IActionResult> Search(string id)
        {
            if (id == null)
            {
                return View();
            }
            var drives = await _context.Drive.Include(d => d.UserId).Where(u => u.UserId.Name.Contains(id)).ToListAsync();
            return View(drives);
        }
        public async Task<IActionResult> SearchJson(string id)
        {
            if (id == null)
            {
                return Json(new List<User>());
            }
            var q = from d in _context.Drive
                    join u in _context.User on d.UserId.Id equals u.Id
                    where d.UserId.Name.Contains(id)
                    select new { id = d.Id, name = d.UserId.Name, usage = d.Current_usage };

            return Json(await q.ToListAsync());
        }
    }
}
