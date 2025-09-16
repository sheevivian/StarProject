using System.Text.Json;

namespace StarProject.Helpers
{
	public class ImgUploadHelper
	{
		private static readonly string ApiKey = "43aab2d4b2cd785f71444760486c0134";
		private static readonly string ApiUrl = "https://api.imgbb.com/1/upload";

		public static async Task<string> UploadToImgBB(IFormFile file)
		{
			using var httpClient = new HttpClient();
			using var form = new MultipartFormDataContent();

			// 將 IFormFile 轉成 ByteArrayContent
			using var ms = new MemoryStream();
			await file.CopyToAsync(ms);
			byte[] fileBytes = ms.ToArray();

			var fileContent = new ByteArrayContent(fileBytes);
			fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

			form.Add(fileContent, "image", file.FileName);

			var response = await httpClient.PostAsync($"{ApiUrl}?key={ApiKey}", form);
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync();
			using var doc = JsonDocument.Parse(json);
			return doc.RootElement.GetProperty("data").GetProperty("url").GetString();
		}
	}
}

