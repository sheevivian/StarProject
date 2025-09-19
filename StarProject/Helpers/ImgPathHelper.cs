using System.Text.Json;

namespace StarProject.Helpers
{
	public class ImgPathHelper
	{
		private static readonly string ApiKey = "9a256f4abcba9979ece5b6db3da4a566";
		private static readonly string ApiUrl = "https://api.imgbb.com/1/upload";

		public static async Task<string> UploadToImgBB(string filePath)
		{
			using (var httpClient = new HttpClient())
			using (var form = new MultipartFormDataContent())
			{
				byte[] imageBytes = File.ReadAllBytes(filePath);
				var imageContent = new ByteArrayContent(imageBytes);
				imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/*");

				// ✅ 把圖片加入表單
				form.Add(imageContent, "image", Path.GetFileName(filePath));

				string requestUrl = $"{ApiUrl}?key={ApiKey}";
				HttpResponseMessage response = await httpClient.PostAsync(requestUrl, form);
				response.EnsureSuccessStatusCode();

				string jsonResponse = await response.Content.ReadAsStringAsync();
				using (var doc = JsonDocument.Parse(jsonResponse))
				{
					string url = doc.RootElement.GetProperty("data").GetProperty("url").GetString();
					return url;
				}
			}
		}
	}
}
