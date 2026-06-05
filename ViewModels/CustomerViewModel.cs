using System.Collections.ObjectModel;
using System.Windows;
using ShopSystem.Database;
using ShopSystem.Helpers;
using ShopSystem.Models;

namespace ShopSystem.ViewModels
{
    public class CustomerViewModel : BaseViewModel
    {
        private ObservableCollection<Customer> _customers = new();
        private Customer? _selectedCustomer;
        private string _searchText = "";
        private string _name = "", _email = "", _phone = "", _address = "";
        private bool _isEditing;

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                SetProperty(ref _selectedCustomer, value);
                if (value != null) LoadToForm(value);
                IsEditing = value != null;
            }
        }

        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); Search(); } }
        public string Name       { get => _name;    set => SetProperty(ref _name, value); }
        public string Email      { get => _email;   set => SetProperty(ref _email, value); }
        public string Phone      { get => _phone;   set => SetProperty(ref _phone, value); }
        public string Address    { get => _address; set => SetProperty(ref _address, value); }
        public bool IsEditing    { get => _isEditing; set => SetProperty(ref _isEditing, value); }

        public RelayCommand LoadCommand   { get; }
        public RelayCommand AddCommand    { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand  { get; }

        public CustomerViewModel()
        {
            LoadCommand   = new RelayCommand(_ => LoadCustomers());
            AddCommand    = new RelayCommand(_ => AddCustomer());
            UpdateCommand = new RelayCommand(_ => UpdateCustomer(), _ => SelectedCustomer != null);
            DeleteCommand = new RelayCommand(_ => DeleteCustomer(), _ => SelectedCustomer != null);
            ClearCommand  = new RelayCommand(_ => ClearForm());
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            var list = string.IsNullOrWhiteSpace(SearchText)
                ? DatabaseManager.GetAllCustomers()
                : DatabaseManager.SearchCustomers(SearchText);
            Customers = new ObservableCollection<Customer>(list);
        }

        private void Search() => LoadCustomers();

        private void AddCustomer()
        {
            if (!ValidateForm()) return;
            DatabaseManager.AddCustomer(new Customer
            {
                Name = Name, Email = Email, Phone = Phone,
                Address = Address, RegisteredDate = DateTime.Today
            });
            MessageBox.Show("Customer added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm(); LoadCustomers();
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null || !ValidateForm()) return;
            DatabaseManager.UpdateCustomer(new Customer
            {
                Id = SelectedCustomer.Id, Name = Name,
                Email = Email, Phone = Phone, Address = Address,
                RegisteredDate = SelectedCustomer.RegisteredDate
            });
            MessageBox.Show("Customer updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm(); LoadCustomers();
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;
            var r = MessageBox.Show($"Delete customer '{SelectedCustomer.Name}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Yes)
            {
                DatabaseManager.DeleteCustomer(SelectedCustomer.Id);
                ClearForm(); LoadCustomers();
            }
        }

        private void LoadToForm(Customer c)
        {
            Name = c.Name; Email = c.Email; Phone = c.Phone; Address = c.Address;
        }

        private void ClearForm()
        {
            SelectedCustomer = null;
            Name = Email = Phone = Address = "";
            IsEditing = false;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Name and Email are required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
