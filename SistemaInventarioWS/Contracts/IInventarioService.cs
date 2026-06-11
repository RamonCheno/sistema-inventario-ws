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

        //Categoria
        [OperationContract] string GetCategoria();
        [OperationContract] string GetCategoriaById(int id);
        [OperationContract] string CreateCategoria(string json);
        [OperationContract] string UpdateCategoria(string json);
        [OperationContract] bool DeleteCategoria(int id);

        //Proveedores
        [OperationContract] string GetProveedores();
        [OperationContract] string GetProveedorById(int id);
        [OperationContract] string CreateProveedor(string json);
        [OperationContract] string UpdateProveedor(string json);
        [OperationContract] bool DeleteProveedor(int id);

        //Productos
        [OperationContract] string GetProductos();
        [OperationContract] string GetProductoById(int id);
        [OperationContract] string CreateProducto(string json);
        [OperationContract] string UpdateProducto(string json);
        [OperationContract] bool DeleteProducto(int id);

        //Clientes
        [OperationContract] string GetClientes();
        [OperationContract] string GetClienteById(int id);
        [OperationContract] string CreateCliente(string json);
        [OperationContract] string UpdateCliente(string json);
        [OperationContract] bool DeleteCliente(int id);

        //Ventas
        [OperationContract] string GetVentas();
        [OperationContract] string GetVentaById(int id);
        [OperationContract] string CreateVenta(string json);
        [OperationContract] bool DeleteVenta(int id);

    }
}
