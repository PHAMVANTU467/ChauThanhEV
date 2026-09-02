using System.Text.Json;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MockDataService _data;

        public HomeController(MockDataService data)
        {
            _data = data;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public IActionResult Index()
        {
            var model = _data.GetDashboard();
            model.AdminName = User.Identity?.Name ?? "admin";

            ViewBag.RevenueTodayJson = JsonSerializer.Serialize(model.RevenueToday, JsonOptions);
            ViewBag.Revenue7DaysJson = JsonSerializer.Serialize(model.Revenue7Days, JsonOptions);
            ViewBag.Revenue30DaysJson = JsonSerializer.Serialize(model.Revenue30Days, JsonOptions);
            ViewBag.ActiveUsersTodayJson = JsonSerializer.Serialize(model.ActiveUsersToday, JsonOptions);
            ViewBag.ActiveUsers7DaysJson = JsonSerializer.Serialize(model.ActiveUsers7Days, JsonOptions);
            ViewBag.ActiveUsers30DaysJson = JsonSerializer.Serialize(model.ActiveUsers30Days, JsonOptions);
            ViewBag.DoughnutJson = JsonSerializer.Serialize(new
            {
                labels = new[] { "Available", "Charging", "Fault" },
                values = new[] { model.AvailableConnectors, model.ChargingConnectors, model.FaultConnectors }
            }, JsonOptions);

            return View(model);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ChauThanhEV.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
