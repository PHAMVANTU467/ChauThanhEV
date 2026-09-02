using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class OperationsController : Controller
    {
        private readonly MockDataService _data;

        public OperationsController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string tab = "faults")
        {
            var vm = _data.GetOperations(tab);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResolveFault(int id, string? note, string tab = "faults")
        {
            var ok = _data.ResolveFault(id, note);
            this.SetToast(ok ? "Đã xử lý lỗi và cập nhật trạng thái cổng sạc." : "Không tìm thấy lỗi cần xử lý.", ok ? "success" : "error");
            return RedirectToAction("Index", new { tab });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReactivateCharger(int id, string tab = "offline")
        {
            var ok = _data.ReactivateCharger(id);
            this.SetToast(ok ? "Đã kích hoạt lại trụ sạc, trụ đã hoạt động trở lại." : "Không tìm thấy trụ sạc.", ok ? "success" : "error");
            return RedirectToAction("Index", new { tab });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResolveAbnormal(int id, string? resolutionNote, string tab = "abnormal")
        {
            var ok = _data.ResolveAbnormalOrder(id, string.IsNullOrWhiteSpace(resolutionNote) ? "Đã kiểm tra và xử lý." : resolutionNote);
            this.SetToast(ok ? "Đã đánh dấu đơn hàng bất thường là đã xử lý." : "Không tìm thấy đơn hàng.", ok ? "success" : "error");
            return RedirectToAction("Index", new { tab });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdjustAmount(int id, decimal newAmount, string? note, string tab = "abnormal")
        {
            var ok = _data.AdjustAbnormalOrderAmount(id, newAmount, note ?? "");
            this.SetToast(ok ? "Đã điều chỉnh số tiền của đơn hàng." : "Không tìm thấy đơn hàng.", ok ? "success" : "error");
            return RedirectToAction("Index", new { tab });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refund(int id, decimal refundAmount, string? note, string tab = "abnormal")
        {
            var ok = _data.RefundAbnormalOrder(id, refundAmount, note ?? "");
            this.SetToast(ok ? "Đã hoàn tiền vào ví khách hàng." : "Không tìm thấy đơn hàng.", ok ? "success" : "error");
            return RedirectToAction("Index", new { tab });
        }
    }
}
