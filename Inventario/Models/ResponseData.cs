using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inventario.Models {
    public class ResponseData {
        public ResponseData() { }

        public object Data {get; set;}
        public bool Error { get; set; }
        public string Mensaje { get; set; }
    }
}