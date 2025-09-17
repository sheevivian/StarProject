using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StarProject.Services
{
	/// <summary>
	/// 將 Razor 視圖渲染為字串的服務實作。
	/// </summary>
	public class ViewRenderService : IViewRenderService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IServiceProvider _serviceProvider;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IViewEngine _viewEngine;

		public ViewRenderService(
			IHttpContextAccessor httpContextAccessor,
			IServiceProvider serviceProvider,
			ITempDataProvider tempDataProvider,
			IViewEngine viewEngine)
		{
			_httpContextAccessor = httpContextAccessor;
			_serviceProvider = serviceProvider;
			_tempDataProvider = tempDataProvider;
			_viewEngine = viewEngine;
		}

		public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
		{
			var actionContext = GetActionContext();
			var viewResult = _viewEngine.FindView(actionContext, viewName, false);

			if (viewResult.View == null)
			{
				throw new ArgumentNullException($"{viewName} does not match any available view.");
			}

			var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			{
				Model = model
			};

			var tempDictionary = new TempDataDictionary(_httpContextAccessor.HttpContext, _tempDataProvider);

			using (var writer = new StringWriter())
			{
				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					tempDictionary,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return writer.ToString();
			}
		}

		private ActionContext GetActionContext()
		{
			var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
			return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
		}
	}
}