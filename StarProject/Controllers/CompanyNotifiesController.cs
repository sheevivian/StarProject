using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class CompanyNotifiesController : Controller
    {
        private readonly StarProjectContext _context;

        public CompanyNotifiesController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: CompanyNotifies
        public async Task<IActionResult> Index()
        {
            return View(await _context.CompanyNotifies.ToListAsync());
        }

        // GET: CompanyNotifies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies
                .FirstOrDefaultAsync(m => m.No == id);
            if (companyNotify == null)
            {
                return NotFound();
            }

            return View(companyNotify);
        }

        // GET: CompanyNotifies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CompanyNotifies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("No,Title,Content,Category,PublishDate")] CompanyNotify companyNotify)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyNotify);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(companyNotify);
        }

        // GET: CompanyNotifies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies.FindAsync(id);
            if (companyNotify == null)
            {
                return NotFound();
            }
            return View(companyNotify);
        }

        // POST: CompanyNotifies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Title,Content,Category,PublishDate")] CompanyNotify companyNotify)
        {
            if (id != companyNotify.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyNotify);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyNotifyExists(companyNotify.No))
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
            return View(companyNotify);
        }

        // GET: CompanyNotifies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies
                .FirstOrDefaultAsync(m => m.No == id);
            if (companyNotify == null)
            {
                return NotFound();
            }

            return View(companyNotify);
        }

        // POST: CompanyNotifies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyNotify = await _context.CompanyNotifies.FindAsync(id);
            if (companyNotify != null)
            {
                _context.CompanyNotifies.Remove(companyNotify);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyNotifyExists(int id)
        {
            return _context.CompanyNotifies.Any(e => e.No == id);
        }

		[HttpPost]
		public async Task<IActionResult> UploadImage(IFormFile upload)
		{
			if (upload == null || upload.Length == 0)
				return Json(new { uploaded = false, error = new { message = "No file uploaded." } });

			try
			{
				// 上傳到 ImgBB
				string url = await ImgUploadHelper.UploadToImgBB(upload);

				// 回傳 CKEditor 可接受的 JSON 格式
				return Json(new
				{
					uploaded = true,
					url = url
				});
			}
			catch (Exception ex)
			{
				return Json(new { uploaded = false, error = new { message = ex.Message } });
			}
		}
	}
}
