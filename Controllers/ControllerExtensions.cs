using Microsoft.AspNetCore.Mvc;

namespace ChauThanhEV.Controllers
{
    public static class ControllerExtensions
    {
        public static void SetToast(this Controller controller, string message, string type = "success")
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = type; // success | error
        }
    }
}
