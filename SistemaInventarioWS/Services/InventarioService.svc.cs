using Newtonsoft.Json;
using SistemaInventarioWS.Contracts;
using SistemaInventarioWS.Data;
using SistemaInventarioWS.Models;
using System.Linq;

namespace SistemaInventarioWS.Services
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "InventarioService" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione InventarioService.svc o InventarioService.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class InventarioService : IInventarioService
    {

        private readonly AppDbContext _db = new AppDbContext();

        public string GetCategorias()
        {
            var lista = _db.Categorias.ToList();
            return JsonConvert.SerializeObject(lista);
        }

        public string GetCategoriaById(int id)
        {
            var item = _db.Categorias.Find(id);
            return JsonConvert.SerializeObject(item);
        }

        public string CreateCategoria(string json)
        {
            var item = JsonConvert.DeserializeObject<Categoria>(json);
            _db.Categorias.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string UpdateCategoria(string json)
        {
            var item = JsonConvert.DeserializeObject<Categoria>(json);
            var existing = _db.Categorias.Find(item.Id);
            existing.Nombre = item.Nombre;
            _db.SaveChanges();
            return JsonConvert.SerializeObject(existing);
        }

        public bool DeleteCategoria(int id)
        {
            var item = _db.Categorias.Find(id);
            if (item == null) return false;
            _db.Categorias.Remove(item);
            _db.SaveChanges();
            return true;
        }

        // ── PROVEEDORES ─────────────────────────────────────────
        public string GetProveedores()
        {
            return JsonConvert.SerializeObject(_db.Proveedores.ToList());
        }

        public string GetProveedorById(int id)
        {
            return JsonConvert.SerializeObject(_db.Proveedores.Find(id));
        }

        public string CreateProveedor(string json)
        {
            var item = JsonConvert.DeserializeObject<Proveedor>(json);
            _db.Proveedores.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string UpdateProveedor(string json)
        {
            var item = JsonConvert.DeserializeObject<Proveedor>(json);
            var existing = _db.Proveedores.Find(item.Id);
            existing.Nombre = item.Nombre;
            existing.Telefono = item.Telefono;
            existing.Email = item.Email;
            _db.SaveChanges();
            return JsonConvert.SerializeObject(existing);
        }

        public bool DeleteProveedor(int id)
        {
            var item = _db.Proveedores.Find(id);
            if (item == null) return false;
            _db.Proveedores.Remove(item);
            _db.SaveChanges();
            return true;
        }

        // ── PRODUCTOS ───────────────────────────────────────────
        public string GetProductos()
        {
            var lista = _db.Productos
                .Include("Categoria")
                .Include("Proveedor")
                .ToList();
            return JsonConvert.SerializeObject(lista, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public string GetProductoById(int id)
        {
            var item = _db.Productos
                .Include("Categoria")
                .Include("Proveedor")
                .FirstOrDefault(p => p.Id == id);
            return JsonConvert.SerializeObject(item, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public string CreateProducto(string json)
        {
            var item = JsonConvert.DeserializeObject<Producto>(json);
            _db.Productos.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string UpdateProducto(string json)
        {
            var item = JsonConvert.DeserializeObject<Producto>(json);
            var existing = _db.Productos.Find(item.Id);
            existing.Nombre = item.Nombre;
            existing.Precio = item.Precio;
            existing.Stock = item.Stock;
            existing.StockMinimo = item.StockMinimo;
            existing.CategoriaId = item.CategoriaId;
            existing.ProveedorId = item.ProveedorId;
            _db.SaveChanges();
            return JsonConvert.SerializeObject(existing);
        }

        public bool DeleteProducto(int id)
        {
            var item = _db.Productos.Find(id);
            if (item == null) return false;
            _db.Productos.Remove(item);
            _db.SaveChanges();
            return true;
        }

        // ── CLIENTES ────────────────────────────────────────────
        public string GetClientes()
        {
            return JsonConvert.SerializeObject(_db.Clientes.ToList());
        }

        public string GetClienteById(int id)
        {
            return JsonConvert.SerializeObject(_db.Clientes.Find(id));
        }

        public string CreateCliente(string json)
        {
            var item = JsonConvert.DeserializeObject<Cliente>(json);
            _db.Clientes.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string UpdateCliente(string json)
        {
            var item = JsonConvert.DeserializeObject<Cliente>(json);
            var existing = _db.Clientes.Find(item.Id);
            existing.Nombre = item.Nombre;
            existing.Email = item.Email;
            existing.Telefono = item.Telefono;
            _db.SaveChanges();
            return JsonConvert.SerializeObject(existing);
        }

        public bool DeleteCliente(int id)
        {
            var item = _db.Clientes.Find(id);
            if (item == null) return false;
            _db.Clientes.Remove(item);
            _db.SaveChanges();
            return true;
        }

        // ── VENTAS ──────────────────────────────────────────────
        public string GetVentas()
        {
            var lista = _db.Ventas
                .Include("Cliente")
                .Include("Detalles")
                .Include("Detalles.Producto")
                .ToList();
            return JsonConvert.SerializeObject(lista, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public string GetVentaById(int id)
        {
            var item = _db.Ventas
                .Include("Cliente")
                .Include("Detalles")
                .Include("Detalles.Producto")
                .FirstOrDefault(v => v.Id == id);
            return JsonConvert.SerializeObject(item, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public string CreateVenta(string json)
        {
            var venta = JsonConvert.DeserializeObject<Venta>(json);
            venta.Total = 0;
            foreach (var detalle in venta.Detalles)
            {
                var producto = _db.Productos.Find(detalle.ProductoId);
                detalle.PrecioUnitario = producto.Precio;
                producto.Stock -= detalle.Cantidad;
                venta.Total += detalle.Cantidad * detalle.PrecioUnitario;
            }
            _db.Ventas.Add(venta);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(venta, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public bool DeleteVenta(int id)
        {
            var item = _db.Ventas.Find(id);
            if (item == null) return false;
            _db.Ventas.Remove(item);
            _db.SaveChanges();
            return true;
        }
    }
}
