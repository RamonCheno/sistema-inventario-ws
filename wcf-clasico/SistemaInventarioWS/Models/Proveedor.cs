using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaInventarioWS.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Telefono { get; set; }

        public string Email {  get; set; }

        public ICollection<Producto> Productos { get; set; }

    }
}