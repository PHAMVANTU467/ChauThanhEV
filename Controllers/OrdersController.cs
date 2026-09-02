using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly MockDataService _data;

        public OrdersController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string tab = "charging", string? keyword = null, string? status = null, int page = 1)
        {
            var vm = new OrderListViewModel { Tab = tab, Keyword = keyword, StatusFilter = status };

            if (tab == "topup")
            {
                vm.TopUpResult = _data.SearchTopUpOrders(keyword, status, page);
            }
            else
            {
                vm.Tab = "charging";
                vm.ChargingResult = _data.SearchChargingOrders(keyword, status, page);
            }

            return View(vm);
        }
    }
}
