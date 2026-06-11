using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioWS.Contracts
{
    [ServiceContract]
    public interface IInventarioService
    {

        // Categorías
        /// <summary>Retorna todas las categorías</summary>
        [OperationContract] string GetCategorias();
        /// <summary>Retorna una categoría por Id</summary>
        [OperationContract] string GetCategoriaById(int id);
        /// <summary>Crea una nueva categoría</summary>
        [OperationContract] string CreateCategoria(string json);
        /// <summary>Actualiza el nombre de una categoría existente</summary>
        [OperationContract] string UpdateCategoria(string json);
        /// <summary>Elimina una categoría por Id</summary>
        [OperationContract] bool DeleteCategoria(int id);

        // Proveedores
        /// <summary>Retorna todos los proveedores</summary>
        [OperationContract] string GetProveedores();
        /// <summary>Retorna un proveedor por Id</summary>
        [OperationContract] string GetProveedorById(int id);
        /// <summary>Crea un nuevo proveedor</summary>
        [OperationContract] string CreateProveedor(string json);
        /// <summary>Actualiza los datos de un proveedor existente</summary>
        [OperationContract] string UpdateProveedor(string json);
        /// <summary>Elimina un proveedor por Id</summary>
        [OperationContract] bool DeleteProveedor(int id);

        // Productos
        /// <summary>Retorna todos los productos con su categoría y proveedor</summary>
        [OperationContract] string GetProductos();
        /// <summary>Retorna un producto por Id con su categoría y proveedor</summary>
        [OperationContract] string GetProductoById(int id);
        /// <summary>Crea un nuevo producto</summary>
        [OperationContract] string CreateProducto(string json);
        /// <summary>Actualiza los datos de un producto existente</summary>
        [OperationContract] string UpdateProducto(string json);
        /// <summary>Elimina un producto por Id</summary>
        [OperationContract] bool DeleteProducto(int id);

        // Clientes
        /// <summary>Retorna todos los clientes</summary>
        [OperationContract] string GetClientes();
        /// <summary>Retorna un cliente por Id</summary>
        [OperationContract] string GetClienteById(int id);
        /// <summary>Crea un nuevo cliente</summary>
        [OperationContract] string CreateCliente(string json);
        /// <summary>Actualiza los datos de un cliente existente</summary>
        [OperationContract] string UpdateCliente(string json);
        /// <summary>Elimina un cliente por Id</summary>
        [OperationContract] bool DeleteCliente(int id);

        // Ventas
        /// <summary>Retorna todas las ventas con cliente y detalle de productos</summary>
        [OperationContract] string GetVentas();
        /// <summary>Retorna una venta por Id con cliente y detalle</summary>
        [OperationContract] string GetVentaById(int id);
        /// <summary>Crea una venta — calcula Total y PrecioUnitario, descuenta stock</summary>
        [OperationContract] string CreateVenta(string json);
        /// <summary>Elimina una venta por Id</summary>
        [OperationContract] bool DeleteVenta(int id);

    }
}
