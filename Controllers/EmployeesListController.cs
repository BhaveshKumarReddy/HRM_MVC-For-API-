using HRM_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRM_MVC.Controllers
{
    public class EmployeesListController : Controller
    {
        private readonly HRM_DB_ProjectContext db;
        public EmployeesListController(HRM_DB_ProjectContext _db)
        {
            db = _db;
        }

        [HttpGet]
        public async Task<IActionResult> display_Employees()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                string location_id = HttpContext.Session.GetString("Location");
                List<EmployeesList> employees_list = new();
                List<LocationList> all_locations = new();
                using (var client = new HttpClient())
                {
                    using (var res = await client.GetAsync("https://localhost:44392/api/EmployeesList/" + location_id))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            var Response = await res.Content.ReadAsStringAsync();
                            employees_list = JsonConvert.DeserializeObject<List<EmployeesList>>(Response);
                        }
                        else
                        {
                            throw new Exception("Error occured while fetching details!");
                        }
                    }
                    using (var res = await client.GetAsync("https://localhost:44392/api/LocationList"))
                    {
                        var Response = await res.Content.ReadAsStringAsync();
                        all_locations = JsonConvert.DeserializeObject<List<LocationList>>(Response);
                    }
                    ViewBag.location_name = all_locations.FirstOrDefault(x => x.LocationId == location_id).LocationName;
                    return View(employees_list);
                }
            }
            catch (JsonException)
            {
                return RedirectToAction("handleException", "Home", new { message =  "JSON serialization and deserialization error!"});
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
        public async Task<IActionResult> add_Employee()
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                string hr_location = HttpContext.Session.GetString("Location");
                ViewBag.LocationId = hr_location;
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("handleException", "Home", new { message = e.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> add_Employee(EmployeesList employee)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                using(var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");
                    using (var res = await client.PostAsync("https://localhost:44392/api/EmployeesList", content))
                    {
                        if (!res.IsSuccessStatusCode)
                        {
                            throw new Exception("Error occured while adding employee!");
                        }
                        var Response = await res.Content.ReadAsStringAsync();
                    }
                }
                return RedirectToAction("display_Employees");
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
        public async Task<IActionResult> open_Employee(int id)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                EmployeesList employee = new();
                List<LocationList> all_locations = new();
                using (var client = new HttpClient())
                {
                    using (var res = await client.GetAsync("https://localhost:44392/api/EmployeesList/getEmployeeByID/" + id))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            var Response = await res.Content.ReadAsStringAsync();
                            employee = JsonConvert.DeserializeObject<EmployeesList>(Response);
                            ViewBag.eligible = (employee.EmployeeAppraisalDate <= DateTime.Today);
                        }
                        else
                        {
                            throw new Exception("Error occured while fetching details!");
                        }
                    }
                    using (var res = await client.GetAsync("https://localhost:44392/api/LocationList"))
                    {
                        string Response = await res.Content.ReadAsStringAsync();
                        all_locations = JsonConvert.DeserializeObject<List<LocationList>>(Response);
                    }
                    ViewBag.locations = all_locations.Select(x => new SelectListItem { Value = x.LocationId.ToString(), Text = x.LocationName }).ToList();
                    return View(employee);
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
        public async Task<IActionResult> edit_Details(EmployeesList employee)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                using(var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");
                    using (var res = await client.PutAsync("https://localhost:44392/api/EmployeesList/edit_Employee", content))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            var Response = await res.Content.ReadAsStringAsync();
                            return RedirectToAction("display_Employees");
                        }
                        else
                        {
                            throw new Exception("Error occured while editing info!");
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
        public async Task<IActionResult> increment_Salary(EmployeesList employee)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                using(var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");
                    using(var res = await client.PutAsync("https://localhost:44392/api/EmployeesList/increment_Salary/id/", content))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            return RedirectToAction("open_Employee", new { id = employee.EmployeeId });
                        }
                        else
                        {
                            throw new Exception("Error occured while incrementing salary!");
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
        public async Task<IActionResult> transfer_Employee(EmployeesList employee)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                using (var client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");
                    using (var res = await client.PutAsync("https://localhost:44392/api/EmployeesList/transfer_Employee/" + employee.EmployeeId + "/" + employee.LocationId, content))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            return RedirectToAction("display_Employees", new { id = employee.EmployeeId });
                        }
                        else
                        {
                            throw new Exception("Error occured while transfering employee!");
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
        public async Task<IActionResult> fireEmployee(EmployeesList employee)
        {
            try
            {
                if (HttpContext.Session.GetString("Email") == null)
                {
                    return RedirectToAction("login_HR", "HrList");
                }
                using(var client = new HttpClient())
                {
                    using (var res = await client.DeleteAsync("https://localhost:44392/api/EmployeesList/fire_Employee/" + employee.EmployeeId))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            return RedirectToAction("display_Employees", new { id = employee.EmployeeId });
                        }
                        else
                        {
                            throw new Exception("Error occured while firing employee!");
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
    }
}
