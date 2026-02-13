using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ComiczoneContext _context;

        public BlogsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.Blogs.Include(b => b.Author).Include(b => b.Category);
            return View(await comiczoneContext.ToListAsync());
        }
    }
}
