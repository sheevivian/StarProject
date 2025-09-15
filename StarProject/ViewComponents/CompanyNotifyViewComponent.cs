using Microsoft.AspNetCore.Mvc;
using StarProject.Models;

namespace StarProject.ViewComponents
{
    public class CompanyNotifyViewComponent : ViewComponent
    {
        private readonly StarProjectContext _context;

        public CompanyNotifyViewComponent(StarProjectContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // 例如取最新 5 筆公告
            var notifies = _context.CompanyNotifies
                                   .OrderByDescending(n => n.PublishDate)
                                   .Take(5)
                                   .ToList();

            return View(notifies);
        }
    }
}
