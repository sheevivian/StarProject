using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace StarProject.Helpers
{
	public class PaginationHelper
	{
		/// <summary>
		/// 共用分頁查詢 (直接回傳 Model)
		/// </summary>
		/// <typeparam name="TEntity">資料庫實體類型</typeparam>
		/// <param name="query">IQueryable 查詢來源</param>
		/// <param name="page">第幾頁</param>
		/// <param name="pageSize">每頁大小</param>
		/// <returns>(Items, Total)</returns>
		public static async Task<(IReadOnlyList<TEntity> Items, int Total, int TotalPages)> PaginateAsync<TEntity>(

        IQueryable<TEntity> query,
        int page,
        int pageSize)
        where TEntity : class
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total, totalPages);
        }
	}
}

