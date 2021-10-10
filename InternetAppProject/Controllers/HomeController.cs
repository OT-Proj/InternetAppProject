using InternetAppProject.Models;
using InternetAppProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InternetAppProject.Controllers
{

    // good luck, test push!
    public class HomeController : Controller
    {
        private readonly StartupService _startup;

        public HomeController(StartupService startup)
        {
            _startup = startup;
        }

        public async Task<IActionResult> Index()
        {
            await _startup.InitDB();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ShowError(string id)
        {
            ViewData["ErrorMsg"] = id;
            return View();

        }
    }
}
