using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomersController : Controller
    {
        private readonly ComiczoneContext _context;

        public CustomersController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Index(CustomerSearchModel search)
        {
            var query = _context.Customers.Include(c => c.User).AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(c => c.Fullname.Contains(search.Keyword) ||
                                        c.Phone.Contains(search.Keyword) ||
                                        c.Address.Contains(search.Keyword) ||
                                        c.User.Email.Contains(search.Keyword));
            }

            if (!string.IsNullOrEmpty(search.FullName))
            {
                query = query.Where(c => c.Fullname.Contains(search.FullName));
            }

            if (!string.IsNullOrEmpty(search.Phone))
            {
                query = query.Where(c => c.Phone.Contains(search.Phone));
            }

            if (!string.IsNullOrEmpty(search.Email))
            {
                query = query.Where(c => c.User.Email.Contains(search.Email));
            }

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(c => c.Isdeleted == search.IsDeleted.Value);
            }

            if (search.CreatedFrom.HasValue)
            {
                query = query.Where(c => c.Createdat >= search.CreatedFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(c => c.Createdat <= search.CreatedTo.Value);
            }

            // Sorting
            search.SortColumn = search.SortColumn ?? "Customerid";
            bool isAsc = search.IsAscending;

            query = search.SortColumn switch
            {
                "Fullname" => isAsc ? query.OrderBy(c => c.Fullname) : query.OrderByDescending(c => c.Fullname),
                "Phone" => isAsc ? query.OrderBy(c => c.Phone) : query.OrderByDescending(c => c.Phone),
                "Address" => isAsc ? query.OrderBy(c => c.Address) : query.OrderByDescending(c => c.Address),
                "Createdat" => isAsc ? query.OrderBy(c => c.Createdat) : query.OrderByDescending(c => c.Createdat),
                "Email" => isAsc ? query.OrderBy(c => c.User.Email) : query.OrderByDescending(c => c.User.Email),
                _ => isAsc ? query.OrderBy(c => c.Customerid) : query.OrderByDescending(c => c.Customerid)
            };

            // Pagination
            search.TotalCount = await query.CountAsync();
            var customers = await query
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            ViewBag.SearchModel = search;

            return View(customers);
        }

        // GET: Admin/Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Customerid == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Admin/Customers/Create
        public IActionResult Create()
        {
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Admin/Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Customerid,Userid,Fullname,Phone,Address,Createdat")] Customer customer)
        {
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Username", customer.Userid);
            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Customerid == id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Username", customer.Userid);
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Customerid,Userid,Fullname,Phone,Address,Createdat")] Customer customer)
        {
            if (id != customer.Customerid)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Customerid))
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
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Username", customer.Userid);
            customer.User = await _context.Users.FindAsync(customer.Userid);
            return View(customer);
        }

        // GET: Admin/Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Customerid == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var customer = await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.Customerid == id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.Isdeleted = !customer.Isdeleted;
            
            // Optional: If deleted, we could also deactivate the user or vice versa
            // customer.User.Isdeleted = customer.Isdeleted;

            // Notification
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = customer.Userid,
                Title = customer.Isdeleted ? "Hồ sơ đã bị vô hiệu hóa" : "Hồ sơ đã được khôi phục",
                Message = customer.Isdeleted 
                    ? $"Hồ sơ khách hàng '{customer.Fullname}' đã bị vô hiệu hóa bởi Quản trị viên." 
                    : $"Hồ sơ khách hàng '{customer.Fullname}' đã được Quản trị viên khôi phục.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/Account/UserProfiles/MyProfile"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = customer.Isdeleted });
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Customerid == id);
        }
    }
}
