using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
	public class EmpsController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly IEmailService _emailService;

		public EmpsController(StarProjectContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// GET: Emps
		public async Task<IActionResult> Index()
		{
			var emps = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.ToListAsync();
			return View(emps);
		}

		// GET: Emps/Details/5
		public async Task<IActionResult> Details(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.FirstOrDefaultAsync(m => m.No == id);

			if (emp == null)
				return NotFound();

			return View(emp);
		}

		// GET: Emps/Create
		[Authorize(Roles = "HR_Manager")]
		public IActionResult Create()
		{
			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName");
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName");

			var viewModel = new CreateEmpViewModel
			{
				HireDate = DateTime.Today
			};

			return View(viewModel);
		}

		// POST: Emps/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateEmpViewModel viewModel)
		{
			// 除錯資訊
			System.Diagnostics.Debug.WriteLine($"收到的資料: Name={viewModel.Name}, DeptNo={viewModel.DeptNo}, RoleNo={viewModel.RoleNo}, HireDate={viewModel.HireDate}");

			// 檢查ModelState
			if (!ModelState.IsValid)
			{
				System.Diagnostics.Debug.WriteLine("ModelState驗證失敗:");
				foreach (var error in ModelState)
				{
					System.Diagnostics.Debug.WriteLine($"欄位: {error.Key}, 錯誤: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
				}

				ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
				ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
				return View(viewModel);
			}

			try
			{
				// 建立新的Emp物件
				var emp = new Emp
				{
					No = Guid.NewGuid().ToString(),
					Name = viewModel.Name,
					DeptNo = viewModel.DeptNo,
					RoleNo = viewModel.RoleNo,
					HireDate = viewModel.HireDate,
					Email = viewModel.Email,
					Phone = viewModel.Phone,
					IdNumber = viewModel.IdNumber,
					BirthDate = viewModel.BirthDate,
					Status = true,
					ForceChangePassword = true
				};

				// 生成員工編號
				emp.EmpCode = await GenerateEmpCodeAsync(emp.DeptNo);

				// 生成預設密碼及雜湊
				string defaultPassword = "Abc12345";
				(emp.PasswordHash, emp.PasswordSalt) = PasswordHelper.HashPassword(defaultPassword);

				// 存入資料庫
				_context.Emps.Add(emp);
				await _context.SaveChangesAsync();

				// 發送歡迎郵件
				if (!string.IsNullOrEmpty(emp.Email))
				{
					try
					{
						System.Diagnostics.Debug.WriteLine($"準備發送Email到: {emp.Email}");
						// 將以下原本的錯誤行：
						// object emailResult = await _emailService.SendWelcomeEmailAsync(emp.Email, emp.Name, emp.EmpCode, defaultPassword);

						// 改為使用 IEmailService 介面已存在的 SendAsync 方法，並自行組合郵件內容：
						await _emailService.SendAsync(
								emp.Email,
								"🎉 歡迎加入公司",
								$@"<h2>親愛的 {emp.Name}，歡迎加入！</h2>
									<p>您的員工編號是：<b>{emp.EmpCode}</b></p>
									<p>預設密碼為：<b>{defaultPassword}</b></p>
									<p>請盡快登入系統並修改密碼。</p>
									<p>— 人資部</p>",
								isHtml: true
							);

						// 將這一行移除，因為 IEmailService 並沒有 SendWelcomeEmailAsync 方法：
						// object emailResult = await _emailService.SendWelcomeEmailAsync(emp.Email, emp.Name, emp.EmpCode, defaultPassword);
						TempData["EmailSent"] = true;
						System.Diagnostics.Debug.WriteLine("Email發送成功");
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Email發送失敗: {ex.Message}");
						System.Diagnostics.Debug.WriteLine($"詳細錯誤: {ex}");
						TempData["EmailError"] = $"Email發送失敗：{ex.Message}";
					}
				}
				else
				{
					TempData["EmailNotSent"] = "未填寫Email，跳過發送通知";
				}

				// HR 可以看到剛生成的員工編號
				TempData["NewEmpCode"] = emp.EmpCode;
				TempData["TempPassword"] = defaultPassword;

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"建立員工錯誤: {ex.Message}");
				ModelState.AddModelError("", $"建立員工時發生錯誤：{ex.Message}");
			}

			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
			return View(viewModel);
		}

		// 生成員工編號
		private async Task<string> GenerateEmpCodeAsync(int deptNo)
		{
			var dept = await _context.Depts.FindAsync(deptNo);
			if (dept == null)
				throw new Exception("部門不存在");

			string deptCode = dept.DeptCode;

			int count = await _context.Emps.CountAsync(e => e.DeptNo == deptNo);

			string empCode = $"{deptCode}{(count + 1):D3}";
			return empCode;
		}

		// GET: Emps/Edit/5
		public async Task<IActionResult> Edit(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps.FindAsync(id);
			if (emp == null)
				return NotFound();

			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", emp.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", emp.RoleNo);
			return View(emp);
		}

		// POST: Emps/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, [Bind("No,Name,RoleNo,DeptNo,HireDate,PasswordHash,PasswordSalt,EmpCode,Status,ForceChangePassword,Email,Phone,IdNumber,BirthDate")] Emp emp)
		{
			if (id != emp.No)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(emp);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!EmpExists(emp.No))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}

			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", emp.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", emp.RoleNo);
			return View(emp);
		}

		// GET: Emps/Delete/5
		public async Task<IActionResult> Delete(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.FirstOrDefaultAsync(m => m.No == id);

			if (emp == null)
				return NotFound();

			return View(emp);
		}

		// POST: Emps/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var emp = await _context.Emps.FindAsync(id);
			if (emp != null)
			{
				_context.Emps.Remove(emp);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool EmpExists(string id)
		{
			return _context.Emps.Any(e => e.No == id);
		}
	}
}