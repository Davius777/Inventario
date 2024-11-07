using Inventario.DTO;
using Inventario.Models;
using Inventario.Models.ModelBD;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Inventario {
    public class InventarioLogic {
        public static List<MedicamentoDTO> ObtenMedicamentos() {
            List<MedicamentoDTO> ls = new List<MedicamentoDTO>();
            using (LocalDataEntities db = new LocalDataEntities()) {
                ls = db.Medicamento.Select(
                    med => new MedicamentoDTO {
                        Nombre = med.Nombre
                    }).Distinct().ToList();
            };
            return ls;
        }
        public static List<CatPresentacionDTO> ObtenPresentaciones() {
            List<CatPresentacionDTO> ls = new List<CatPresentacionDTO>();
            using (LocalDataEntities db = new LocalDataEntities()) {
                ls = db.CatPresentacion.Where(c => (bool)c.Visible).Select(
                    cat => new CatPresentacionDTO {
                        Id = cat.IdPresentacion,
                        Nombre = cat.Nombre
                    }).ToList();
            };
            return ls;
        }
        public static Dictionary<int, string> ObtenDiccionarioPresentaciones() {
            Dictionary<int, string> dc = new Dictionary<int, string>();
            List<CatPresentacionDTO> ls = ObtenPresentaciones();
            foreach (CatPresentacionDTO cat in ls) { 
                string nombreCat = cat.Nombre.ToUpper().Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U"); ;
                dc.Add(cat.Id, nombreCat);
            }
            return dc; 
        }
        public static string Guardar(MedicamentoDTO med, int usuarioId, int idEmpresa) {
            string res = string.Empty;
            using (LocalDataEntities db = new LocalDataEntities()) {
                db.EntradaSalida(true, idEmpresa, usuarioId, med.Nombre, med.IdPresentacion, med.Observaciones, med.Cantidad);
            };
            return res;
        }
        public static int ValidarExistencia(MedicamentoDTO med, int idEmpresa) {
            int existenciaActual = 0;
            using (LocalDataEntities db = new LocalDataEntities()) {
                var medicamentoObj = db.Medicamento.
                    Where(a => ((int)a.IdEmpresa).Equals(idEmpresa) && a.Nombre.Equals(med.Nombre) && a.IdPresentacion.Equals(med.IdPresentacion)).FirstOrDefault();
                if (medicamentoObj != null) {
                    var existencia = db.Existencia.
                        Where(a => a.IdMedicamento.Equals((int)medicamentoObj.IdMedicamento)).FirstOrDefault();
                    existenciaActual = (int)existencia.SaldoActual;
                }
                else {
                    existenciaActual = -1;
                }
                return existenciaActual;
            };
        }
        public static string Obtener(MedicamentoDTO med, int usuarioId, int idEmpresa) {
            string res = string.Empty;
            using (LocalDataEntities db = new LocalDataEntities()) {
                db.EntradaSalida(false, idEmpresa, usuarioId, med.Nombre, med.IdPresentacion, med.Observaciones, med.Cantidad);
            };
            return res;
        }
        public static List<MedicamentoDTO> Listar(int idEmpresa) {
            List<MedicamentoDTO> ls = new List<MedicamentoDTO> ();
            using (LocalDataEntities db = new LocalDataEntities()) {
                ls = (from med in db.Medicamento
                    join ext in db.Existencia on
                        new { IdMedicamento = med.IdMedicamento, IdPresentacion = med.IdPresentacion, IdEmpresa = (int)med.IdEmpresa } 
                            equals new { IdMedicamento = ext.IdMedicamento, IdPresentacion = ext.IdPresentacion, IdEmpresa = ext.IdEmpresa }
                    join cat in db.CatPresentacion on
                        new { IdPresentacion = med.IdPresentacion }
                            equals new { IdPresentacion = cat.IdPresentacion}
                    select new { Nombre = med.Nombre, Presentacion = cat.Nombre, ext.SaldoActual}
                     ).ToList().Select(
                    med => new MedicamentoDTO {
                        Nombre = med.Nombre,
                        Presentacion = med.Presentacion,
                        Cantidad = (int)med.SaldoActual
                    }).ToList();
            };
            return ls;
        }
    }
}