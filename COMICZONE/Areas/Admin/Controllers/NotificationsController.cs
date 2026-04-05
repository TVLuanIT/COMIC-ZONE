using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NotificationsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public NotificationsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Notifications
        public async Task<IActionResult> Index(string? keyword, string? statusFilter, string? sortColumn, bool isAscending = false, int page = 1)
        {
            var query = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.CreatedByNavigation)
                .AsQueryable();

            // 1. Search (Title, Username, Message)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(n => n.Title.Contains(keyword) || 
                                         n.User.Username.Contains(keyword) ||
                                         n.CreatedByNavigation.Username.Contains(keyword) ||
                                         n.Message.Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            // 2. Sort
            if (string.IsNullOrEmpty(sortColumn)) sortColumn = "CreatedAt";
            query = query.ApplySort(sortColumn, isAscending);

            // 3. Paging
            const int pageSize = 10;
            query = query.ApplyPagination(page, pageSize);

            var searchModel = new AdminSearchModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                SortColumn = sortColumn,
                IsAscending = isAscending,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            ViewBag.SearchModel = searchModel;

            return View(await query.ToListAsync());
        }

        // GET: Admin/Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.CreatedByNavigation)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Admin/Notifications/Create
        public IActionResult Create()
        {
            ViewData["CreatedBy"] = new SelectList(_context.Users.Where(u => u.Role == "Admin"), "Id", "Username");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Admin/Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationId,UserId,CreatedBy,Title,Message,Link,IsRead,CreatedAt")] Notification notification, bool sendToAll = false)
        {
            ModelState.Remove("User");
            ModelState.Remove("CreatedByNavigation");

            if (ModelState.IsValid)
            {
                if (sendToAll)
                {
                    var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
                    var notifications = userIds.Select(id => new Notification
                    {
                        UserId = id,
                        CreatedBy = notification.CreatedBy,
                        Title = notification.Title,
                        Message = notification.Message,
                        Link = notification.Link,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    }).ToList();

                    _context.Notifications.AddRange(notifications);
                }
                else
                {
                    notification.CreatedAt = DateTime.Now;
                    notification.IsRead = false;
                    _context.Add(notification);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedBy"] = new SelectList(_context.Users.Where(u => u.Role == "Admin"), "Id", "Username", notification.CreatedBy);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", notification.UserId);
            return View(notification);
        }

        // GET: Admin/Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["CreatedBy"] = new SelectList(_context.Users.Where(u => u.Role == "Admin"), "Id", "Username", notification.CreatedBy);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", notification.UserId);
            return View(notification);
        }

        // POST: Admin/Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationId,UserId,CreatedBy,Title,Message,Link,IsRead,CreatedAt")] Notification notification)
        {
            ModelState.Remove("User");
            ModelState.Remove("CreatedByNavigation");

            if (id != notification.NotificationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.NotificationId))
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
            ViewData["CreatedBy"] = new SelectList(_context.Users.Where(u => u.Role == "Admin"), "Id", "Username", notification.CreatedBy);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", notification.UserId);
            return View(notification);
        }

        // GET: Admin/Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.CreatedByNavigation)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Admin/Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Notifications/ToggleDeleted
        [HttpPost]
        public async Task<IActionResult> ToggleDeleted(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return Json(new { success = false });

            notification.Isdeleted = !notification.Isdeleted;
            _context.Update(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = notification.Isdeleted });
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}
