using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class ChargersController : Controller
    {
        private readonly MockDataService _data;

        public ChargersController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchChargers(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChargerFormModel form)
        {
            if (string.IsNullOrWhiteSpace(form.Name))
            {
                this.SetToast("Vui lòng nhập đầy đủ thông tin trụ sạc.", "error");
                return RedirectToAction("Index");
            }

            _data.AddCharger(form);
            this.SetToast($"Đã thêm trụ sạc \"{form.Name}\" thành công.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChargerFormModel form)
        {
            var ok = _data.UpdateCharger(form);
            this.SetToast(ok ? "Đã cập nhật trụ sạc thành công." : "Không tìm thấy trụ sạc cần cập nhật.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ok = _data.DeleteCharger(id);
            this.SetToast(ok ? "Đã xóa trụ sạc khỏi hệ thống." : "Không tìm thấy trụ sạc cần xóa.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
