using Microsoft.AspNetCore.Mvc;
using StarProject.Metadatas;

namespace StarProject.Models
{
	[ModelMetadataType(typeof(ProductMetadata))]
	public partial class Product
	{
	}
}