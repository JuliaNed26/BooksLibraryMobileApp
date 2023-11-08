using MAUISql.ViewModels;

namespace MAUISql;

public partial class ContactsListPage : ContentPage
{
	private readonly ContactsViewModel _viewModel;

	public ContactsListPage(ContactsViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.FillFilteredContacts();
	}
}