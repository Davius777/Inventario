using System;
using System.Web.Mvc;

namespace Inventario.Controllers
{
    public class BaseController : Controller
    {
        private string userName;
        private int userId;
        private string empresaNombre;
        private int empresaId;
        public string UserName {
            get {
                return Environment.UserDomainName + "\\" + Environment.UserName;
            }
        }

        public int UserId {
            get {
                return 1;
            }
        }

        public string EmpresaNombre {
            get {
                return "Bimbo";
            }
        }

        public int EmpresaId {
            get {
                return 1;
            }
        }
    }
}