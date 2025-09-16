namespace StarProject.Controllers
{
    public partial class ProductController
	{
		public class ImgSaveDto
		{
			public List<ImgOrderDto> ImgData { get; set; }
			public List<int> DeletedIds { get; set; }
		}
    }
}
