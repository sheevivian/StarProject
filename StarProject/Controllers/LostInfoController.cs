using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Core.Types;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
                        Category = lostInfo.Category,
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
			// 轉成 ViewModel
			var vm = new LostInfoVM
			{
				No = lostInfo.No,
				Name = lostInfo.Name,
				Category = lostInfo.Category,
				Desc = lostInfo.Desc,
				Image = lostInfo.Image,
				Status = lostInfo.Status,
				FoundDate = lostInfo.FoundDate,
				CreatedDate = lostInfo.CreatedDate,
				OwnerName = lostInfo.OwnerName,
				OwnerPhone = lostInfo.OwnerPhone
			};

			return View(vm);
		}

        // POST: LostInfo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Name,Category,Desc,Image,Status,FoundDate,CreatedDate,OwnerName,OwnerPhone,ImageFile")] LostInfoVM lostInfo)
        {
			var original = await _context.LostInfos.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();

			// 保持原本的 CreatedDate，不讓它被編輯
			lostInfo.CreatedDate = original.CreatedDate;
			lostInfo.Image = original.Image;

			if (id != lostInfo.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (lostInfo.ImageFile != null)
                    {
						lostInfo.Image = await ImgUploadHelper.UploadToImgBB(lostInfo.ImageFile);
                    }
                    else
                    {
                        lostInfo.Image = original.Image;
					}

					LostInfo lost = new LostInfo
					{
                        No = id,
						Name = lostInfo.Name,
						Category = lostInfo.Category,
						Desc = lostInfo.Desc,
						Status = lostInfo.Status,
						FoundDate = lostInfo.FoundDate,
						CreatedDate = original.CreatedDate,
						OwnerName = lostInfo.OwnerName,
						OwnerPhone = lostInfo.OwnerPhone,
						Image = lostInfo.Image
					};

					_context.Update(lost);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest();
			}

            var items = await _context.LostInfos.Where(x => ids.Contains(x.No)).ToListAsync();
			
            if (items.Any())
			{
				_context.LostInfos.RemoveRange(items);
				await _context.SaveChangesAsync();
			}

			return Ok();
		}

		// POST: LostInfo/SearchSelect
		[HttpPost]
		public IActionResult SearchSelect([FromBody] SearchFilterVM filters)
		{
			var query = _context.LostInfos.AsQueryable();

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.Name.Contains(filters.keyword)
									 || x.Category.Contains(filters.keyword)
									 || x.Status.Contains(filters.keyword));
			}

			// 分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.Category));

			// 狀態
			if (filters.Statuses != null && filters.Statuses.Any())
				query = query.Where(x => filters.Statuses.Contains(x.Status));

			// 日期區間
			if (!string.IsNullOrEmpty(filters.DateFrom))
				query = query.Where(x => x.FoundDate >= DateTime.Parse(filters.DateFrom));

			if (!string.IsNullOrEmpty(filters.DateTo))
				query = query.Where(x => x.FoundDate <= DateTime.Parse(filters.DateTo));

			return PartialView("_LostInfoRows", query.ToList());
		}

		private bool LostInfoExists(int id)
        {
            return _context.LostInfos.Any(e => e.No == id);
        }
    }
}
