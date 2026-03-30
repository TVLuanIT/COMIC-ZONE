using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryLogsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public InventoryLogsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/InventoryLogs
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.InventoryLogs.Include(i => i.Product);
            return View(await comiczoneContext.ToListAsync());
        }

        // GET: Admin/InventoryLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryLog == null)
            {
                return NotFound();
            }

            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Admin/InventoryLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,ChangeAmount,Type,CreatedAt")] InventoryLog inventoryLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventoryLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs.FindAsync(id);
            if (inventoryLog == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // POST: Admin/InventoryLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,ChangeAmount,Type,CreatedAt")] InventoryLog inventoryLog)
        {
            if (id != inventoryLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventoryLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryLogExists(inventoryLog.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryLog == null)
            {
                return NotFound();
            }

            return View(inventoryLog);
        }

        // POST: Admin/InventoryLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryLog = await _context.InventoryLogs.FindAsync(id);
            if (inventoryLog != null)
            {
                _context.InventoryLogs.Remove(inventoryLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryLogExists(int id)
        {
            return _context.InventoryLogs.Any(e => e.Id == id);
        }
    }
}
