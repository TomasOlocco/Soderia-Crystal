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
using X.PagedList.Extensions;

namespace SODERIA_I.Controllers
{
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComprasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            // Definir el número de elementos por página
            int pageSize = 3; // Puedes ajustar este valor según tus necesidades
            int pageNumber = (page ?? 1); // Si page es null, usar la página 1

            // Obtener las compras paginadas
            var compras = _context.compras
                .Include(c => c.cliente) // Incluir los datos del cliente
                .Select(c => new Compra
                {
                    Id = c.Id,
                    fecha = c.fecha,
                    Cantidad = c.Cantidad,
                    Monto = c.Monto,
                    TipoCompraId = c.TipoCompraId,
                    cliente = c.cliente // Relación completa para usar en la vista
                })
                .ToPagedList(pageNumber, pageSize); // Aplicar paginación

            return View(compras);
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
        new { Id = 1, Tipo = "Bidón 12 litros", Precio = 100 },
        new { Id = 2, Tipo = "Bidón 20 litros", Precio = 150 },
        new { Id = 3, Tipo = "Sifón", Precio = 200 }
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
        new { Id = 1, Tipo = "Bidón 12 litros", Precio = 100 },
        new { Id = 2, Tipo = "Bidón 20 litros", Precio = 150 },
        new { Id = 3, Tipo = "Sifón", Precio = 200 }
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
            ViewData["clienteId"] = new SelectList(_context.clientes, "Id", "apellido", compra.clienteId);
            return View(compra);
        }

        // POST: Compras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,clienteId,fecha,TipoCompraId,Cantidad,Monto,ClienteNombre")] Compra compra)
        {
            if (id != compra.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compra);
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
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["clienteId"] = new SelectList(_context.clientes, "Id", "apellido", compra.clienteId);
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
