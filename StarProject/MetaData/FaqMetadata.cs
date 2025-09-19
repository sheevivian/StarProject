using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using StarProject.Models;
using System.ComponentModel.DataAnnotations;

namespace StarProject.MetaData
{
	internal class FaqMetadata
	{

		[Display(Name = "類型")]
		[Required(ErrorMessage = "請輸入類型")]
		public int CategoryNo { get; set; }

		[Display(Name = "問題")]
		[Required(ErrorMessage = "請輸入標題")]
		[StringLength(100, ErrorMessage = "問題過長，請修正")]
		public string Question { get; set; } = null!;

		[Display(Name = "說明")]
		[Required(ErrorMessage = "請輸入說明")]
		public string Answer { get; set; } = null!;

		[Display(Name = "更新日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime UpdateDate { get; set; }

		[ValidateNever]
		[Display(Name = "類型")]
		public virtual Faqcategory CategoryNoNavigation { get; set; } = null!;

		public virtual ICollection<Faqkeyword> Faqkeywords { get; set; } = new List<Faqkeyword>();
	}
}