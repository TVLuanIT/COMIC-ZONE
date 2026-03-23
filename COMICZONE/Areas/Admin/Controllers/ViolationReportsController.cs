using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ViolationReportsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public ViolationReportsController(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.ViolationReports
                .Include(v => v.User)
                //.Where(v => !v.Isdeleted)
                .ToListAsync();

            var userIds = reports
                .Select(r => r.Userid)
                .Distinct()
                .ToList();

            var users = (await _context.Users
                .AsNoTracking()
                .ToListAsync())
                .ToDictionary(u => u.Id);

            ViewBag.Users = users;

            var reviewList = await _context.ProductReviews.ToListAsync();
            var replyList = await _context.ProductReviewReplies.ToListAsync();

            ViewBag.ReviewContents = reviewList
                .ToDictionary(r => r.Reviewid, r => r.Reviewcontent);

            ViewBag.ReplyContents = replyList
                .ToDictionary(r => r.Replyid, r => r.Replycontent);

            return View(reports);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var report = await _context.ViolationReports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            // Load review & reply giống Index
            var reviewList = await _context.ProductReviews
                .AsNoTracking()
                .ToListAsync();

            var replyList = await _context.ProductReviewReplies
                .AsNoTracking()
                .ToListAsync();

            ViewBag.ReviewContents = reviewList
                .ToDictionary(r => r.Reviewid, r => r.Reviewcontent);

            ViewBag.ReplyContents = replyList
                .ToDictionary(r => r.Replyid, r => r.Replycontent);

            return View(report);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var report = await _context.ViolationReports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            // Load giống Index / Details
            var reviewList = await _context.ProductReviews
                .AsNoTracking()
                .ToListAsync();

            var replyList = await _context.ProductReviewReplies
                .AsNoTracking()
                .ToListAsync();

            ViewBag.ReviewContents = reviewList
                .ToDictionary(r => r.Reviewid, r => r.Reviewcontent);

            ViewBag.ReplyContents = replyList
                .ToDictionary(r => r.Replyid, r => r.Replycontent);

            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Username", report.Userid);

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ViolationReport model)
        {
            if (id != model.Id)
                return NotFound();

            var existingReport = await _context.ViolationReports
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReport == null)
                return NotFound();

            // bỏ validation navigation property
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    existingReport.Userid = model.Userid;
                    existingReport.Reporttype = model.Reporttype;
                    existingReport.Targetid = model.Targetid;
                    existingReport.Status = model.Status;
                    existingReport.Createdat = model.Createdat;
                    existingReport.Isdeleted = model.Isdeleted;
                    existingReport.Reason = model.Reason;

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ViolationReports.Any(e => e.Id == id))
                        return NotFound();

                    throw;
                }
            }
            
            //foreach (var state in ModelState)
            //{
            //    foreach (var error in state.Value.Errors)
            //    {
            //        Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
            //    }
            //}

            return View(model);
        }

        //// POST: Admin/ViolationReports/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Userid,Reporttype,Targetid,Reason,Status,Createdat,Isdeleted")] ViolationReport violationReport)
        //{
        //    if (id != violationReport.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(violationReport);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ViolationReportExists(violationReport.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", violationReport.Userid);
        //    return View(violationReport);
        //}

        // GET: Admin/ViolationReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationReport = await _context.ViolationReports
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (violationReport == null)
            {
                return NotFound();
            }

            var reviewList = await _context.ProductReviews.ToListAsync();
            var replyList = await _context.ProductReviewReplies.ToListAsync();

            ViewBag.ReviewContents = reviewList
                .ToDictionary(r => r.Reviewid, r => r.Reviewcontent);

            ViewBag.ReplyContents = replyList
                .ToDictionary(r => r.Replyid, r => r.Replycontent);

            return View(violationReport);
        }

        // POST: Admin/ViolationReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var violationReport = await _context.ViolationReports.FindAsync(id);
            if (violationReport != null)
            {
                _context.ViolationReports.Remove(violationReport);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ViolationReportExists(int id)
        {
            return _context.ViolationReports.Any(e => e.Id == id);
        }
    }
}
