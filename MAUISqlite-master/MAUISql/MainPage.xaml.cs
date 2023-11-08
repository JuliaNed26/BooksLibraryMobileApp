using MAUISql.ViewModels;

namespace MAUISql;

public partial class MainPage : ContentPage
{
    private readonly BooksViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public MainPage(BooksViewModel viewModel, IServiceProvider services)
	{
		_viewModel = viewModel;
		InitializeComponent();
        BindingContext= viewModel;
        _serviceProvider = services;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBooksAsync();
    }

    private async void RedirectToAuthorInfoPage(object sender, EventArgs e)
    {
	    await Navigation.PushAsync(_serviceProvider.GetRequiredService<AuthorInfoPage>());
    }

    private async void RedirectToFilteredContactsPage(object sender, EventArgs e)
    {
	    await Navigation.PushAsync(_serviceProvider.GetRequiredService<ContactsListPage>());
    }

    private async void FilterPicker_OnSelectedIndexChanged(object sender, EventArgs e)
    {
	    _viewModel.SelectedFilterOption = _viewModel.FilterOptions.ElementAt(FilterPicker.SelectedIndex);
		switch (_viewModel.SelectedFilterOption)
		{
			case "All":
				await _viewModel.LoadBooksAsync();
				break;
			case "> 10":
				await _viewModel.GetOlderThanTenYears();
				break;
		}
}

    private void ChangePercentageProperty(object sender, CheckedChangedEventArgs e)
    {
        _viewModel.UpdatePercentagePicked();
    }
}

