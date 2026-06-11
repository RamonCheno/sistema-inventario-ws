using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SistemaInventarioWS.Models
{
    public class DetalleVenta
    {

        [Key]
        public int Id { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public int VentaId { get; set; }
        //[ForeignKey("Venta")]
        public Venta Venta {  get; set; }

        public int ProductoId { get; set; }
        //[ForeignKey("Producto")] 
        public Producto Producto { get; set; }


    }
}