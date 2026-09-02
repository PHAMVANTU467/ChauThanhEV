using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class FinancialManagementController : Controller
    {
        private readonly MockDataService _data;

        public FinancialManagementController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword, int page = 1)
        {
            var vm = _data.GetFinancialViewModel(keyword, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Withdraw(decimal amount, string bankAccount)
        {
            if (amount <= 0 || string.IsNullOrWhiteSpace(bankAccount))
            {
                this.SetToast("Vui lòng nhập số tiền và tài khoản ngân hàng hợp lệ.", "error");
                return RedirectToAction("Index");
            }

            var ok = _data.WithdrawBalance(amount, bankAccount);
            this.SetToast(ok ? "Đã tạo lệnh rút tiền thành công." : "Số dư không đủ để thực hiện rút tiền.", ok ? "success" : "error");
            return RedirectToAction("Index");
        }
    }
}
