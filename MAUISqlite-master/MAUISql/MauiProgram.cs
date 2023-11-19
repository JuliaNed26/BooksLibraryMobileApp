using MAUISql.Data;
using MAUISql.ViewModels;
using Microsoft.Extensions.Logging;

namespace MAUISql;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<DatabaseContext>();
		builder.Services.AddSingleton<BooksViewModel>();
		builder.Services.AddSingleton<ContactsViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<ContactsListPage>();
		builder.Services.AddSingleton<AuthorInfoPage>();

		return builder.Build();
	}
}
