# Sistema de Inventario WS

Backend del Sistema de Inventario y Ventas — servicio SOAP para gestionar productos, proveedores, clientes y ventas con control de stock automático.

Monorepo de carpetas con dos implementaciones del mismo backend:

```
sistema-inventario-ws/
├── wcf-clasico/       ← WCF SOAP clásico sobre .NET Framework 4.8 (en uso)
└── corewcf-net10/     ← Migración a CoreWCF sobre .NET 10 (aprendizaje, en progreso)
```

| Carpeta | Descripción |
|---|---|
| [`wcf-clasico/`](wcf-clasico) | Implementación de referencia, la que consume el frontend en producción |
| [`corewcf-net10/`](corewcf-net10) | Ejercicio de aprendizaje — WCF SOAP en .NET moderno, no reemplaza a `wcf-clasico/` |
