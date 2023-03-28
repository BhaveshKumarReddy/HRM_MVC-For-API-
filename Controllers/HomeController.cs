using HRM_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRM_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HRM_DB_ProjectContext db;

        public HomeController(ILogger<HomeController> logger, HRM_DB_ProjectContext _db)
        {
            _logger = logger;
            db = _db;
        }
        public IActionResult handleException(string message = "Exception")
        {
            ViewBag.Emessage = message;
            return View();
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                List<LocationList> all_locations = new();
                using (var client = new HttpClient())
                {
                    using (var res = await client.GetAsync("https://localhost:44392/api/LocationList"))
                    {
                        if(res.IsSuccessStatusCode)
                        {
                            string Response = await res.Content.ReadAsStringAsync();
                            all_locations = JsonConvert.DeserializeObject<List<LocationList>>(Response);
                        }
                        else
                        {
                            throw new Exception("Check API Connection!");
                        }
                    }
                }
                ViewBag.all_locations = all_locations.ToList();
                if (HttpContext.Session.GetString("Email") != null)
                {
                    string email = HttpContext.Session.GetString("Email");
                    ViewBag.location = all_locations.FirstOrDefault(x => x.LocationId == HttpContext.Session.GetString("Location")).LocationName;
                    return View();
                }
                return View();
            }
            catch (JsonException)
            {
                return RedirectToAction("handleException", "Home", new { message = "JSON serialization and deserialization error!" });
            }
            catch (HttpRequestException)
            {
                return RedirectToAction("handleException", "Home", new { message = "HttptClient Exception!" });
            }
            catch (SqlException)
            {
                return RedirectToAction("handleException", "Home", new { message = "Sql Server returned an error!" });
            }
            catch (Exception e)
            {
                return RedirectToAction("handleException", "Home", new { message = e.Message });
            }
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
    }
}
