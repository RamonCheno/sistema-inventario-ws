# Sistema de Inventario WS — CoreWCF / .NET 10

Migración de aprendizaje del backend WCF SOAP clásico (ver [`../wcf-clasico`](../wcf-clasico)) a **CoreWCF** sobre **.NET 10**.

> Este proyecto **no reemplaza** a `wcf-clasico/` — es un ejercicio para aprender cómo se implementa WCF SOAP en .NET moderno. `wcf-clasico/` sigue siendo la versión en uso real por el frontend.

## Objetivo

Aprender el modelo de hosting de WCF sobre ASP.NET Core (`Program.cs`, sin `Global.asax`/`Web.config`), usando el mismo contrato (`IInventarioService`) y las mismas entidades que el proyecto clásico, como referencia comparativa directa.

## Stack

- C# / .NET 10
- CoreWCF (Primitives, Http, WebHttp)
- Entity Framework Core
- SQL Server LocalDB

## Estado

🚧 En progreso — proyecto aún no creado.

## Etapas del plan

1. Crear proyecto base .NET 10 (SDK-style, `Program.cs`)
2. Instalar paquetes CoreWCF
3. Portar el contrato (`IInventarioService.cs`)
4. Configurar el servicio en `Program.cs`
5. Migrar `AppDbContext` y modelos de EF6 a EF Core
6. Configurar CORS nativo (`UseCors`)
7. Configurar endpoint de metadata (mex/WSDL)
8. Probar contra el mismo frontend (`sistema-inventario-ui`)
9. [Opcional] Evaluar si se recrea la HelpPage (sin equivalente directo en CoreWCF)

Ver también: `ROADMAP.md` — Fase 2.5 (CoreWCF, opcional).
