using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.ViewModels;

namespace StarProject.Controllers
{
	public class ProductStockController : Controller
	{
		private readonly StarProjectContext _context;

		public ProductStockController(StarProjectContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			// Step 1: 先在 DB 層 group by ProductNo (標量欄位)
			var productStocks = await _context.ProductStocks
				.GroupBy(p => p.ProductNo)
				.Select(g => new
				{
					ProductNo = g.Key,
					TotalTransQuantity = g.Sum(x => x.TransQuantity)
				})
				.ToListAsync();

			// Step 2: 把 Product 撈出來
			var productIds = productStocks.Select(x => x.ProductNo).ToList();
			var products = await _context.Products
				.Where(p => productIds.Contains(p.No))
				.ToListAsync();

			// Step 3: join 兩者
			var vm = (from ps in productStocks
					  join p in products on ps.ProductNo equals p.No
					  select new ProductStockSumViewModel
					  {
						  ProductNo = ps.ProductNo,
						  ProductName = p.Name,
						  SumQuantity = ps.TotalTransQuantity
					  }).ToList();

			return View(vm);
		}

	}
}