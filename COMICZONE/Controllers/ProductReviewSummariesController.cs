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
    public class ProductReviewSummariesController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductReviewSummariesController(ComiczoneContext context)
        {
            _context = context;
        }
    }
}
