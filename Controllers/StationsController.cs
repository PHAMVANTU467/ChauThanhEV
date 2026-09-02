using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class StationsController : Controller
    {
        private readonly MockDataService _data;

        public StationsController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchStations(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StationFormModel form)
        {
            if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Address))
            {
                this.SetToast("Vui lòng nhập đầy đủ thông tin trạm.", "error");
                return RedirectToAction("Index");
            }

            _data.AddStation(form);
            this.SetToast($"Đã thêm trạm \"{form.Name}\" thành công.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StationFormModel form)
        {
            var ok = _data.UpdateStation(form);
            this.SetToast(ok ? "Đã cập nhật trạm thành công." : "Không tìm thấy trạm cần cập nhật.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ok = _data.DeleteStation(id);
            this.SetToast(ok ? "Đã xóa trạm khỏi hệ thống." : "Không tìm thấy trạm cần xóa.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
