//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Inventario.Models.ModelBD
{
    using System;
    using System.Collections.Generic;
    
    public partial class Existencia
    {
        public int IdMedicamento { get; set; }
        public int IdPresentacion { get; set; }
        public Nullable<int> Entradas { get; set; }
        public Nullable<int> Salidas { get; set; }
        public Nullable<int> SaldoActual { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public int IdUsuarioCreacion { get; set; }
        public int IdEmpresa { get; set; }
    
        public virtual Empresa Empresa { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual CatPresentacion CatPresentacion { get; set; }
        public virtual Medicamento Medicamento { get; set; }
    }
}
