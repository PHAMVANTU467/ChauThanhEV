using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly MockDataService _data;

        public CustomersController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, string? status, int page = 1)
        {
            var vm = _data.SearchCustomers(keyword, status, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerFormModel form)
        {
            if (string.IsNullOrWhiteSpace(form.FullName) || string.IsNullOrWhiteSpace(form.Phone))
            {
                this.SetToast("Vui lòng nhập đầy đủ họ tên và số điện thoại.", "error");
                return RedirectToAction("Index");
            }

            _data.AddCustomer(form);
            this.SetToast($"Đã thêm khách hàng \"{form.FullName}\" thành công.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CustomerFormModel form)
        {
            var ok = _data.UpdateCustomer(form);
            this.SetToast(ok ? "Đã cập nhật thông tin khách hàng." : "Không tìm thấy khách hàng cần cập nhật.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var ok = _data.DeleteCustomer(id);
            this.SetToast(ok ? "Đã xóa khách hàng khỏi hệ thống." : "Không tìm thấy khách hàng cần xóa.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdjustBalance(AdjustBalanceModel form)
        {
            var ok = _data.AdjustBalance(form.CustomerId, form.Amount, form.Note);
            var verb = form.Amount >= 0 ? "cộng" : "trừ";
            this.SetToast(ok
                ? $"Đã {verb} {MockDataService.FormatCurrency(Math.Abs(form.Amount))} vào ví khách hàng."
                : "Không tìm thấy khách hàng.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
