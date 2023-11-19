using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUISql.Data;
using MAUISql.Models;
using System.Collections.ObjectModel;

namespace MAUISql.ViewModels
{
	public partial class BooksViewModel : ObservableObject
	{
		private readonly DatabaseContext _context;

		public BooksViewModel(DatabaseContext context)
		{
			_context = context;
		}

		[ObservableProperty] private ObservableCollection<Book> _books = new();

		[ObservableProperty] private Book _operatingBook = new();

		[ObservableProperty] private bool _isBusy;

		[ObservableProperty] private string _busyText;

		[ObservableProperty] private string _selectedFilterOption = "All";

		[ObservableProperty] private double _pickedPercentage;

		public IEnumerable<string> FilterOptions { get; } = new List<string> { "All", "> 10" };

		public async Task LoadBooksAsync()
		{
			await ExecuteAsync(async () =>
			{
				var books = await _context.GetAllAsync<Book>();
				if (books is not null && books.Any())
				{
					Books = new ObservableCollection<Book>();

					foreach (var book in books)
					{
						Books.Add(book);
					}
				}
			}, "Fetching books...");
		}

		public async Task GetOlderThanTenYears()
		{
			await ExecuteAsync(async () =>
			{
				var books = await _context.GetAllAsync<Book>();
				books = books.Where(b => DateTime.UtcNow.Year - b.PublishingYear > 10).ToList();
				if (books.Any())
				{
					Books = new ObservableCollection<Book>();

					foreach (var book in books)
					{
						Books.Add(book);
					}
				}
			}, "Fetching books...");
		}

		public void UpdatePercentagePicked()
		{
			var picked = Books.Count(b => b.IsSelected);
			PickedPercentage = picked * 100.0 / Books.Count;
		}

		[RelayCommand]
		private void SetOperatingBook(Book? book) => OperatingBook = book ?? new();

		[RelayCommand]
		private async Task SaveBookAsync()
		{
			if (OperatingBook is null)
				return;

			var (isValid, errorMessage) = OperatingBook.Validate();
			if (!isValid)
			{
				await Shell.Current.DisplayAlert("Validation Error", errorMessage, "Ok");
				return;
			}

			var busyText = OperatingBook.Id == Guid.Empty ? "Creating book..." : "Updating book...";
			await ExecuteAsync(async () =>
			{
				if (OperatingBook.Id == Guid.Empty)
				{
					// Create book
					OperatingBook.Id = Guid.NewGuid();
					await _context.AddItemAsync<Book>(OperatingBook);
					Books.Add(OperatingBook);
				}
				else
				{
					// Update book
					if (await _context.UpdateItemAsync<Book>(OperatingBook))
					{
						var bookCopy = new Book()
						{
							Id = OperatingBook.Id,
							Name = OperatingBook.Name,
							Author = OperatingBook.Author,
							PagesCount = OperatingBook.PagesCount,
							PublishingYear = OperatingBook.PublishingYear,
							PublishingAddress = OperatingBook.PublishingAddress,
							IsSelected = OperatingBook.IsSelected
						};

						var index = Books.IndexOf(OperatingBook);
						Books.RemoveAt(index);

						Books.Insert(index, bookCopy);
					}
					else
					{
						await Shell.Current.DisplayAlert("Error", "Book updation error", "Ok");
						return;
					}
				}

				SetOperatingBookCommand.Execute(new());
			}, busyText);
		}

		[RelayCommand]
		private async Task DeleteBookAsync(Guid id)
		{
			await ExecuteAsync(async () =>
			{
				if (await _context.DeleteItemByKeyAsync<Book>(id))
				{
					var book = Books.FirstOrDefault(p => p.Id == id);
					Books.Remove(book);
				}
				else
				{
					await Shell.Current.DisplayAlert("Delete Error", "Book was not deleted", "Ok");
				}
			}, "Deleting book...");
		}

		private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
		{
			IsBusy = true;
			BusyText = busyText ?? "Processing...";
			try
			{
				await operation?.Invoke();
			}
			catch (Exception ex)
			{
			}
			finally
			{
				IsBusy = false;
				BusyText = "Processing...";
			}
		}
	}
}

