using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SODERIA_I.Data;
using SODERIA_I.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.Extensions;

namespace SODERIA_I.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 3; 
            int pageNumber = (page ?? 1); 

            var clientes = _context.clientes.ToPagedList(pageNumber, pageSize);

            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            ViewBag.Zonas = new List<string> { "Norte", "Sur" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Zonas = new List<string> { "Norte", "Sur" };
                return View(cliente);
            }

            // Si el usuario no ingresa un teléfono, asignamos un valor por defecto
            if (string.IsNullOrWhiteSpace(cliente.Telefono))
            {
                cliente.Telefono = "0000000000";  // Número predeterminado
            }

            _context.clientes.Add(cliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private string FormatearTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return "+549000000000";  // Valor por defecto en caso de error
            }

            // Eliminar espacios, guiones y paréntesis
            telefono = telefono.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Si el usuario ingresó con código de país, no lo modificamos
            if (telefono.StartsWith("+"))
            {
                return telefono;
            }

            // Si el usuario ingresó con "549", lo dejamos así
            if (telefono.StartsWith("549"))
            {
                return $"+{telefono}";
            }

            // Si el usuario ingresó sin código de país, agregamos +54 9
            return $"+549{telefono}";
        }



        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,zona,nombre,apellido")] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
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
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.clientes.Any(e => e.Id == id);
        }
    }
}
