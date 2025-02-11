using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SODERIA_I.Data;
using SODERIA_I.Models;
using SODERIA_I.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SODERIA_I.Controllers
{
    [Authorize]
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly List<string> _usuariosAutorizados = new List<string>
    {
        "tomasolocco04@gmail.com",
        "gustavoolocco@hotmail.com"
    };

        public ComprasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page)
        {
            // Verifica que el usuario autenticado esté en la lista de autorizados
            if (!_usuariosAutorizados.Contains(User.Identity.Name))
            {
                return Forbid(); // Bloquea el acceso
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            // Armamos la consulta con el orden y demás
            var query = _context.compras
                .OrderByDescending(c => c.fecha)
                .Include(c => c.cliente)
                .AsNoTracking();

            // Obtenemos el total de elementos para la paginación
            int totalItems = query.Count();

            // Aplicamos skip y take para traer solo la página actual
            var comprasList = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Creamos un StaticPagedList que implementa IPagedList<Compra>
            var pagedCompras = new StaticPagedList<Compra>(comprasList, pageNumber, pageSize, totalItems);

            return View(pagedCompras);
        }

        public IActionResult Create()
        {
            // Obtener clientes
            var clientes = _context.clientes
                .Where(c => c.zona != null)
                .OrderBy(c => c.zona)
                .ThenBy(c => c.nombre)
                .ToList();

            // Definir tipos de compra
            var tiposCompra = new List<dynamic>
    {
        new { Id = 1, Tipo = "Bidón 12 litros", Precio = 1500 },
        new { Id = 2, Tipo = "Bidón 20 litros", Precio = 2500 },
        new { Id = 3, Tipo = "Sifón", Precio = 500 },
        new { Id = 4, Tipo = "12 litros (Reparto)", Precio = 1700},
        new { Id = 5, Tipo = "20 litros (Reparto)", Precio = 2800},
        new { Id = 6, Tipo = "Sifón (Reparto)", Precio = 600}
    };

            // Crear el ViewModel
            var viewModel = new CompraViewModel
            {
                Compra = new Compra(), // Compra inicial
                Clientes = clientes,
                TiposCompra = tiposCompra,
                Fecha = DateTime.Now
            };

            return View(viewModel);
        }

        // POST: Compra/Create
        [HttpPost]
        public IActionResult Create(Compra compra)
        {
            // Verificar cliente y tipo de compra
            var cliente = _context.clientes.FirstOrDefault(c => c.Id == compra.clienteId);
            var tipoCompra = new List<dynamic>
    {
        new { Id = 1, Tipo = "Bidón 12 litros", Precio = 1500 },
        new { Id = 2, Tipo = "Bidón 20 litros", Precio = 2500 },
        new { Id = 3, Tipo = "Sifón", Precio = 500 },
        new { Id = 4, Tipo = "12 litros (Reparto)", Precio = 1700},
        new { Id = 5, Tipo = "20 litros (Reparto)", Precio = 2800},
        new { Id = 6, Tipo = "Sifón (Reparto)", Precio = 600}
    }.FirstOrDefault(t => t.Id == compra.TipoCompraId);

            if (cliente == null || tipoCompra == null)
            {
                return RedirectToAction("Index");
            }

            // Calcular el monto
            compra.Monto = tipoCompra.Precio * compra.Cantidad;

            // Asegurar que la fecha no sea sobrescrita ni nula
            compra.fecha = compra.fecha == DateTime.MinValue ? DateTime.Now : compra.fecha;

            compra.ClienteNombre = cliente.NombreCompleto;

            _context.compras.Add(compra);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // GET: Compras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.compras.FindAsync(id);
            if (compra == null)
            {
                return NotFound();
            }

            // Asumiendo que el precio se determina según el TipoCompraId:
            decimal precio = 0;
            switch (compra.TipoCompraId)
            {
                case 1:
                    precio = 1500;
                    break;
                case 2:
                    precio = 2500;
                    break;
                case 3:
                    precio = 500;
                    break;
                // Agregar más casos si es necesario
                case 4:
                    precio = 1700;
                    break;
                case 5:
                    precio = 2800;
                    break;
                case 6:
                    precio = 600;
                    break;
                default:
                    precio = 0;
                    break;
            }

            // Calcular el monto actual con la cantidad existente
            compra.Monto = precio * compra.Cantidad;

            // Podés pasar también el precio a la vista usando ViewBag o un campo oculto en el modelo, por ejemplo:
            ViewBag.Precio = precio;

            return View(compra);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Cantidad")] Compra compra)
        {
            if (id != compra.Id)
            {
                return NotFound();
            }

            // Buscar la compra existente en la base de datos
            var existingCompra = await _context.compras.FindAsync(id);
            if (existingCompra == null)
            {
                return NotFound();
            }

            // Actualizar la cantidad
            existingCompra.Cantidad = compra.Cantidad;

            // Calcular el precio basado en el TipoCompraId
            decimal precio = 0;
            switch (existingCompra.TipoCompraId)
            {
                case 1:
                    precio = 1500;
                    break;
                case 2:
                    precio = 2500;
                    break;
                case 3:
                    precio = 500;
                    break;
                case 4: //12 reparto
                    precio = 1700;
                    break;
                case 5: //20 reparto
                    precio = 2800;
                    break;
                case 6: //sifon reparto
                    precio = 600;
                    break;
            }

            // Recalcular el monto total
            existingCompra.Monto = precio * compra.Cantidad;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompraExists(compra.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "La compra ha sido modificada por otro usuario. Por favor, refresca la página e intenta nuevamente.");
                    return View(compra);
                }
            }

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.compras
                .FirstOrDefaultAsync(m => m.Id == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View(compra);
        }

        // GET: Compras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.compras
                .Include(c => c.cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View(compra);
        }

        // POST: Compras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compra = await _context.compras.FindAsync(id);
            if (compra != null)
            {
                _context.compras.Remove(compra);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompraExists(int id)
        {
            return _context.compras.Any(e => e.Id == id);
        }
    }
}
