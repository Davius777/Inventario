using System;

namespace Inventario.DTO {
    public class MedicamentoDTO {
        public int IdMedicamento { get; set; }
        public string Nombre { get; set; }
        public int IdPresentacion { get; set; }
        public string Presentacion { get; set; }
        public int IdEmpresa { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string IdUsuarioCreacion { get; set; }
        public int Cantidad { get; set; }
    }
}