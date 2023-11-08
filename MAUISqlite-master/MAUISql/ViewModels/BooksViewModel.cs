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
			}, "Fetching products...");
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
			}, "Fetching products...");
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
						await Shell.Current.DisplayAlert("Error", "Product updation error", "Ok");
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
					await Shell.Current.DisplayAlert("Delete Error", "Product was not deleted", "Ok");
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
				/*
				 * {System.TypeInitializationException: The type initializer for 'SQLite.SQLiteConnection' threw an exception.
				 ---> System.IO.FileNotFoundException: Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
				File name: 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b'
				   at SQLitePCL.Batteries_V2.Init()
				   at SQLite.SQLiteConnection..cctor()
				   --- End of inner exception stack trace ---
				   at SQLite.SQLiteConnectionWithLock..ctor(SQLiteConnectionString connectionString)
				   at SQLite.SQLiteConnectionPool.Entry..ctor(SQLiteConnectionString connectionString)
				   at SQLite.SQLiteConnectionPool.GetConnectionAndTransactionLock(SQLiteConnectionString connectionString, Object& transactionLock)
				   at SQLite.SQLiteConnectionPool.GetConnection(SQLiteConnectionString connectionString)
				   at SQLite.SQLiteAsyncConnection.GetConnection()
				   at SQLite.SQLiteAsyncConnection.<>c__DisplayClass33_0`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].<WriteAsync>b__0()
				   at System.Threading.Tasks.Task`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].InnerInvoke()
				   at System.Threading.Tasks.Task.<>c.<.cctor>b__273_0(Object obj)
				   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
				--- End of stack trace from previous location ---
				   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
				   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
				--- End of stack trace from previous location ---
				   at MAUISql.Data.DatabaseContext.<CreateTableIfNotExists>d__6`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 18
				   at MAUISql.Data.DatabaseContext.<GetTableAsync>d__7`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 23
				   at MAUISql.Data.DatabaseContext.<GetAllAsync>d__8`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 29
				   at MAUISql.ViewModels.BooksViewModel.<LoadBooksAsync>b__6_0() in D:\MAUI\MAUISql\MAUISql\ViewModels\BooksViewModel.cs:line 34
				   at MAUISql.ViewModels.BooksViewModel.ExecuteAsync(Func`1 operation, String busyText) in D:\MAUI\MAUISql\MAUISql\ViewModels\BooksViewModel.cs:line 103}
				 */
			}
			finally
			{
				IsBusy = false;
				BusyText = "Processing...";
			}
		}
	}
}

