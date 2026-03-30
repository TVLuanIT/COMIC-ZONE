using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Extensions;
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

            // Lấy nội dung báo cáo cụ thể (Review hoặc Reply)
            if (report.ReportTypeEnum == ReportType.Review)
            {
                var review = await _context.ProductReviews
                    .Include(r => r.Product)
                        .ThenInclude(pr => pr.Pictures)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Reviewid == report.Targetid);
                ViewBag.ReportedItem = review;
            }
            else if (report.ReportTypeEnum == ReportType.Reply)
            {
                var reply = await _context.ProductReviewReplies
                    .Include(r => r.User)
                    .Include(r => r.Review.Product)
                        .ThenInclude(pr => pr.Pictures)
                    .FirstOrDefaultAsync(r => r.Replyid == report.Targetid);
                ViewBag.ReportedItem = reply;
            }

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
                    var oldStatus = existingReport.StatusEnum;
                    bool isStatusChanged = existingReport.Status != model.Status;

                    existingReport.Userid = model.Userid;
                    existingReport.Reporttype = model.Reporttype;
                    existingReport.Targetid = model.Targetid;
                    existingReport.Status = model.Status;
                    existingReport.Createdat = model.Createdat;
                    existingReport.Isdeleted = model.Isdeleted;
                    existingReport.Reason = model.Reason;

                    // Gửi thông báo nếu có thay đổi và không phải trường hợp đang bị ẩn (xóa mềm)
                    bool remainsHidden = existingReport.Isdeleted && model.Isdeleted;
                    bool statusChanged = oldStatus != existingReport.StatusEnum;

                    if (!remainsHidden)
                    {
                        var adminIdStr = HttpContext.Session.GetString("UserId");
                        int? adminId = null;
                        if (int.TryParse(adminIdStr, out int parsedId))
                        {
                            adminId = parsedId;
                        }

                        string notifMsg = $"Báo cáo vi phạm #{existingReport.Id} của bạn đã được Admin cập nhật.";
                        if (isStatusChanged)
                        {
                            notifMsg = $"Trạng thái báo cáo vi phạm #{existingReport.Id} đã thay đổi: {oldStatus.GetDisplayName()} ➔ {existingReport.StatusEnum.GetDisplayName()}.";
                        }

                        _context.Notifications.Add(new Notification
                        {
                            UserId = existingReport.Userid,
                            Title = "Cập nhật báo cáo vi phạm",
                            Message = notifMsg,
                            CreatedBy = adminId,
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                            Link = "/UserProfiles/Notifications"
                        });
                    }

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

            // Lấy nội dung báo cáo cụ thể (Review hoặc Reply)
            if (violationReport.ReportTypeEnum == ReportType.Review)
            {
                var review = await _context.ProductReviews
                    .Include(r => r.Product)
                        .ThenInclude(pr => pr.Pictures)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Reviewid == violationReport.Targetid);
                ViewBag.ReportedItem = review;
            }
            else if (violationReport.ReportTypeEnum == ReportType.Reply)
            {
                var reply = await _context.ProductReviewReplies
                    .Include(r => r.User)
                    .Include(r => r.Review.Product)
                        .ThenInclude(pr => pr.Pictures)
                    .FirstOrDefaultAsync(r => r.Replyid == violationReport.Targetid);
                ViewBag.ReportedItem = reply;
            }

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
                // Thêm thông báo trước khi xóa (chỉ thông báo nếu bản ghi chưa bị xóa mềm)
                if (!violationReport.Isdeleted)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = null;
                    if (int.TryParse(adminIdStr, out int parsedId))
                    {
                        adminId = parsedId;
                    }

                    _context.Notifications.Add(new Notification
                    {
                        UserId = violationReport.Userid,
                        Title = "Báo cáo vi phạm bị gỡ",
                        Message = $"Báo cáo vi phạm #{violationReport.Id} của bạn đã bị gỡ/xóa bởi hệ thống.",
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        Link = "/UserProfiles/Notifications"
                    });
                }

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

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var report = await _context.ViolationReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.Isdeleted = !report.Isdeleted;

            // Thêm thông báo
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = report.Userid,
                Title = report.Isdeleted ? "Báo cáo bị gỡ bỏ" : "Báo cáo được khôi phục",
                Message = $"Báo cáo vi phạm #{report.Id} của bạn đã bị " +
                          (report.Isdeleted ? "Admin gỡ bỏ tạm thời khỏi hệ thống." : "Admin khôi phục thành công."),
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/UserProfiles/Notifications"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = report.Isdeleted });
        }
    }
}
