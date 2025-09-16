using System.Threading.Tasks;

namespace StarProject.Services
{
	/// <summary>
	/// 定義了將 Razor 視圖渲染為字串的服務。
	/// </summary>
	public interface IViewRenderService
	{
		/// <summary>
		/// 異步渲染指定的視圖並將其內容作為字串返回。
		/// </summary>
		/// <typeparam name="TModel">視圖模型的類型。</typeparam>
		/// <param name="viewName">要渲染的視圖名稱。</param>
		/// <param name="model">要傳遞給視圖的模型。</param>
		/// <returns>包含渲染後視圖內容的字串。</returns>
		Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
	}
}