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
    public class CartsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public CartsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Carts
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product);
            return View(await comiczoneContext.ToListAsync());
        }

        // GET: Admin/Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Pictures)
                .FirstOrDefaultAsync(m => m.CartId == id);
                
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }


        // GET: Admin/Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", cart.UserId);
            return View(cart);
        }

        // POST: Admin/Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,UserId,CreatedAt")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            // Remove navigation properties from validation to avoid ModelState.IsValid = false
            ModelState.Remove("User");
            ModelState.Remove("CartItems");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin giỏ hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
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
            TempData["Error"] = "Cập nhật thất bại. Vui lòng kiểm tra lại thông tin!";
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", cart.UserId);
            return View(cart);
        }

        // GET: Admin/Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(m => m.CartId == id);
                
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Admin/Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(m => m.CartId == id);

            if (cart != null)
            {
                // Remove all associated items first to avoid foreign key constraint error
                if (cart.CartItems != null && cart.CartItems.Any())
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                }
                
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa giỏ hàng vĩnh viễn thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy giỏ hàng để xóa.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }
    }
}
