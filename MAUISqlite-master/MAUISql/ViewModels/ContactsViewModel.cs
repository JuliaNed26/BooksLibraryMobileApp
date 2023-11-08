using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MAUISql.ViewModels;
public partial class ContactsViewModel : ObservableObject
{
	[ObservableProperty] private ObservableCollection<Contact> _filteredContacts = new();

	public async Task FillFilteredContacts()
	{
		var contacts = await Microsoft.Maui.ApplicationModel.Communication.Contacts.GetAllAsync();
		var filteredContacts= contacts.Where(c => c.DisplayName.EndsWith("ко"));
		foreach (var contact in filteredContacts)
		{
			FilteredContacts.Add(contact);
		}
	}
}
