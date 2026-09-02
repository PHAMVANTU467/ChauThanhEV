using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly MockDataService _data;

        public ReservationsController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchReservations(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReservationFormModel form)
        {
            var result = _data.CreateReservation(form);
            if (!result.Success)
            {
                this.SetToast(result.Message, "error");
                return RedirectToAction("Index");
            }

            this.SetToast("Đã tạo đặt chỗ thành công.");
            return RedirectToAction("Index");
        }
    }
}
