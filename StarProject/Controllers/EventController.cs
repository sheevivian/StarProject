using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.Data;

namespace StarProject.Controllers
{
	public class EventController : Controller
	{
		private readonly StarProjectContext _context;

		public EventController(StarProjectContext context)
		{
			_context = context;
		}

		// ===================1) 活動列表頁（Index）=====================
		// 
		// 路由：GET /Event 或 /Event/Index
		// 功能：讀取所有活動資料，按建立時間由新到舊排序後，傳給 View 顯示
		// 對應 View：Views/Event/Index.cshtml（模型型別 IEnumerable<Event>）
		// ============================================================

		public async Task<IActionResult> Index()
		{
			// 從資料庫查詢所有 Event，並依 CreatedTime 進行倒序排列
			var events = await _context.Events
										.OrderByDescending(e => e.CreatedTime)
										.ToListAsync();

			// 將查詢結果傳給 View（Razor 頁面）渲染
			return View(events);
		}

		// =================2) 活動詳情頁（Details）=====================
		// 路由：GET /Event/Details/{id}
		// 功能：依主鍵 no 取出單筆活動顯示（包含圖片網址）
		// 對應 View：Views/Event/Details.cshtml（模型型別 Event）
		// ============================================================

		public async Task<IActionResult> Details(int? no)
		{
			// 基本防呆：沒有 id 就回傳 404
			if (no == null) 
				return NotFound();

			// 依主鍵no查資料
			var ev = await _context.Events.FirstOrDefaultAsync(m => m.No == no);

			// 若找不到資料則回傳404
			if (ev == null)
				return NotFound();

			//傳給詳情頁面
			return View(ev);
		}

		// ================(3) 新增活動（Create）- 顯示表單===============
		// 路由：GET /Event/Create
		// 功能：只回傳空白表單（讓使用者輸入）
		// 對應 View：Views/Event/Create.cshtml
		// ============================================================

		public IActionResult Create()
		{ 
			return View();
		}

		// =============(3) 新增活動（Create）- 表單送出==================

		// 路由：POST /Event/Create
		// 屬性：
		//   [HttpPost] 只接 POST
		//   [ValidateAntiForgeryToken] 防範 CSRF（表單內需含有防偽權杖）
		//   [Bind(...)] 防止 overposting（只綁定允許的欄位）
		// 功能：驗證 → 設定系統欄位 → 寫入 DB → 轉回列表
		// ============================================================

	}
}
