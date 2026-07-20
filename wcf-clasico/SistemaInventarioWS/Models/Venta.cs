using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SistemaInventarioWS.Models
{
    public class Venta
    {

        [Key]
        public int Id {  get; set; }

        public DateTime Fecha {get; set; }
        public decimal Total { get; set; }

        public int ClienteId { get; set; }
        //[ForeignKey("Cliente")]
        public Cliente Cliente { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; }  

    }
}