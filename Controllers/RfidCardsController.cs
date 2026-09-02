using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class RfidCardsController : Controller
    {
        private readonly MockDataService _data;

        public RfidCardsController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchRfidCards(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RfidCardFormModel form)
        {
            if (string.IsNullOrWhiteSpace(form.CardId) || form.UserId <= 0)
            {
                this.SetToast("Vui lòng nhập mã thẻ và chọn người dùng.", "error");
                return RedirectToAction("Index");
            }

            _data.AddRfidCard(form);
            this.SetToast($"Đã thêm thẻ RFID {form.CardId} thành công.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ok = _data.DeleteRfidCard(id);
            this.SetToast(ok ? "Đã xóa thẻ RFID." : "Không tìm thấy thẻ RFID cần xóa.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var ok = _data.ToggleRfidCardStatus(id);
            this.SetToast(ok ? "Đã cập nhật trạng thái thẻ RFID." : "Không tìm thấy thẻ RFID.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
