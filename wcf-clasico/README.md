# Sistema de Inventario WS — WCF clásico

Backend del Sistema de Inventario y Ventas. Servicio SOAP construido con WCF (.svc) sobre .NET Framework 4.8 y Entity Framework 6.

Es la implementación de referencia y la que consume el frontend en producción. Ver [`../corewcf-net10`](../corewcf-net10) para la migración de aprendizaje a .NET moderno.

## Stack

- C# / .NET Framework 4.8
- WCF SOAP (.svc)
- Entity Framework 6
- SQL Server LocalDB

## Entidades

| Entidad | Descripción |
|---|---|
| Categoria | Clasificación de productos |
| Proveedor | Proveedor de productos |
| Producto | Artículo con stock y precio |
| Cliente | Cliente del sistema |
| Venta | Cabecera de venta con total calculado |
| DetalleVenta | Líneas de cada venta |

## Requisitos

- Visual Studio 2022
- .NET Framework 4.8
- SQL Server LocalDB (incluido con VS 2022)

## Configuración

1. Abrir `SistemaInventarioWS.sln` en Visual Studio 2022
2. Restaurar paquetes NuGet (clic derecho en la solución → Restore NuGet Packages)
3. En el Package Manager Console, aplicar la migración:
   ```
   Update-Database
   ```
4. Ejecutar el proyecto (F5)

El servicio queda disponible en:
```
http://localhost:{puerto}/Services/InventarioService.svc
```

## Estructura

```
SistemaInventarioWS/
├── Contracts/
│   └── IInventarioService.cs    ← contratos SOAP (OperationContract)
├── Data/
│   └── AppDbContext.cs          ← DbContext de Entity Framework
├── Migrations/                  ← migraciones EF6
├── Models/                      ← entidades del dominio
│   ├── Categoria.cs
│   ├── Proveedor.cs
│   ├── Producto.cs
│   ├── Cliente.cs
│   ├── Venta.cs
│   └── DetalleVenta.cs
├── Services/
│   └── InventarioService.svc.cs ← implementación del servicio
├── Global.asax.cs               ← manejo de CORS (preflight OPTIONS)
└── Web.config                   ← binding, CORS headers, connection string
```

## Notas

- Las respuestas usan JSON serializado como string dentro del envelope SOAP
- CORS habilitado para consumo desde React (localhost)
- `DetalleVenta` se gestiona a través de las operaciones de `Venta` (no tiene CRUD propio)
- Al crear una venta, el stock de cada producto se descuenta automáticamente
