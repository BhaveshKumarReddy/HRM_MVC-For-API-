using EncryptionLibrary;
using HRM_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HRM_MVC.Controllers
{
    public class HrListController : Controller
    {
        private readonly HRM_DB_ProjectContext db;
        RailFence cipher = new RailFence();
        public HrListController(HRM_DB_ProjectContext _db)
        {
            db = _db;
        }
        public async Task<ActionResult> display_HRs()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                List<HrList> all_hrs = new();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var Res = await client.GetAsync("https://localhost:44392/api/HrList"))
                    {
                        var Response = await Res.Content.ReadAsStringAsync();
                        if (Res.IsSuccessStatusCode)
                        {
                            all_hrs = JsonConvert.DeserializeObject<List<HrList>>(Response);
                        }
                        else
                        {
                            throw new Exception(Response);
                        }
                        return View(all_hrs);
                    }
                }
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
        public async Task<IActionResult> create_HR()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                List<LocationList> all_locations = new();
                using(var client = new HttpClient())
                {
                    using(var res = await client.GetAsync("https://localhost:44392/api/LocationList"))
                    {
                        string Response = await res.Content.ReadAsStringAsync();
                        if (res.IsSuccessStatusCode)
                        {
                            all_locations = JsonConvert.DeserializeObject<List<LocationList>>(Response);
                        }
                        else
                        {
                            throw new Exception(Response);
                        }
                    }
                }
                ViewBag.LocationId = all_locations.Select(x => new SelectListItem { Value = x.LocationId.ToString(), Text = x.LocationName }).ToList();
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

        [HttpPost]
        public async Task<IActionResult> create_HR(HrList hr)
        {
            try
            {
                HrList new_hr = hr;
                new_hr.HrPassword = cipher.Encrypt(hr.HrPassword);
                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(new_hr),Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync("https://localhost:44392/api/HrList", content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            new_hr = JsonConvert.DeserializeObject<HrList>(apiResponse);
                            HttpContext.Session.SetString("Location", hr.LocationId);
                            HttpContext.Session.SetString("Email", hr.HrEmail);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            throw new Exception(apiResponse);
                        }
                    }
                }
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
        [HttpGet]
        public IActionResult login_HR()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("handleException", "Home", new { message = e.Message });
            }
        }
        public async Task<IActionResult> login_HR(HrList hr)
        {
            try
            {
                string encrypted_password = cipher.Encrypt(hr.HrPassword);
                HrList login_account = new();
                using (var client = new HttpClient())
                {
                    using (var Res = await client.GetAsync("https://localhost:44392/api/HrList/" + hr.HrEmail))
                    {
                        var Response = Res.Content.ReadAsStringAsync().Result;
                        if (Res.IsSuccessStatusCode)
                        {
                            login_account = JsonConvert.DeserializeObject<HrList>(Response);
                            if (login_account != null && login_account.HrPassword == encrypted_password)
                            {
                                HttpContext.Session.SetString("Location", login_account.LocationId);
                                HttpContext.Session.SetString("Email", login_account.HrEmail);
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError(String.Empty,"login details are incorrect");
                                return View();
                            }
                        }
                        else if(Res.ReasonPhrase.ToString() == "Not Found")
                        {
                            ModelState.AddModelError("HrEmail", "Email not found");
                            return View();
                        }
                        else
                        {
                            throw new Exception(Response);
                        }
                    }
                }
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
        
        public IActionResult logout_HR()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("handleException", "Home", new { message = e.Message });
            }
        }
        
        public IActionResult confirmLogout_HR()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                HttpContext.Session.Clear();
                return RedirectToAction("login_HR");
            }
            catch (Exception e)
            {
                return RedirectToAction("handleException", "Home", new { message = e.Message });
            }
        }
        
        public async Task<IActionResult> edit_HR()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                string email = HttpContext.Session.GetString("Email");
                using (var client = new HttpClient())
                {
                    using(var res = await client.GetAsync("https://localhost:44392/api/HrList/" + email))
                    {
                        string Response = await res.Content.ReadAsStringAsync();
                        if (res.IsSuccessStatusCode)
                        {
                            var account_details = JsonConvert.DeserializeObject<HrList>(Response);
                            account_details.HrPassword = cipher.Decrypt(account_details.HrPassword);
                            return View(account_details);
                        }
                        else
                        {
                            throw new Exception(Response);
                        }
                    }
                }
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
        
        [HttpPost]
        public async Task<IActionResult> edit_HR(HrList hr)
        {
            try
            {
                HrList new_hr = hr;
                new_hr.HrPassword = cipher.Encrypt(hr.HrPassword);
                using (var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(new_hr), Encoding.UTF8, "application/json");
                    using(var res = await client.PutAsync("https://localhost:44392/api/HrList", content))
                    {
                        var Response = await res.Content.ReadAsStringAsync();
                        if (res.IsSuccessStatusCode)
                        {
                            new_hr = JsonConvert.DeserializeObject<HrList>(Response);
                            HttpContext.Session.SetString("Location", new_hr.LocationId);
                            HttpContext.Session.SetString("Email", new_hr.HrEmail);
                        }
                        else
                        {
                            throw new Exception(Response);
                        }
                    }
                }
                return RedirectToAction("Index", "Home");
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
        
        // Remote Validation for HR - Email
        [HttpPost]
        public async Task<JsonResult> IsEmailAvailable(HrList hr)
        {
            using (var client = new HttpClient())
            {
                using (var res = await client.GetAsync("https://localhost:44392/api/HrList/" + hr.HrEmail))
                {
                    if (res.IsSuccessStatusCode)
                    {
                        return Json(false);
                    }
                }
            }
            return Json(true);
        }

        // Remote Validation for Employee - DateOfJoining
        public async Task<JsonResult> IsDOJValid(EmployeesList emp)
        {
            if (emp.EmployeeDoj <= DateTime.Today)
            {
                return Json(true);
            }
            return Json(false);
        }

        // Remote Validation for Employee - Appraisaldate
        public async Task<JsonResult> IsAppraisalDateValid(EmployeesList emp)
        {
            if (emp.EmployeeAppraisalDate >= DateTime.Today)
            {
                return Json(true);
            }
            return Json(false);
        }
    }
}
