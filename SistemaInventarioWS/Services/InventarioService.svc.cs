using Newtonsoft.Json;
using SistemaInventarioWS.Contracts;
using SistemaInventarioWS.Data;
using SistemaInventarioWS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SistemaInventarioWS.Services
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "InventarioService" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione InventarioService.svc o InventarioService.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class InventarioService : IInventarioService
    {

        private readonly AppDbContext _db = new AppDbContext();
        
        //Categoria
        public string CreateCategoria(string json)
        {
            Categoria item = JsonConvert.DeserializeObject<Categoria>(json);
            _db.Categorias.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public bool DeleteCategoria(int id)
        {
            Categoria item = _db.Categorias.Find(id);
            if (item != null) return false;
            _db.Categorias.Remove(item);
            _db.SaveChanges();
            return true;
        }

        public string GetCategoria()
        {
            List<Categoria> lista = _db.Categorias.ToList();
            return JsonConvert.SerializeObject(lista);
        }

        public string GetCategoriaById(int id)
        {
            Categoria item = _db.Categorias.Find(id);
            return JsonConvert.SerializeObject(item);
        }

        public string UpdateCategoria(string json)
        {
            throw new NotImplementedException();
        }

        //Clientes
        public string CreateCliente(string json)
        {
            Cliente item = JsonConvert.DeserializeObject<Cliente>(json);
            _db.Clientes.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string DeleteCliente(int id)
        {
            throw new NotImplementedException();
        }

        public string GetClienteById(int id)
        {
            throw new NotImplementedException();
        }

        public string GetClientes()
        {
            throw new NotImplementedException();
        }

        public string UpdateCliente(string json)
        {
            throw new NotImplementedException();
        }

        //Productos
        public string CreateProducto(string json)
        {
            Producto item = JsonConvert.DeserializeObject<Producto>(json);
            _db.Productos.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string DeleteProducto(int id)
        {
            throw new NotImplementedException();
        }

        public string GetProductoById(int id)
        {
            throw new NotImplementedException();
        }

        public string GetProductos()
        {
            throw new NotImplementedException();
        }

        public string UpdateProducto(string json)
        {
            throw new NotImplementedException();
        }

        //Proveedor
        public string CreateProveedor(string json)
        {
            Proveedor item = JsonConvert.DeserializeObject<Proveedor>(json);
            _db.Proveedores.Add(item);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(item);
        }

        public string DeleteProveedor(int id)
        {
            throw new NotImplementedException();
        }

        public string GetProveedorById(int id)
        {
            throw new NotImplementedException();
        }

        public string GetProveedores()
        {
            throw new NotImplementedException();
        }

        public string UpdateProveedor(string json)
        {
            throw new NotImplementedException();
        }

        //Venta
        public string CreateVenta(string json)
        {
            Venta venta = JsonConvert.DeserializeObject<Venta>(json);
            venta.Total = 0;
            foreach (DetalleVenta detalle in venta.Detalles)
            {
                Producto producto = _db.Productos.Find(detalle.ProductoId);
                detalle.PrecioUnitario = producto.Precio;
                producto.Stock -= detalle.Cantidad;
                venta.Total += detalle.Cantidad * detalle.PrecioUnitario;
            }
            _db.Ventas.Add(venta);
            _db.SaveChanges();
            return JsonConvert.SerializeObject(venta, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
        }

        public string DeleteVenta(int id)
        {
            throw new NotImplementedException();
        }

        public string GetVentaById(int id)
        {
            throw new NotImplementedException();
        }

        public string GetVentas()
        {
            throw new NotImplementedException();
        }
    }
}
