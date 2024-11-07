using Inventario.Models;
using System.Web.Mvc;

namespace Inventario.Controllers {
    public class HomeController : BaseController {
        public ActionResult Index() {
            BaseViewModel model = new BaseViewModel { Id = UserId, Name = UserName, Empresa = EmpresaNombre };
            return View("Index",model);
        }
    }
}