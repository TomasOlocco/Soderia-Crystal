using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SODERIA_I.Data;
using SODERIA_I.Models;
using SODERIA_I.Services;
using SODERIA_I.ViewModels;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using static SODERIA_I.ViewModels.CompraViewModel;

namespace SODERIA_I.Controllers
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FacturasDigitales _FacturasDigitales;
        private readonly AzureBlobStorageService _blobStorageService;
        private readonly List<string> _usuariosAutorizados = new List<string>
    {
        "tomasolocco04@gmail.com",
        "gustavoolocco@hotmail.com"
    };

        public FacturacionController(
            ApplicationDbContext context,
            FacturasDigitales facturaPdfService,
            AzureBlobStorageService blobStorageService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _FacturasDigitales = facturaPdfService ?? throw new ArgumentNullException(nameof(facturaPdfService));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        }


        // Método para subir una factura
        public async Task<IActionResult> GenerarFactura(int clienteId)
        {
            // Verifica que el usuario autenticado esté en la lista de autorizados
            if (!_usuariosAutorizados.Contains(User.Identity.Name))
            {
                return Forbid(); // Bloquea el acceso
            }

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
                                 c.TipoCompraId == 3 ? "Sifón" :
                                 c.TipoCompraId == 4 ? "12 litros (Reparto)" :
                                 c.TipoCompraId == 5 ? "20 litros (Reparto)" :
                                 c.TipoCompraId == 6 ? "Sifón (Reparto)" : "Desconocido",
                    cantidad = Convert.ToInt32(c.Cantidad),
                    precio = c.TipoCompraId == 1 ? 1500m :
                             c.TipoCompraId == 2 ? 2500m :
                             c.TipoCompraId == 3 ? 500m :
                             c.TipoCompraId == 4 ? 1700m :
                             c.TipoCompraId == 5 ? 2800m :
                             c.TipoCompraId == 6 ? 600m : 0m
                })
                .ToList();

            // Calcular el total de la factura
            decimal total = compras.Sum(x => x.cantidad * x.precio);

            // Convertir la lista a la estructura que espera el método de generación de PDF
            var detalleCompras = compras.Select(x => (x.tipoCompra, x.cantidad, x.precio)).ToList();

            // Generar el PDF con el servicio de facturas digitales
            var pdfBytes = _FacturasDigitales.GenerarFactura(clienteNombre, detalleCompras, total);

            // Nombre del archivo PDF
            var fileName = $"{clienteNombre.Replace(" ", "_")}_{DateTime.Now:MM_yyyy}.pdf";

            // Subir el PDF a Azure Blob Storage
            using (var stream = new MemoryStream(pdfBytes))
            {
                await _blobStorageService.UploadFileAsync("facturas", fileName, stream);
            }

            // Obtener la cadena de conexión del blob storage (esto depende de cómo esté implementado tu servicio)
            string connectionString = _blobStorageService.GetConnectionString(); // O, si está en la configuración, recuperala

            // Instanciar el BlobContainerClient para el contenedor "facturas"
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, "facturas");

            // Obtener el BlobClient para el archivo subido
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Generar un SAS URI con permisos de lectura, válido por 1 hora
            Uri sasUri = blobClient.GenerateSasUri(
                BlobSasPermissions.Read,
                DateTimeOffset.UtcNow.AddHours(48)
            );

            // Ahora, en lugar de construir la URL manualmente, usamos la SAS URI
            var urlFactura = sasUri.ToString();

            // Verificar que el cliente tenga un número de WhatsApp válido
            if (string.IsNullOrWhiteSpace(numeroWhatsAppCliente))
                return BadRequest("El cliente no tiene un número de WhatsApp registrado.");

            // Preparar el mensaje para WhatsApp
            string mensaje = $"Hola {clienteNombre}, su boleta del mes ya está disponible. Descárgala aquí: {urlFactura}";
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
        new { Id = 3, Tipo = "Sifón" },
        new { Id = 4, Tipo = "12 litros (Reparto)", Precio = 150},
        new { Id = 5, Tipo = "20 litros (Reparto)", Precio = 250},
        new { Id = 6, Tipo = "Sifón (Reparto)", Precio = 300}
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
