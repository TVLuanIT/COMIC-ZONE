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
    public class ViolationReportsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ViolationReportsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/ViolationReports
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.ViolationReport.Include(v => v.User);
            return View(await comiczoneContext.ToListAsync());
        }

        // GET: Admin/ViolationReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationReport = await _context.ViolationReport
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (violationReport == null)
            {
                return NotFound();
            }

            return View(violationReport);
        }

        // GET: Admin/ViolationReports/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/ViolationReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ReportType,ReviewId,ReplyId,BlogCommentId,ProductId,Reason,IsResolved,CreatedAt")] ViolationReport violationReport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(violationReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", violationReport.UserId);
            return View(violationReport);
        }

        // GET: Admin/ViolationReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationReport = await _context.ViolationReport.FindAsync(id);
            if (violationReport == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", violationReport.UserId);
            return View(violationReport);
        }

        // POST: Admin/ViolationReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ReportType,ReviewId,ReplyId,BlogCommentId,ProductId,Reason,IsResolved,CreatedAt")] ViolationReport violationReport)
        {
            if (id != violationReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(violationReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViolationReportExists(violationReport.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", violationReport.UserId);
            return View(violationReport);
        }

        // GET: Admin/ViolationReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationReport = await _context.ViolationReport
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (violationReport == null)
            {
                return NotFound();
            }

            return View(violationReport);
        }

        // POST: Admin/ViolationReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var violationReport = await _context.ViolationReport.FindAsync(id);
            if (violationReport != null)
            {
                _context.ViolationReport.Remove(violationReport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViolationReportExists(int id)
        {
            return _context.ViolationReport.Any(e => e.Id == id);
        }
    }
}
