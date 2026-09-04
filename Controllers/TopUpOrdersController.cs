using ChauThanhEV.Models;
using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    [Authorize]
    public class TopUpOrdersController : Controller
    {
        private readonly MockDataService _data;

        public TopUpOrdersController(MockDataService data)
        {
            _data = data;
        }

        public IActionResult Index(string? keyword = null, string? status = null, int page = 1)
        {
            var vm = new TopUpOrderListViewModel
            {
                Keyword = keyword,
                StatusFilter = status,
                Result = _data.SearchTopUpOrders(keyword, status, page)
            };

            return View(vm);
        }
    }
}
