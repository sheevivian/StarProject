using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class LostInfoController : Controller
    {
		
		private readonly StarProjectContext _context;

        public LostInfoController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: LostInfo
        public async Task<IActionResult> Index()
        {
            return View(await _context.LostInfos.ToListAsync());
        }

        // GET: LostInfo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lostInfo = await _context.LostInfos
                .FirstOrDefaultAsync(m => m.No == id);
            if (lostInfo == null)
            {
                return NotFound();
            }

            return View(lostInfo);
        }

        // GET: LostInfo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LostInfo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LostInfoVM lostInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
					lostInfo.Image = await ImgUploadHelper.UploadToImgBB(lostInfo.ImageFile);

					LostInfo lost = new LostInfo
					{
						Name = lostInfo.Name,
						Desc = lostInfo.Desc,
						Status = lostInfo.Status,
						FoundDate = lostInfo.FoundDate,
						CreatedDate = DateTime.Now,
						OwnerName = null,
						OwnerPhone = null,
						Image = lostInfo.Image
					};

					_context.Add(lost);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
                catch (Exception ex)
				{
					ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
				}
			}
            return View(lostInfo);
        }

        // GET: LostInfo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lostInfo = await _context.LostInfos.FindAsync(id);
            if (lostInfo == null)
            {
                return NotFound();
            }
            return View(lostInfo);
        }

        // POST: LostInfo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Name,Desc,Image,Status,FoundDate,CreatedDate,OwnerName,OwnerPhone")] LostInfo lostInfo)
        {
            if (id != lostInfo.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lostInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LostInfoExists(lostInfo.No))
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
            return View(lostInfo);
        }

        // GET: LostInfo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lostInfo = await _context.LostInfos
                .FirstOrDefaultAsync(m => m.No == id);
            if (lostInfo == null)
            {
                return NotFound();
            }

            return View(lostInfo);
        }

        // POST: LostInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lostInfo = await _context.LostInfos.FindAsync(id);
            if (lostInfo != null)
            {
                _context.LostInfos.Remove(lostInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LostInfoExists(int id)
        {
            return _context.LostInfos.Any(e => e.No == id);
        }
    }
}
