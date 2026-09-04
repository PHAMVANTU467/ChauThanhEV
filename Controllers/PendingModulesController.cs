using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class UnregisteredChargersController : Controller
    {
        private readonly MockDataService _data;
        public UnregisteredChargersController(MockDataService data) => _data = data;

        public IActionResult Index() => View("~/Views/Shared/ModulePlaceholder.cshtml", new ModulePlaceholderViewModel
        {
            Title = "Trụ sạc chưa đăng ký",
            Description = "Theo dõi các trụ sạc OCPP đang chờ gán vào trạm.",
            Columns = new[] { "Mã trụ", "Model", "Địa chỉ OCPP", "Lần kết nối cuối", "Trạng thái" },
            Statuses = new[] { "Chờ đăng ký", "Đang kết nối" },
            Rows = Enumerable.Range(1, 5).Select(i => $"CHUA-GAN-{i:000}|DC Fast Charger {120 + i * 30} kW|ocpp-gateway-0{i}.chauthanhev.mock|{_data.Now.AddMinutes(-i * 18):dd/MM/yyyy HH:mm}|Chờ đăng ký").ToArray()
        });
    }

    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly MockDataService _data;
        public InvoicesController(MockDataService data) => _data = data;

        public IActionResult Index() => View("~/Views/Shared/ModulePlaceholder.cshtml", new ModulePlaceholderViewModel
        {
            Title = "Thông tin hóa đơn",
            Description = "Tham khảo thông tin hóa đơn phát sinh từ các đơn sạc.",
            Columns = new[] { "Mã hóa đơn", "Mã đơn sạc", "Khách hàng", "Ngày phát hành", "Trạng thái" },
            Statuses = new[] { "Đã phát hành", "Chờ phát hành" },
            Rows = _data.ChargingOrders.OrderByDescending(order => order.StartTime).Take(8).Select((order, index) => $"CT-HĐ-{order.Id:0000}|{order.Code}|{_data.Customers.FirstOrDefault(customer => customer.Id == order.CustomerId)?.FullName}|{order.StartTime:dd/MM/yyyy}|{(order.Status == ChargingOrderStatus.Completed ? "Đã phát hành" : "Chờ phát hành")}").ToArray()
        });
    }

    [Authorize]
    public class InternalTransactionsController : Controller
    {
        private readonly MockDataService _data;
        public InternalTransactionsController(MockDataService data) => _data = data;

        public IActionResult Index() => View("~/Views/Shared/ModulePlaceholder.cshtml", new ModulePlaceholderViewModel
        {
            Title = "Lịch sử giao dịch nội bộ",
            Description = "Tra cứu biến động số dư và các giao dịch nội bộ của hệ thống.",
            Columns = new[] { "Mã giao dịch", "Loại giao dịch", "Số tiền", "Thời gian", "Trạng thái" },
            Statuses = new[] { "Hoàn thành", "Đang xử lý" },
            Rows = _data.Transactions.OrderByDescending(transaction => transaction.TransactionTime).Take(12).Select(transaction => $"{transaction.TransactionId}|{transaction.TransactionType}|{MockDataService.FormatCurrency(transaction.Amount)}|{transaction.TransactionTime:dd/MM/yyyy HH:mm}|Hoàn thành").ToArray()
        });
    }
}
