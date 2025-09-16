namespace StarProject.DTOs
{
	// DTO for role updates
	public class RoleUpdateDto
	{
		public int RoleId { get; set; }
		public bool Emp { get; set; }
		public bool User { get; set; }
		public bool Info { get; set; }
		public bool Event { get; set; }
		public bool Pd { get; set; }
		public bool Tic { get; set; }
		public bool Pm { get; set; }
		public bool Order { get; set; }
		public bool Cs { get; set; }
		public bool Oa { get; set; }
		public bool CoNlist { get; set; }
		public bool CoNe { get; set; }
	}
}
