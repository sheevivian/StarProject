using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
    internal class StarMapMetadata
    {
        [Display(Name = "名稱")]
        [Required(ErrorMessage = "請輸入景點名稱")]
        public string Name { get; set; } = null!;

        [Display(Name = "描述")]
        [Required(ErrorMessage = "請輸入景點描述")]
        public string Desc { get; set; } = null!;

        [Display(Name = "圖片")]
        public string Image { get; set; } = null!;

        [Display(Name = "圖片")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "地址")]
        [Required(ErrorMessage = "請輸入景點位置")]
        public string Address { get; set; } = null!;

        [Display(Name = "緯度")]
        public decimal MapLatitude { get; set; }

        [Display(Name = "經度")]
        public decimal MapLongitude { get; set; }
    }
}