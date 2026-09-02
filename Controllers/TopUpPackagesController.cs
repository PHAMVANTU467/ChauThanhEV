using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class TopUpPackagesController : Controller
    {
        private readonly MockDataService _data;

        public TopUpPackagesController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchTopUpPackages(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TopUpPackageFormModel form)
        {
            if (string.IsNullOrWhiteSpace(form.Name) || form.Amount <= 0)
            {
                this.SetToast("Vui lòng nhập tên gói và số tiền hợp lệ.", "error");
                return RedirectToAction("Index");
            }

            _data.AddTopUpPackage(form);
            this.SetToast($"Đã thêm gói \"{form.Name}\" thành công.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TopUpPackageFormModel form)
        {
            var ok = _data.UpdateTopUpPackage(form);
            this.SetToast(ok ? "Đã cập nhật gói nạp tiền." : "Không tìm thấy gói cần cập nhật.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ok = _data.DeleteTopUpPackage(id);
            this.SetToast(ok ? "Đã xóa gói nạp tiền." : "Không tìm thấy gói cần xóa.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
