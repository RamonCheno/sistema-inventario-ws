namespace SistemaInventarioWS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categorias",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Productoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                        Precio = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                        StockMinimo = c.Int(nullable: false),
                        CategoriaId = c.Int(nullable: false),
                        ProveedorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categorias", t => t.CategoriaId, cascadeDelete: true)
                .ForeignKey("dbo.Proveedors", t => t.ProveedorId, cascadeDelete: true)
                .Index(t => t.CategoriaId)
                .Index(t => t.ProveedorId);
            
            CreateTable(
                "dbo.Proveedors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                        Telefono = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                        Email = c.String(),
                        Telefono = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Ventas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Fecha = c.DateTime(nullable: false),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ClienteId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clientes", t => t.ClienteId, cascadeDelete: true)
                .Index(t => t.ClienteId);
            
            CreateTable(
                "dbo.DetalleVentas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cantidad = c.Int(nullable: false),
                        PrecioUnitario = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VentaId = c.Int(nullable: false),
                        ProductoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Productoes", t => t.ProductoId, cascadeDelete: true)
                .ForeignKey("dbo.Ventas", t => t.VentaId, cascadeDelete: true)
                .Index(t => t.VentaId)
                .Index(t => t.ProductoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleVentas", "VentaId", "dbo.Ventas");
            DropForeignKey("dbo.DetalleVentas", "ProductoId", "dbo.Productoes");
            DropForeignKey("dbo.Ventas", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.Productoes", "ProveedorId", "dbo.Proveedors");
            DropForeignKey("dbo.Productoes", "CategoriaId", "dbo.Categorias");
            DropIndex("dbo.DetalleVentas", new[] { "ProductoId" });
            DropIndex("dbo.DetalleVentas", new[] { "VentaId" });
            DropIndex("dbo.Ventas", new[] { "ClienteId" });
            DropIndex("dbo.Productoes", new[] { "ProveedorId" });
            DropIndex("dbo.Productoes", new[] { "CategoriaId" });
            DropTable("dbo.DetalleVentas");
            DropTable("dbo.Ventas");
            DropTable("dbo.Clientes");
            DropTable("dbo.Proveedors");
            DropTable("dbo.Productoes");
            DropTable("dbo.Categorias");
        }
    }
}
