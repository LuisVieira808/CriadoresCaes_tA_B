using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CriadoresCaes_tA_B.Data;
using CriadoresCaes_tA_B.Models;

namespace CriadoresCaes_tA_B.Controllers
{
    public class FotografiasController : Controller
    {

       /// <summary>
       /// este atributo representa a base de dados do projeto
       /// </summary>
        private readonly CriadoresCaesDB _context;

        public FotografiasController(CriadoresCaesDB context)
        {
            _context = context;
        }

        // GET: Fotografias
        public async Task<IActionResult> Index()
        {
            //criação de variavel que vai conter um conjunto de dados
            //vindos da base de dados

            var fotografias = _context.Fotografias.Include(f => f.Cao);
            
            
            return View(await fotografias.ToListAsync());
        }

        // GET: Fotografias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fotografias = await _context.Fotografias
                .Include(f => f.Cao)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fotografias == null)
            {
                return NotFound();
            }

            return View(fotografias);
        }

        // GET: Fotografias/Create
        public IActionResult Create()
        {
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id");
            return View();
        }

        // POST: Fotografias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fotografia,DataFoto,Local,CaoFK")] Fotografias fotografias)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fotografias);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id", fotografias.CaoFK);
            return View(fotografias);
        }

        // GET: Fotografias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fotografias = await _context.Fotografias.FindAsync(id);
            if (fotografias == null)
            {
                return NotFound();
            }
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id", fotografias.CaoFK);
            return View(fotografias);
        }

        // POST: Fotografias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fotografia,DataFoto,Local,CaoFK")] Fotografias fotografias)
        {
            if (id != fotografias.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fotografias);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FotografiasExists(fotografias.Id))
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
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id", fotografias.CaoFK);
            return View(fotografias);
        }

        // GET: Fotografias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fotografias = await _context.Fotografias
                .Include(f => f.Cao)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fotografias == null)
            {
                return NotFound();
            }

            return View(fotografias);
        }

        // POST: Fotografias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fotografias = await _context.Fotografias.FindAsync(id);
            _context.Fotografias.Remove(fotografias);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FotografiasExists(int id)
        {
            return _context.Fotografias.Any(e => e.Id == id);
        }
    }
}
