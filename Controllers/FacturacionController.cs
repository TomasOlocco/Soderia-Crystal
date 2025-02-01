using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SODERIA_I.Data;
using SODERIA_I.Models;
using SODERIA_I.Services;
using SODERIA_I.ViewModels;
using static SODERIA_I.ViewModels.CompraViewModel;

namespace SODERIA_I.Controllers
{
    public class FacturacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FacturasDigitales _FacturasDigitales;

        public FacturacionController(ApplicationDbContext context, FacturasDigitales facturaPdfService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _FacturasDigitales = facturaPdfService ?? throw new ArgumentNullException(nameof(facturaPdfService));
        }

        public IActionResult GenerarFactura(int clienteId)
        {
            // Obtener el cliente real de la base de datos
            var clienteObj = _context.clientes.FirstOrDefault(c => c.Id == clienteId);
            if (clienteObj == null)
                return NotFound("Cliente no encontrado");

            string clienteNombre = clienteObj.NombreCompleto;
            string numeroWhatsAppCliente = clienteObj.Telefono; // Número de WhatsApp del cliente

            // Obtener las compras del cliente en el último mes
            var compras = _context.compras
                .Where(c => c.clienteId == clienteId && c.fecha >= DateTime.Now.AddMonths(-1))
                .Select(c => new
                {
                    tipoCompra = c.TipoCompraId == 1 ? "Bidón 12 litros" :
                                 c.TipoCompraId == 2 ? "Bidón 20 litros" :
                                 c.TipoCompraId == 3 ? "Sifón" : "Desconocido",
                    cantidad = Convert.ToInt32(c.Cantidad),
                    precio = c.TipoCompraId == 1 ? 100m :
                             c.TipoCompraId == 2 ? 150m :
                             c.TipoCompraId == 3 ? 200m : 0m
                })
                .ToList();

            // Calcular el total de la factura
            decimal total = compras.Sum(x => x.cantidad * x.precio);

            // Convertir la lista a la estructura que espera el método de generación de PDF
            var detalleCompras = compras.Select(x => (x.tipoCompra, x.cantidad, x.precio)).ToList();

            // Generar el PDF con el servicio de facturas digitales
            var pdfBytes = _FacturasDigitales.GenerarFactura(clienteNombre, detalleCompras, total);

            // Asegurar la existencia de la carpeta wwwroot/facturas
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "facturas");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Construir el nombre del archivo reemplazando espacios
            var fileName = $"{clienteNombre.Replace(" ", "_")}_{DateTime.Now:MM_yy}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            // Guardar el PDF en la carpeta
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            // Construir la URL pública de la factura
            var urlFactura = $"{Request.Scheme}://{Request.Host}/facturas/{fileName}";

            // Verificar que el cliente tenga un número de WhatsApp válido
            if (string.IsNullOrWhiteSpace(numeroWhatsAppCliente))
                return BadRequest("El cliente no tiene un número de WhatsApp registrado.");

            // Preparar el mensaje para WhatsApp
            string mensaje = $"Hola {clienteNombre}, tu factura del mes ya está disponible. Descárgala aquí: {urlFactura}";
            string mensajeCodificado = System.Net.WebUtility.UrlEncode(mensaje);
            string urlWhatsApp = $"https://wa.me/{numeroWhatsAppCliente}?text={mensajeCodificado}";

            // Redirigir al usuario a WhatsApp con el mensaje predefinido
            return Redirect(urlWhatsApp);
        }


        // GET: FacturacionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FacturacionController/Create
        [HttpGet]
        public IActionResult Create(int? clienteId, string zonaFiltro)
        {
            // Obtener clientes agrupados por zona
            var clientes = _context.clientes
                .Where(c => c.zona != null)
                .OrderBy(c => c.zona)
                .ToList();

            // Inicializamos el ViewModel
            var viewModel = new CompraViewModel
            {
                Compra = new Compra(),
                Clientes = clientes,
                ComprasDelUltimoMes = new List<Compra>(), // Lista de compras
                ClienteIdSeleccionado = clienteId, // Agregar el cliente seleccionado
                ZonaSeleccionada = zonaFiltro // Agregar la zona seleccionada
            };

            // Si se seleccionó un cliente, traemos sus compras del último mes
            if (clienteId.HasValue)
            {
                var fechaInicio = DateTime.Now.AddMonths(-1); // Último mes

                // Obtener compras del último mes
                viewModel.ComprasDelUltimoMes = _context.compras
                    .Include(c => c.cliente) // Incluir el cliente
                    .Where(c => c.clienteId == clienteId.Value && c.fecha >= fechaInicio)
                    .ToList();
            }

            return View(viewModel);
        }

        // POST: FacturacionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public IActionResult ComprasUltimoMes(int clienteId)
        {
            // Fecha de inicio: 1 mes atrás desde hoy
            var fechaInicio = DateTime.Now.AddMonths(-1);

            // Filtrar compras por cliente y fecha, y agruparlas por tipo de compra
            var comprasUltimoMes = _context.compras
                .Where(c => c.clienteId == clienteId && c.fecha >= fechaInicio)
                .GroupBy(c => c.TipoCompraId)
                .Select(g => new
                {
                    TipoCompraId = g.Key,
                    CantidadTotal = g.Sum(c => c.Cantidad)
                })
                .ToList();

            // Si deseas devolver datos a una vista:
            var viewModel = new CompraViewModel
            {
                ComprasPorTipo = comprasUltimoMes.Select(c => new CompraViewModel.CompraPorTipoViewModel
                {
                    TipoCompraId = c.TipoCompraId,
                    TipoCompraNombre = ObtenerNombreTipoCompra(c.TipoCompraId), // Método para obtener el nombre
                    CantidadTotal = c.CantidadTotal
                }).ToList()
            };

            return View(viewModel);
        }

        // Método para obtener el nombre del tipo de compra
        private string ObtenerNombreTipoCompra(int tipoCompraId)
        {
            var tiposCompra = new List<dynamic>
    {
        new { Id = 1, Tipo = "Bidón 12 litros" },
        new { Id = 2, Tipo = "Bidón 20 litros" },
        new { Id = 3, Tipo = "Sifón" }
    };

            return tiposCompra.FirstOrDefault(t => t.Id == tipoCompraId)?.Tipo ?? "Desconocido";
        }

        // GET: FacturacionController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FacturacionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FacturacionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FacturacionController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
