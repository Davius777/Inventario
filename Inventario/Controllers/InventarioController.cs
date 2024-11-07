using Inventario.DTO;
using Inventario.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;
using System.Linq;
using Inventario.Models.ModelBD;

namespace Inventario.Controllers
{
    public class InventarioController : BaseController
    {
        // GET: Inventario
        public ActionResult Stock()
        {
            ViewBag.Message = "Stock.";
            return View();
        }
        public ActionResult Entradas() {
            ViewBag.Message = "Entradas.";
            return View();
        }
        public ActionResult Salidas() {
            ViewBag.Message = "Salidas.";
            return View();
        }
        public ActionResult CMasiva() {
            ViewBag.Message = "Carga masiva.";
            return View();
        }

        public JsonResult CatMedicamentos(string search) {
            ResponseData rs = new ResponseData();
            try {
                List<MedicamentoDTO> ls = InventarioLogic.ObtenMedicamentos().Where(m => m.Nombre.Contains(search.ToUpper())).ToList();
                rs = new ResponseData { Error = false, Data = ls };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        public JsonResult CatPresentacion() {
            ResponseData rs = new ResponseData();
            try {
                List<CatPresentacionDTO> ls = InventarioLogic.ObtenPresentaciones();
                rs = new ResponseData { Error = false, Data = ls };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        public JsonResult Guardar(MedicamentoDTO medicamento) {
            ResponseData rs = new ResponseData();
            try {
                if(medicamento.Cantidad > 0) {
                    InventarioLogic.Guardar(medicamento, UserId, EmpresaId);
                    rs = new ResponseData { Error = false, Mensaje = "Medicamento guardado exitosamente" };
                }
                else
                    rs = new ResponseData { Error = true, Mensaje = "La cantidad debe ser mayor a cero" };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        public JsonResult GuardarLista(List<FilaValidacion> medicamentos) {
            ResponseData rs = new ResponseData();
            try {
                foreach (var fila in medicamentos) {
                    MedicamentoDTO medicamento = new MedicamentoDTO {
                        Nombre = fila.Nombre,
                        IdPresentacion = fila.IdPresentacion,
                        Cantidad = fila.Cantidad,
                        Observaciones = ""
                    };
                    InventarioLogic.Guardar(medicamento, UserId, EmpresaId);
                }
                rs = new ResponseData { Error = false, Mensaje = "Medicamentos guardados exitosamente" };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        public JsonResult Obtener(MedicamentoDTO medicamento) {
            ResponseData rs = new ResponseData();
            try {
                if (medicamento.Cantidad > 0) {
                    int cantidadActual = InventarioLogic.ValidarExistencia(medicamento, EmpresaId);
                    if (cantidadActual != -1) {
                        if (cantidadActual >= medicamento.Cantidad) {
                            InventarioLogic.Obtener(medicamento, UserId, EmpresaId);
                            rs = new ResponseData { Error = false, Mensaje = "Medicamento surtido exitosamente" };
                        }
                        else
                            rs = new ResponseData { Error = true, Mensaje = "La cantidad solicitada excede la cantidad en el inventario." };
                    }
                    else
                        rs = new ResponseData { Error = true, Mensaje = "El medicamento solicitado no ha sido registrado." };
                }
                else
                    rs = new ResponseData { Error = true, Mensaje = "La cantidad debe ser mayor a cero" };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        public JsonResult Listar() {
            ResponseData rs = new ResponseData();
            try {
                List<MedicamentoDTO> ls = InventarioLogic.Listar(EmpresaId);
                rs = new ResponseData { Error = false, Mensaje = "", Data = ls };
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }

        public JsonResult ValidarPlantilla(HttpPostedFileBase excelFile) {
            ResponseData rs = new ResponseData();
            try {
                if (Request.Files.Count > 0) {
                    foreach (string file in Request.Files) {
                        excelFile = Request.Files[file];
                    }
                    TempData["ExcelFile"] = excelFile;
                    string mensaje = "";
                    List<FilaValidacion> ls = ValidarExcel(excelFile.InputStream, ref mensaje);
                    rs = new ResponseData { Error = !string.IsNullOrEmpty(mensaje), Mensaje = mensaje, Data = ls };
                }
            }
            catch (Exception ex) {
                rs = new ResponseData { Error = true, Mensaje = ex.Message };
            }
            return Json(rs);
        }
        private List<FilaValidacion> ValidarExcel(Stream archivo, ref string mensaje) {
            List<FilaValidacion> ls = new List<FilaValidacion>();
            Dictionary<int, string> dcPresentacion = InventarioLogic.ObtenDiccionarioPresentaciones();
            XLWorkbook wb = new XLWorkbook(archivo);
            var rows = wb.Worksheet(1).Rows().Skip(1);
            if (rows.Count() > 0) {
                foreach (var row in rows) {
                    FilaValidacion fila = new FilaValidacion {
                        NumeroFila = row.RowNumber(),
                        Estatus = true,
                        Mensaje = ""
                    };
                    string nombre = "";
                    string presentacion = "";
                    int cantidad = 0;
                    // Validacion de nombre del medicamento.
                    if (row.Cell(1).TryGetValue(out nombre))
                        fila.Nombre = nombre.ToUpper();
                    else {
                        fila.Nombre = row.Cell(0).ToString();
                        fila.Mensaje += "El valor del Medicamento no es de tipo texto.\n";
                        fila.Estatus = false;
                    }
                    // Validacion de tipo de presentacion.
                    if (row.Cell(2).TryGetValue(out presentacion)) {
                        fila.Presentacion = presentacion;
                        int valPresentacion = ValidaPresentacion(dcPresentacion, presentacion);
                        if(valPresentacion != 0)
                            fila.IdPresentacion = valPresentacion;
                        else {
                            fila.Mensaje += "El valor de Presentación no existe en el catálogo.\n";
                            fila.Estatus = false;
                        }

                    }
                    else {
                        fila.Presentacion = row.Cell(1).ToString();
                        fila.Mensaje += "El valor de Presentación no es de tipo texto.\n";
                        fila.Estatus = false;
                    }
                    // Validacion de cantidad.
                    if (row.Cell(3).TryGetValue(out cantidad))
                        fila.Cantidad = cantidad;
                    else {
                        fila.Cantidad = -1;
                        fila.Mensaje += "El valor de Cantidad no es de tipo texto.\n";
                        fila.Estatus = false;
                    }
                    if(fila.Estatus){
                        if (ls.Count > 0) {
                            var filaObj = ls.Where(m => m.Nombre.ToUpper().Equals(fila.Nombre) && m.IdPresentacion.Equals(fila.IdPresentacion)).FirstOrDefault();
                            if (filaObj != null) {
                                fila.Mensaje += $"El medicamento ya se encuentra en la fila {filaObj.NumeroFila} del archivo Excel.\n";
                                fila.Estatus = false;
                            }
                        }
                    }
                        
                    ls.Add(fila);
                }
            }
            else {
                mensaje = "El archivo no contiene medicamentos.";
            }

            return ls;
        }
        private int ValidaPresentacion(Dictionary<int, string> dc, string presentacion) {
            int key = 0;
            string presentacionComp = presentacion.ToUpper()
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
            if(dc.Any(d => d.Value == presentacionComp))
                key = dc.Where(d => d.Value == presentacionComp).Select(s => s.Key).First();
            return key;
        }

    }
}
