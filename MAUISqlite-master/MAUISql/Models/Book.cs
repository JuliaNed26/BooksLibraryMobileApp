using SQLite;

namespace MAUISql.Models
{
    public class Book
	{
		[PrimaryKey]
		public Guid Id { get; set; }

		public string Author { get; set; }

		public string Name { get; set; }

		public int PublishingYear { get; set; }

		public int PagesCount { get; set; }

		public string PublishingAddress { get; set; }

		public bool IsSelected { get; set; }

		public (bool IsValid, string? ErrorMessage) Validate()
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				return (false, $"{nameof(Name)} is required.");
			}
			if (string.IsNullOrWhiteSpace(Author))
			{
				return (false, $"{nameof(Author)} is required.");
			}
			if (string.IsNullOrWhiteSpace(PublishingAddress))
			{
				return (false, $"{nameof(PublishingAddress)} is required.");
			}
			if (PagesCount <= 0)
			{
				return (false, $"{nameof(PagesCount)} should be greater than 0.");
			}
			if (PublishingYear <= 0)
			{
				return (false, $"{nameof(PublishingYear)} should be greater than 0.");
			}
			return (true, null);
		}
}
}
