using SistemaInventarioWS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SistemaInventarioWS.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext() : base("name=SIstemaInventarioDB") { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

    }
}