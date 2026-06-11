using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SistemaInventarioWS.Models
{
    public class Producto
    {

        [Key]
        public string Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }
        public int StockMin { get; set; }

        public int CategoriaId { get; set; }
        //[ForeignKey("Categoria")]
        public Categoria Categoria { get; set; }

        public int ProveedorId { get; set; }
        //[ForeignKey("Proveedor")]
        public Proveedor Proveedor { get; set; }

    }
}