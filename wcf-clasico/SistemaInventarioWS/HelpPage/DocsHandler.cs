using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace SistemaInventarioWS
{
    public class DocsHandler : IHttpHandler
    {
        private const string InterfacePrefix = "M:SistemaInventarioWS.Contracts.IInventarioService.";

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Write(BuildPage(context));
        }

        // ── Página principal ──────────────────────────────────────────────────

        private string BuildPage(HttpContext ctx)
        {
            var ops = LoadOperations(ctx);
            var sb = new StringBuilder();
            sb.Append(HtmlHead());
            sb.Append("<body>");
            sb.Append(HtmlHeader());
            sb.Append(HtmlMetaBar());
            sb.Append("<main>");
            sb.Append(OperationsSection(ops));
            sb.Append(ModelsSection());
            sb.Append(SoapUiSection());
            sb.Append("</main>");
            sb.Append(HtmlFooter());
            sb.Append(ScriptBlock());
            sb.Append("</body></html>");
            return sb.ToString();
        }

        // ── Carga de operaciones desde XML de documentación ───────────────────

        private List<OpDoc> LoadOperations(HttpContext ctx)
        {
            var result = new List<OpDoc>();
            string xmlPath = ctx.Server.MapPath("~/App_Data/SistemaInventarioWS.xml");
            if (!File.Exists(xmlPath)) return result;
            try
            {
                var doc = XDocument.Load(xmlPath);
                foreach (var member in doc.Descendants("member"))
                {
                    var fullName = (string)member.Attribute("name") ?? string.Empty;
                    if (!fullName.StartsWith(InterfacePrefix)) continue;
                    var segment = fullName.Substring(InterfacePrefix.Length);
                    var paren   = segment.IndexOf('(');
                    var name    = paren >= 0 ? segment.Substring(0, paren) : segment;
                    result.Add(new OpDoc
                    {
                        Name    = name,
                        Summary = Normalize(member.Element("summary"))
                    });
                }
            }
            catch { }
            return result;
        }

        private string Normalize(XElement el)
        {
            if (el == null) return string.Empty;
            var lines = el.Value
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.None)
                .Select(l => l.Trim())
                .SkipWhile(string.IsNullOrEmpty)
                .ToList();
            while (lines.Count > 0 && string.IsNullOrEmpty(lines[lines.Count - 1]))
                lines.RemoveAt(lines.Count - 1);
            return string.Join(" ", lines.Where(l => !string.IsNullOrEmpty(l)));
        }

        // ── Secciones ─────────────────────────────────────────────────────────

        private string OperationsSection(List<OpDoc> ops)
        {
            if (!ops.Any())
                return @"<div class=""warn"">
  <strong>Archivo de documentaci&oacute;n no encontrado.</strong><br/>
  En Visual Studio: clic derecho en el proyecto &rarr; <strong>Propiedades &rarr; Compilar</strong>
  &rarr; activar <em>Archivo de documentaci&oacute;n XML</em> con ruta
  <code>App_Data\SistemaInventarioWS.xml</code>. Luego recompile en modo Debug.
</div>";

            var sb = new StringBuilder();
            string currentGroup = null;
            for (int i = 0; i < ops.Count; i++)
            {
                string group = GetEntityGroup(ops[i].Name);
                if (group != currentGroup)
                {
                    currentGroup = group;
                    if (!string.IsNullOrEmpty(group))
                        sb.Append($"<h2>{group}</h2>");
                }
                sb.Append(OperationCard(ops[i], i));
            }
            return sb.ToString();
        }

        private static string GetEntityGroup(string op)
        {
            if (op.Contains("Categoria")) return "Categor&iacute;as";
            if (op.Contains("Proveedor")) return "Proveedores";
            if (op.Contains("Producto"))  return "Productos";
            if (op.Contains("Cliente"))   return "Clientes";
            if (op.Contains("Venta"))     return "Ventas";
            return string.Empty;
        }

        private string OperationCard(OpDoc op, int idx)
        {
            var content = GetOperationContent(op.Name);
            if (content == null) return string.Empty;

            string tabId = "op" + idx;
            string desc  = string.IsNullOrEmpty(op.Summary) ? op.Name : op.Summary;

            return $@"
<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    {GetBadge(op.Name)}
    <span class=""op-name"">{H(op.Name)}</span>
    <span class=""op-desc"">{H(desc)}</span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <div class=""tabs"" id=""tabs-{tabId}"">
      <div class=""tab active"" onclick=""switchTab('{tabId}','json')"">JSON</div>
      <div class=""tab""        onclick=""switchTab('{tabId}','xml')"">SOAP XML</div>
    </div>
    <div class=""tab-panel active"" id=""{tabId}-json"">
      <div class=""section-label"">Entrada</div>
      <pre>{content.JsonIn}</pre>
      <div class=""section-label"">Respuesta</div>
      <pre>{content.JsonOut}</pre>
    </div>
    <div class=""tab-panel"" id=""{tabId}-xml"">
      <div class=""section-label"">Request SOAP</div>
      <pre>{content.SoapReq}</pre>
      <div class=""section-label"">Response SOAP</div>
      <pre>{content.SoapRes}</pre>
    </div>
  </div>
</div>";
        }

        private static string GetBadge(string op)
        {
            if (op.StartsWith("Get"))    return "<span class=\"badge badge-get\">GET</span>";
            if (op.StartsWith("Create")) return "<span class=\"badge badge-post\">POST</span>";
            if (op.StartsWith("Update")) return "<span class=\"badge badge-put\">PUT</span>";
            if (op.StartsWith("Delete")) return "<span class=\"badge badge-del\">DELETE</span>";
            return "<span class=\"badge badge-soap\">SOAP</span>";
        }

        // ── Contenido por operación ───────────────────────────────────────────

        private OpContent GetOperationContent(string name)
        {
            switch (name)
            {
                case "GetCategorias":    return ContentGetCategorias();
                case "GetCategoriaById": return ContentGetById("GetCategoriaById",  CatObj());
                case "CreateCategoria":  return ContentCreateCategoria();
                case "UpdateCategoria":  return ContentUpdateCategoria();
                case "DeleteCategoria":  return ContentDelete("DeleteCategoria");

                case "GetProveedores":    return ContentGetProveedores();
                case "GetProveedorById":  return ContentGetById("GetProveedorById",  ProvObj());
                case "CreateProveedor":   return ContentCreateProveedor();
                case "UpdateProveedor":   return ContentUpdateProveedor();
                case "DeleteProveedor":   return ContentDelete("DeleteProveedor");

                case "GetProductos":    return ContentGetProductos();
                case "GetProductoById": return ContentGetById("GetProductoById",   ProdObj(true));
                case "CreateProducto":  return ContentCreateProducto();
                case "UpdateProducto":  return ContentUpdateProducto();
                case "DeleteProducto":  return ContentDelete("DeleteProducto");

                case "GetClientes":    return ContentGetClientes();
                case "GetClienteById": return ContentGetById("GetClienteById",    CliObj());
                case "CreateCliente":  return ContentCreateCliente();
                case "UpdateCliente":  return ContentUpdateCliente();
                case "DeleteCliente":  return ContentDelete("DeleteCliente");

                case "GetVentas":    return ContentGetVentas();
                case "GetVentaById": return ContentGetById("GetVentaById",   VentaObj());
                case "CreateVenta":  return ContentCreateVenta();
                case "DeleteVenta":  return ContentDelete("DeleteVenta");

                default: return null;
            }
        }

        // ── Objetos JSON de ejemplo ───────────────────────────────────────────

        private static string CatObj() =>
            "{\n  " + Jk("\"Id\"")     + ": " + Jn("1") + ",\n  " +
                      Jk("\"Nombre\"") + ": " + Js("\"Electrónicos\"") + "\n}";

        private static string ProvObj() =>
            "{\n  " + Jk("\"Id\"")       + ": " + Jn("1") + ",\n  " +
                      Jk("\"Nombre\"")   + ": " + Js("\"TechSupplier\"") + ",\n  " +
                      Jk("\"Telefono\"") + ": " + Js("\"664-123-4567\"") + ",\n  " +
                      Jk("\"Email\"")    + ": " + Js("\"contacto@tech.com\"") + "\n}";

        private static string ProdObj(bool withRelations)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  " + Jk("\"Id\"")          + ": " + Jn("1") + ",\n");
            sb.Append("  " + Jk("\"Nombre\"")      + ": " + Js("\"Laptop\"") + ",\n");
            sb.Append("  " + Jk("\"Precio\"")      + ": " + Jn("15000.00") + ",\n");
            sb.Append("  " + Jk("\"Stock\"")       + ": " + Jn("10") + ",\n");
            sb.Append("  " + Jk("\"StockMinimo\"") + ": " + Jn("2") + ",\n");
            sb.Append("  " + Jk("\"CategoriaId\"") + ": " + Jn("1") + ",\n");
            if (withRelations)
                sb.Append("  " + Jk("\"Categoria\"")  + ": { " + Jk("\"Id\"") + ": " + Jn("1") + ", " + Jk("\"Nombre\"") + ": " + Js("\"Electr&#243;nicos\"") + " },\n");
            sb.Append("  " + Jk("\"ProveedorId\"") + ": " + Jn("1"));
            if (withRelations)
                sb.Append(",\n  " + Jk("\"Proveedor\"")  + ": { " + Jk("\"Id\"") + ": " + Jn("1") + ", " + Jk("\"Nombre\"") + ": " + Js("\"TechSupplier\"") + " }");
            sb.Append("\n}");
            return sb.ToString();
        }

        private static string CliObj() =>
            "{\n  " + Jk("\"Id\"")       + ": " + Jn("1") + ",\n  " +
                      Jk("\"Nombre\"")   + ": " + Js("\"Juan Pérez\"") + ",\n  " +
                      Jk("\"Email\"")    + ": " + Js("\"juan@email.com\"") + ",\n  " +
                      Jk("\"Telefono\"") + ": " + Js("\"664-987-6543\"") + "\n}";

        private static string VentaObj() =>
            "{\n  " + Jk("\"Id\"")        + ": " + Jn("1") + ",\n  " +
                      Jk("\"Fecha\"")     + ": " + Js("\"2026-06-10T00:00:00\"") + ",\n  " +
                      Jk("\"Total\"")     + ": " + Jn("30000.00") + ",\n  " +
                      Jk("\"ClienteId\"") + ": " + Jn("1") + ",\n  " +
                      Jk("\"Cliente\"")   + ": { " + Jk("\"Id\"") + ": " + Jn("1") + ", " + Jk("\"Nombre\"") + ": " + Js("\"Juan P&#233;rez\"") + " },\n  " +
                      Jk("\"Detalles\"")  + ": [\n    { " +
                          Jk("\"ProductoId\"")     + ": " + Jn("1") + ", " +
                          Jk("\"Cantidad\"")       + ": " + Jn("2") + ", " +
                          Jk("\"PrecioUnitario\"") + ": " + Jn("15000.00") + " }\n  ]\n}";

        // ── Métodos de contenido ──────────────────────────────────────────────

        private OpContent ContentGetCategorias() => new OpContent
        {
            JsonIn  = NoParams,
            JsonOut = "[\n  " + CatObj().Replace("\n", "\n  ").TrimEnd() + "\n]",
            SoapReq = Env(ReqNone("GetCategorias")),
            SoapRes = Res("GetCategorias", "[{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Electr&#243;nicos&quot;}]")
        };

        private OpContent ContentGetById(string opName, string obj) => new OpContent
        {
            JsonIn  = IdParam,
            JsonOut = obj,
            SoapReq = Env(ReqId(opName)),
            SoapRes = Res(opName, "{...}")
        };

        private OpContent ContentCreateCategoria() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Nombre\"") + ": " + Js("\"Electrónicos\"") + "  " + Cmt("// string — requerido") + "\n}",
            JsonOut = CatObj(),
            SoapReq = Env(ReqJson("CreateCategoria", "{&quot;Nombre&quot;:&quot;Electr&#243;nicos&quot;}")),
            SoapRes = Res("CreateCategoria", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Electr&#243;nicos&quot;}")
        };

        private OpContent ContentUpdateCategoria() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Id\"")     + ":     " + Jn("1") + ",  " + Cmt("// int    — requerido") + "\n  " +
                                Jk("\"Nombre\"") + ": " + Js("\"Tecnología\"") + "  " + Cmt("// string — requerido") + "\n}",
            JsonOut = "{\n  " + Jk("\"Id\"") + ": " + Jn("1") + ",\n  " + Jk("\"Nombre\"") + ": " + Js("\"Tecnología\"") + "\n}",
            SoapReq = Env(ReqJson("UpdateCategoria", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Tecnolog&#237;a&quot;}")),
            SoapRes = Res("UpdateCategoria", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Tecnolog&#237;a&quot;}")
        };

        private OpContent ContentGetProveedores() => new OpContent
        {
            JsonIn  = NoParams,
            JsonOut = "[\n  " + ProvObj().Replace("\n", "\n  ").TrimEnd() + "\n]",
            SoapReq = Env(ReqNone("GetProveedores")),
            SoapRes = Res("GetProveedores", "[{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;TechSupplier&quot;,...}]")
        };

        private OpContent ContentCreateProveedor() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Nombre\"")   + ":   " + Js("\"TechSupplier\"") + ",  " + Cmt("// string — requerido") + "\n  " +
                                Jk("\"Telefono\"") + ": " + Js("\"664-123-4567\"") + ",  " + Cmt("// string") + "\n  " +
                                Jk("\"Email\"")    + ":    " + Js("\"contacto@tech.com\"") + "  " + Cmt("// string") + "\n}",
            JsonOut = ProvObj(),
            SoapReq = Env(ReqJson("CreateProveedor", "{&quot;Nombre&quot;:&quot;TechSupplier&quot;,&quot;Telefono&quot;:&quot;664-123-4567&quot;,&quot;Email&quot;:&quot;contacto@tech.com&quot;}")),
            SoapRes = Res("CreateProveedor", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;TechSupplier&quot;,...}")
        };

        private OpContent ContentUpdateProveedor() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Id\"")       + ":       " + Jn("1") + ",  " + Cmt("// int — requerido") + "\n  " +
                                Jk("\"Nombre\"")   + ":   " + Js("\"TechSupplier MX\"") + ",\n  " +
                                Jk("\"Telefono\"") + ": " + Js("\"664-999-8888\"") + ",\n  " +
                                Jk("\"Email\"")    + ":    " + Js("\"nuevo@tech.com\"") + "\n}",
            JsonOut = "{\n  " + Jk("\"Id\"") + ": " + Jn("1") + ",\n  " + Jk("\"Nombre\"") + ": " + Js("\"TechSupplier MX\"") + ",\n  " +
                                Jk("\"Telefono\"") + ": " + Js("\"664-999-8888\"") + ",\n  " + Jk("\"Email\"") + ": " + Js("\"nuevo@tech.com\"") + "\n}",
            SoapReq = Env(ReqJson("UpdateProveedor", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;TechSupplier MX&quot;,...}")),
            SoapRes = Res("UpdateProveedor", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;TechSupplier MX&quot;,...}")
        };

        private OpContent ContentGetProductos() => new OpContent
        {
            JsonIn  = NoParams,
            JsonOut = "[\n  " + ProdObj(true).Replace("\n", "\n  ").TrimEnd() + "\n]",
            SoapReq = Env(ReqNone("GetProductos")),
            SoapRes = Res("GetProductos", "[{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Laptop&quot;,&quot;Categoria&quot;:{...},...}]")
        };

        private OpContent ContentCreateProducto() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Nombre\"")      + ":      " + Js("\"Laptop\"") + ",       " + Cmt("// string — requerido") + "\n  " +
                                Jk("\"Precio\"")      + ":      " + Jn("15000.00") + ",    " + Cmt("// decimal") + "\n  " +
                                Jk("\"Stock\"")       + ":       " + Jn("10") + ",          " + Cmt("// int") + "\n  " +
                                Jk("\"StockMinimo\"") + ": " + Jn("2") + ",           " + Cmt("// int") + "\n  " +
                                Jk("\"CategoriaId\"") + ": " + Jn("1") + ",           " + Cmt("// int — FK Categoria") + "\n  " +
                                Jk("\"ProveedorId\"") + ": " + Jn("1") + "            " + Cmt("// int — FK Proveedor") + "\n}",
            JsonOut = ProdObj(false),
            SoapReq = Env(ReqJson("CreateProducto", "{&quot;Nombre&quot;:&quot;Laptop&quot;,&quot;Precio&quot;:15000.00,&quot;Stock&quot;:10,&quot;StockMinimo&quot;:2,&quot;CategoriaId&quot;:1,&quot;ProveedorId&quot;:1}")),
            SoapRes = Res("CreateProducto", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Laptop&quot;,...}")
        };

        private OpContent ContentUpdateProducto() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Id\"")          + ":          " + Jn("1") + ",  " + Cmt("// int — requerido") + "\n  " +
                                Jk("\"Nombre\"")      + ":      " + Js("\"Laptop Pro\"") + ",\n  " +
                                Jk("\"Precio\"")      + ":      " + Jn("18000.00") + ",\n  " +
                                Jk("\"Stock\"")       + ":       " + Jn("8") + ",\n  " +
                                Jk("\"StockMinimo\"") + ": " + Jn("2") + ",\n  " +
                                Jk("\"CategoriaId\"") + ": " + Jn("1") + ",\n  " +
                                Jk("\"ProveedorId\"") + ": " + Jn("1") + "\n}",
            JsonOut = "{\n  " + Jk("\"Id\"") + ": " + Jn("1") + ",\n  " + Jk("\"Nombre\"") + ": " + Js("\"Laptop Pro\"") + ",\n  " +
                                Jk("\"Precio\"") + ": " + Jn("18000.00") + ",\n  ...\n}",
            SoapReq = Env(ReqJson("UpdateProducto", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Laptop Pro&quot;,&quot;Precio&quot;:18000.00,...}")),
            SoapRes = Res("UpdateProducto", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Laptop Pro&quot;,...}")
        };

        private OpContent ContentGetClientes() => new OpContent
        {
            JsonIn  = NoParams,
            JsonOut = "[\n  " + CliObj().Replace("\n", "\n  ").TrimEnd() + "\n]",
            SoapReq = Env(ReqNone("GetClientes")),
            SoapRes = Res("GetClientes", "[{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Juan P&#233;rez&quot;,...}]")
        };

        private OpContent ContentCreateCliente() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Nombre\"")   + ":   " + Js("\"Juan Pérez\"") + ",  " + Cmt("// string — requerido") + "\n  " +
                                Jk("\"Email\"")    + ":    " + Js("\"juan@email.com\"") + ",  " + Cmt("// string") + "\n  " +
                                Jk("\"Telefono\"") + ": " + Js("\"664-987-6543\"") + "  " + Cmt("// string") + "\n}",
            JsonOut = CliObj(),
            SoapReq = Env(ReqJson("CreateCliente", "{&quot;Nombre&quot;:&quot;Juan P&#233;rez&quot;,&quot;Email&quot;:&quot;juan@email.com&quot;,&quot;Telefono&quot;:&quot;664-987-6543&quot;}")),
            SoapRes = Res("CreateCliente", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Juan P&#233;rez&quot;,...}")
        };

        private OpContent ContentUpdateCliente() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"Id\"")       + ":       " + Jn("1") + ",  " + Cmt("// int — requerido") + "\n  " +
                                Jk("\"Nombre\"")   + ":   " + Js("\"Juan Pérez\"") + ",\n  " +
                                Jk("\"Email\"")    + ":    " + Js("\"nuevo@email.com\"") + ",\n  " +
                                Jk("\"Telefono\"") + ": " + Js("\"664-111-2222\"") + "\n}",
            JsonOut = "{\n  " + Jk("\"Id\"") + ": " + Jn("1") + ",\n  " + Jk("\"Nombre\"") + ": " + Js("\"Juan Pérez\"") + ",\n  " +
                                Jk("\"Email\"") + ": " + Js("\"nuevo@email.com\"") + ",\n  " + Jk("\"Telefono\"") + ": " + Js("\"664-111-2222\"") + "\n}",
            SoapReq = Env(ReqJson("UpdateCliente", "{&quot;Id&quot;:1,&quot;Nombre&quot;:&quot;Juan P&#233;rez&quot;,...}")),
            SoapRes = Res("UpdateCliente", "{&quot;Id&quot;:1,...}")
        };

        private OpContent ContentGetVentas() => new OpContent
        {
            JsonIn  = NoParams,
            JsonOut = "[\n  " + VentaObj().Replace("\n", "\n  ").TrimEnd() + "\n]",
            SoapReq = Env(ReqNone("GetVentas")),
            SoapRes = Res("GetVentas", "[{&quot;Id&quot;:1,&quot;Total&quot;:30000.00,&quot;Detalles&quot;:[...],...}]")
        };

        private OpContent ContentCreateVenta() => new OpContent
        {
            JsonIn  = "{\n  " + Jk("\"ClienteId\"") + ": " + Jn("1") + ",                  " + Cmt("// int — FK Cliente") + "\n  " +
                                Jk("\"Fecha\"")     + ":     " + Js("\"2026-06-10T00:00:00\"") + ",  " + Cmt("// DateTime") + "\n  " +
                                Jk("\"Detalles\"")  + ": [\n    {\n      " +
                                    Jk("\"ProductoId\"") + ": " + Jn("1") + ",  " + Cmt("// int — FK Producto") + "\n      " +
                                    Jk("\"Cantidad\"")   + ":   " + Jn("2") + "   " + Cmt("// int") + "\n    },\n    { " +
                                    Jk("\"ProductoId\"") + ": " + Jn("3") + ", " + Jk("\"Cantidad\"") + ": " + Jn("1") + " }\n  ]\n}\n" +
                                Cmt("// Total y PrecioUnitario son calculados por el servicio"),
            JsonOut = VentaObj(),
            SoapReq = Env(ReqJson("CreateVenta", "{&quot;ClienteId&quot;:1,&quot;Fecha&quot;:&quot;2026-06-10T00:00:00&quot;,&quot;Detalles&quot;:[{&quot;ProductoId&quot;:1,&quot;Cantidad&quot;:2}]}")),
            SoapRes = Res("CreateVenta", "{&quot;Id&quot;:1,&quot;Total&quot;:30000.00,&quot;Detalles&quot;:[...]}")
        };

        private OpContent ContentDelete(string opName) => new OpContent
        {
            JsonIn  = IdParam,
            JsonOut = Jb("true") + "  " + Cmt("// false si no existe"),
            SoapReq = Env(ReqId(opName)),
            SoapRes = Res(opName, "true")
        };

        // ── Sección Modelos ───────────────────────────────────────────────────

        private string ModelsSection() => @"
<h2>Modelos</h2>

<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-model"">Modelo</span>
    <span class=""op-name"">Categoria</span>
    <span class=""op-desc""></span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <table>
      <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
      <tbody>
        <tr><td class=""prop"">Id</td>    <td class=""t-int"">int</td>   <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
        <tr><td class=""prop"">Nombre</td><td class=""t-str"">string</td><td class=""req"">Requerido</td><td>Nombre de la categor&iacute;a</td></tr>
      </tbody>
    </table>
  </div>
</div>

<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-model"">Modelo</span>
    <span class=""op-name"">Proveedor</span>
    <span class=""op-desc""></span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <table>
      <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
      <tbody>
        <tr><td class=""prop"">Id</td>      <td class=""t-int"">int</td>   <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
        <tr><td class=""prop"">Nombre</td>  <td class=""t-str"">string</td><td class=""req"">Requerido</td><td>Nombre del proveedor</td></tr>
        <tr><td class=""prop"">Telefono</td><td class=""t-str"">string</td><td class=""opt"">Opcional</td> <td>Tel&eacute;fono de contacto</td></tr>
        <tr><td class=""prop"">Email</td>   <td class=""t-str"">string</td><td class=""opt"">Opcional</td> <td>Correo electr&oacute;nico</td></tr>
      </tbody>
    </table>
  </div>
</div>

<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-model"">Modelo</span>
    <span class=""op-name"">Producto</span>
    <span class=""op-desc""></span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <table>
      <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
      <tbody>
        <tr><td class=""prop"">Id</td>         <td class=""t-int"">int</td>    <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
        <tr><td class=""prop"">Nombre</td>     <td class=""t-str"">string</td> <td class=""req"">Requerido</td><td>Nombre del producto</td></tr>
        <tr><td class=""prop"">Precio</td>     <td class=""t-dec"">decimal</td><td class=""opt"">Opcional</td> <td>Precio de venta</td></tr>
        <tr><td class=""prop"">Stock</td>      <td class=""t-int"">int</td>    <td class=""opt"">Opcional</td> <td>Cantidad en almac&eacute;n (se descuenta al crear venta)</td></tr>
        <tr><td class=""prop"">StockMinimo</td><td class=""t-int"">int</td>    <td class=""opt"">Opcional</td> <td>Nivel m&iacute;nimo para alertas</td></tr>
        <tr><td class=""prop"">CategoriaId</td><td class=""t-int"">int</td>    <td class=""req"">Requerido</td><td>FK &rarr; Categoria.Id</td></tr>
        <tr><td class=""prop"">ProveedorId</td><td class=""t-int"">int</td>    <td class=""req"">Requerido</td><td>FK &rarr; Proveedor.Id</td></tr>
      </tbody>
    </table>
  </div>
</div>

<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-model"">Modelo</span>
    <span class=""op-name"">Cliente</span>
    <span class=""op-desc""></span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <table>
      <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
      <tbody>
        <tr><td class=""prop"">Id</td>      <td class=""t-int"">int</td>   <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
        <tr><td class=""prop"">Nombre</td>  <td class=""t-str"">string</td><td class=""req"">Requerido</td><td>Nombre completo</td></tr>
        <tr><td class=""prop"">Email</td>   <td class=""t-str"">string</td><td class=""opt"">Opcional</td> <td>Correo electr&oacute;nico</td></tr>
        <tr><td class=""prop"">Telefono</td><td class=""t-str"">string</td><td class=""opt"">Opcional</td> <td>Tel&eacute;fono de contacto</td></tr>
      </tbody>
    </table>
  </div>
</div>

<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-model"">Modelo</span>
    <span class=""op-name"">Venta / DetalleVenta</span>
    <span class=""op-desc""></span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <div class=""entity-block"">
      <div class=""entity-title"">Venta</div>
      <table>
        <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
        <tbody>
          <tr><td class=""prop"">Id</td>       <td class=""t-int"">int</td>                      <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
          <tr><td class=""prop"">ClienteId</td><td class=""t-int"">int</td>                      <td class=""req"">Requerido</td><td>FK &rarr; Cliente.Id</td></tr>
          <tr><td class=""prop"">Fecha</td>    <td class=""t-str"">DateTime</td>                 <td class=""req"">Requerido</td><td>Fecha y hora de la venta</td></tr>
          <tr><td class=""prop"">Total</td>    <td class=""t-dec"">decimal</td>                  <td class=""opt"">Auto</td>     <td>Calculado por el servicio &mdash; no enviar</td></tr>
          <tr><td class=""prop"">Detalles</td> <td class=""t-list"">List&lt;DetalleVenta&gt;</td><td class=""req"">Requerido</td><td>L&iacute;neas de la venta</td></tr>
        </tbody>
      </table>
    </div>
    <div class=""entity-block"">
      <div class=""entity-title"">DetalleVenta</div>
      <table>
        <thead><tr><th>Propiedad</th><th>Tipo</th><th>Req.</th><th>Descripci&oacute;n</th></tr></thead>
        <tbody>
          <tr><td class=""prop"">Id</td>            <td class=""t-int"">int</td>    <td class=""opt"">Auto</td>     <td>Clave primaria autogenerada</td></tr>
          <tr><td class=""prop"">ProductoId</td>    <td class=""t-int"">int</td>    <td class=""req"">Requerido</td><td>FK &rarr; Producto.Id</td></tr>
          <tr><td class=""prop"">Cantidad</td>      <td class=""t-int"">int</td>    <td class=""req"">Requerido</td><td>Unidades a vender (descuenta stock autom&aacute;ticamente)</td></tr>
          <tr><td class=""prop"">PrecioUnitario</td><td class=""t-dec"">decimal</td><td class=""opt"">Auto</td>     <td>Tomado del precio del producto &mdash; no enviar</td></tr>
        </tbody>
      </table>
    </div>
  </div>
</div>";

        private string SoapUiSection() => @"
<h2>C&oacute;mo consumirlo</h2>
<div class=""card"">
  <div class=""card-header"" onclick=""toggle(this)"">
    <span class=""badge badge-green"">SoapUI</span>
    <span class=""op-name"">Probar con SoapUI</span>
    <span class=""op-desc"">Importar el WSDL y ejecutar operaciones</span>
    <span class=""chevron"">&#9654;</span>
  </div>
  <div class=""card-body"">
    <div class=""section-label"">Pasos</div>
    <ol style=""padding-left:20px;font-size:.87rem;line-height:2.2"">
      <li>Abrir SoapUI &rarr; <strong>File &rarr; New SOAP Project</strong></li>
      <li>En <em>Initial WSDL</em> pegar: <code>http://localhost:51842/InventarioService.svc?wsdl</code></li>
      <li>Clic en <strong>OK</strong> &mdash; SoapUI genera los requests autom&aacute;ticamente</li>
      <li>Para operaciones con <code>json</code>: pegar el JSON en el par&aacute;metro</li>
      <li>Para operaciones con <code>id</code>: escribir el entero directamente</li>
      <li>Ejecutar con el bot&oacute;n <strong>&#9654;</strong></li>
    </ol>
  </div>
</div>";

        // ── Bloques HTML estáticos ────────────────────────────────────────────

        private string HtmlHead() => @"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width,initial-scale=1.0""/>
<title>SistemaInventarioWS &mdash; Documentaci&oacute;n</title>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#f4f6f9;color:#2d3748;line-height:1.6}
header{background:#1a365d;color:#fff;padding:28px 40px;display:flex;align-items:center;justify-content:space-between}
header h1{font-size:1.6rem;font-weight:700}
header .ver{background:#2b6cb0;font-size:.78rem;padding:4px 12px;border-radius:20px;letter-spacing:.5px}
.meta-bar{background:#2a4365;color:#bee3f8;font-size:.82rem;padding:10px 40px;display:flex;gap:32px;flex-wrap:wrap}
.meta-bar a{color:#90cdf4;text-decoration:none}.meta-bar a:hover{text-decoration:underline}
main{max-width:1000px;margin:32px auto;padding:0 24px 60px}
h2{font-size:1.05rem;font-weight:700;color:#1a365d;margin:40px 0 14px;display:flex;align-items:center;gap:10px}
h2::after{content:'';flex:1;height:1px;background:#e2e8f0}
.warn{background:#fff5f5;border-left:4px solid #fc8181;padding:16px 20px;border-radius:0 8px 8px 0;margin:24px 0;font-size:.9rem;color:#742a2a;line-height:1.8}
.card{background:#fff;border:1px solid #e2e8f0;border-radius:10px;overflow:hidden;margin-bottom:12px;box-shadow:0 1px 4px rgba(0,0,0,.05)}
.card-header{display:flex;align-items:center;gap:12px;padding:14px 20px;border-bottom:1px solid #e2e8f0;background:#f7fafc;cursor:pointer;user-select:none}
.card-header:hover{background:#edf2f7}
.badge{font-size:.68rem;font-weight:700;letter-spacing:.6px;padding:3px 9px;border-radius:5px;text-transform:uppercase;white-space:nowrap}
.badge-get{background:#c6f6d5;color:#276749}
.badge-post{background:#bee3f8;color:#2b6cb0}
.badge-put{background:#feebc8;color:#c05621}
.badge-del{background:#fed7d7;color:#c53030}
.badge-green{background:#c6f6d5;color:#276749}
.badge-soap{background:#bee3f8;color:#2b6cb0}
.badge-model{background:#e9d8fd;color:#553c9a}
.op-name{font-size:.95rem;font-weight:700;flex:1}
.op-desc{font-size:.82rem;color:#718096;max-width:340px;text-align:right}
.chevron{font-size:.9rem;color:#a0aec0;transition:transform .2s;flex-shrink:0}
.card-header.open .chevron{transform:rotate(90deg)}
.card-body{padding:20px;display:none}.card-body.open{display:block}
.tabs{display:flex;gap:4px;margin-bottom:12px;border-bottom:2px solid #e2e8f0}
.tab{padding:7px 16px;font-size:.8rem;font-weight:600;color:#718096;cursor:pointer;border-radius:6px 6px 0 0;border:1px solid transparent;border-bottom:none;margin-bottom:-2px}
.tab:hover{color:#2d3748;background:#f7fafc}
.tab.active{color:#2b6cb0;background:#fff;border-color:#e2e8f0;border-bottom-color:#fff}
.tab-panel{display:none}.tab-panel.active{display:block}
.section-label{font-size:.68rem;font-weight:700;text-transform:uppercase;letter-spacing:.8px;color:#a0aec0;margin:18px 0 7px}.section-label:first-child{margin-top:0}
pre{background:#1a202c;color:#e2e8f0;border-radius:8px;padding:14px 18px;font-size:.8rem;overflow-x:auto;line-height:1.75;white-space:pre-wrap;word-break:break-word}
.xml-tag{color:#f687b3}.xml-attr{color:#76e4f7}.xml-val{color:#68d391}
.json-key{color:#90cdf4}.json-str{color:#68d391}.json-num{color:#f6ad55}.json-bool{color:#fc8181}.cmt{color:#4a5568;font-style:italic}
table{width:100%;border-collapse:collapse;font-size:.83rem;margin-top:4px}
th{background:#edf2f7;text-align:left;padding:8px 12px;font-size:.7rem;text-transform:uppercase;letter-spacing:.5px;color:#4a5568;border-bottom:2px solid #e2e8f0}
td{padding:9px 12px;border-bottom:1px solid #f0f4f8;vertical-align:top}
tr:last-child td{border-bottom:none}tr:hover td{background:#f7fafc}
.prop{font-family:monospace;font-weight:700;color:#2d3748;font-size:.85rem}
.t-str{color:#805ad5;font-family:monospace;font-size:.8rem}.t-int{color:#c05621;font-family:monospace;font-size:.8rem}
.t-dec{color:#276749;font-family:monospace;font-size:.8rem}.t-bool{color:#2b6cb0;font-family:monospace;font-size:.8rem}
.t-list{color:#2c7a7b;font-family:monospace;font-size:.8rem}.t-obj{color:#553c9a;font-family:monospace;font-size:.8rem}
.req{color:#c53030;font-size:.72rem;font-weight:700}.opt{color:#a0aec0;font-size:.72rem}
.entity-block{margin-bottom:20px}.entity-block:last-child{margin-bottom:0}
.entity-title{font-size:.8rem;font-weight:700;color:#553c9a;margin-bottom:6px;display:flex;align-items:center;gap:8px}
.entity-title::before{content:'';width:8px;height:8px;background:#805ad5;border-radius:50%;display:inline-block}
footer{text-align:center;font-size:.75rem;color:#a0aec0;margin-top:48px;padding-bottom:32px}
</style>
</head>";

        private string HtmlHeader() => @"
<header>
  <h1>SistemaInventarioWS</h1>
  <span class=""ver"">SOAP / WCF &middot; basicHttpBinding</span>
</header>";

        private string HtmlMetaBar() => @"
<div class=""meta-bar"">
  <span>Endpoint: <strong>InventarioService.svc</strong></span>
  <span><a href=""../InventarioService.svc?wsdl"" target=""_blank"">&#128196; WSDL</a></span>
  <span><a href=""../InventarioService.svc?singleWsdl"" target=""_blank"">&#128196; Single WSDL</a></span>
  <span><a href=""../InventarioService.svc"" target=""_blank"">&#128279; Servicio</a></span>
  <span>Versi&oacute;n: 1.0.0</span>
</div>";

        private string HtmlFooter() => @"
<footer>SistemaInventarioWS &nbsp;&middot;&nbsp; .NET Framework 4.8 &nbsp;&middot;&nbsp; WCF basicHttpBinding</footer>";

        private string ScriptBlock() => @"
<script>
function toggle(h){h.classList.toggle('open');h.nextElementSibling.classList.toggle('open');}
function switchTab(prefix,tab){
  var tabs=['json','xml'];
  var c=document.getElementById('tabs-'+prefix);
  c.querySelectorAll('.tab').forEach(function(t,i){t.classList.toggle('active',tabs[i]===tab);});
  tabs.forEach(function(t){var p=document.getElementById(prefix+'-'+t);if(p)p.classList.toggle('active',t===tab);});
}
</script>";

        // ── Helpers SOAP ──────────────────────────────────────────────────────

        private static string Env(string body) =>
            Xt("&lt;soapenv:Envelope") + " " + Xa("xmlns:soapenv") + "=" + Xv("http://schemas.xmlsoap.org/soap/envelope/") + "\n" +
            "                   " + Xa("xmlns:tem") + "=" + Xv("http://tempuri.org/") + Xt("&gt;") + "\n" +
            "  " + Xt("&lt;soapenv:Header/&gt;") + "\n" +
            "  " + Xt("&lt;soapenv:Body&gt;") + "\n" +
            body + "\n" +
            "  " + Xt("&lt;/soapenv:Body&gt;") + "\n" +
            Xt("&lt;/soapenv:Envelope&gt;");

        private static string Res(string op, string content) =>
            Xt("&lt;s:Envelope") + " " + Xa("xmlns:s") + "=" + Xv("http://schemas.xmlsoap.org/soap/envelope/") + Xt("&gt;") + "\n" +
            "  " + Xt("&lt;s:Body&gt;") + "\n" +
            "    " + Xt("&lt;" + op + "Response") + " " + Xa("xmlns") + "=" + Xv("http://tempuri.org/") + Xt("&gt;") + "\n" +
            "      " + Xt("&lt;" + op + "Result&gt;") + content + Xt("&lt;/" + op + "Result&gt;") + "\n" +
            "    " + Xt("&lt;/" + op + "Response&gt;") + "\n" +
            "  " + Xt("&lt;/s:Body&gt;") + "\n" +
            Xt("&lt;/s:Envelope&gt;");

        private static string ReqNone(string op) => "    " + Xt("&lt;tem:" + op + "/&gt;");

        private static string ReqId(string op) =>
            "    " + Xt("&lt;tem:" + op + "&gt;") + "\n" +
            "      " + Xt("&lt;tem:id&gt;") + "1" + Xt("&lt;/tem:id&gt;") + "\n" +
            "    " + Xt("&lt;/tem:" + op + "&gt;");

        private static string ReqJson(string op, string json) =>
            "    " + Xt("&lt;tem:" + op + "&gt;") + "\n" +
            "      " + Xt("&lt;tem:json&gt;") + json + Xt("&lt;/tem:json&gt;") + "\n" +
            "    " + Xt("&lt;/tem:" + op + "&gt;");

        // ── Plantillas de entrada compartidas ─────────────────────────────────

        private static readonly string NoParams = "<span class=\"cmt\">// Sin parámetros de entrada</span>";

        private static readonly string IdParam =
            Jk("\"id\"") + ": " + Jn("1") + "  " + Cmt("// int — parámetro en el envelope SOAP");

        // ── Syntax highlighting ───────────────────────────────────────────────

        private static string Jk(string s)  => $"<span class=\"json-key\">{s}</span>";
        private static string Js(string s)  => $"<span class=\"json-str\">{s}</span>";
        private static string Jn(string s)  => $"<span class=\"json-num\">{s}</span>";
        private static string Jb(string s)  => $"<span class=\"json-bool\">{s}</span>";
        private static string Cmt(string s) => $"<span class=\"cmt\">{HttpUtility.HtmlEncode(s)}</span>";
        private static string Xt(string s)  => $"<span class=\"xml-tag\">{s}</span>";
        private static string Xa(string s)  => $"<span class=\"xml-attr\">{s}</span>";
        private static string Xv(string s)  => $"<span class=\"xml-val\">\"{HttpUtility.HtmlEncode(s)}\"</span>";
        private static string H(string s)   => HttpUtility.HtmlEncode(s ?? string.Empty);
    }

    internal class OpDoc     { public string Name { get; set; } public string Summary { get; set; } }
    internal class OpContent { public string JsonIn { get; set; } public string JsonOut { get; set; } public string SoapReq { get; set; } public string SoapRes { get; set; } }
}
