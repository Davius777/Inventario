using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventario.Controllers
{
    public class ReporteController : Controller
    {
        // GET: Reporte
        public ActionResult Inventario()
        {
            ViewBag.Message = "Inventario.";
            return View();
        }
        public ActionResult Movimiento() {
            ViewBag.Message = "Movimientos.";
            return View();
        }
    }
}