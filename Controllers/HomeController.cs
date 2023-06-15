using Kontructo.Data;
using Kontructo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Kontructo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext _db)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Products(string currentFilter, string searchString,
                                                                int? pageNumber, string tag)
        {
            ViewData["ProductType"] = new SelectList(db.ProductTypes, "Tag", "Tag");

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var product = from pd in db.Products.Include(p => p.ProductType)
                                                .OrderByDescending(p => p.Id)
                          select pd;

            if (!String.IsNullOrEmpty(searchString))
            {
                product = product.Where(pd => pd.Name.Contains(searchString));
            }


            if (!String.IsNullOrWhiteSpace(tag))
            {
                product = product.Where(pd => pd.ProductType.Tag.Contains(tag));
            }

            int pageSize = 9;

            return View(await PaginatedList<Product>.CreateAsync(product.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        public IActionResult ProductDetails(string url)
        {
            var product = db.Products.Where(pd => pd.Url == url)
                                    .Include(p => p.ProductType);
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}