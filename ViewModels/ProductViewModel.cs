using System.Collections.ObjectModel;
using System.Windows;
using ShopSystem.Database;
using ShopSystem.Helpers;
using ShopSystem.Models;

namespace ShopSystem.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products = new();
        private Product? _selectedProduct;
        private string _searchText = "";
        private string _name = "", _category = "", _description = "";
        private decimal _price;
        private int _stock;
        private bool _isEditing;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                if (value != null) LoadToForm(value);
                IsEditing = value != null;
            }
        }

        public string SearchText  { get => _searchText;  set { SetProperty(ref _searchText, value);  Search(); } }
        public string Name        { get => _name;        set => SetProperty(ref _name, value); }
        public string Category    { get => _category;    set => SetProperty(ref _category, value); }
        public decimal Price      { get => _price;       set => SetProperty(ref _price, value); }
        public int Stock          { get => _stock;       set => SetProperty(ref _stock, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public bool IsEditing     { get => _isEditing;   set => SetProperty(ref _isEditing, value); }

        public RelayCommand LoadCommand   { get; }
        public RelayCommand AddCommand    { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand  { get; }

        public ProductViewModel()
        {
            LoadCommand   = new RelayCommand(_ => LoadProducts());
            AddCommand    = new RelayCommand(_ => AddProduct());
            UpdateCommand = new RelayCommand(_ => UpdateProduct(), _ => SelectedProduct != null);
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            ClearCommand  = new RelayCommand(_ => ClearForm());
            LoadProducts();
        }

        private void LoadProducts()
        {
            var list = string.IsNullOrWhiteSpace(SearchText)
                ? DatabaseManager.GetAllProducts()
                : DatabaseManager.SearchProducts(SearchText);
            Products = new ObservableCollection<Product>(list);
        }

        private void Search() => LoadProducts();

        private void AddProduct()
        {
            if (!ValidateForm()) return;
            DatabaseManager.AddProduct(new Product
            {
                Name = Name, Category = Category, Price = Price,
                Stock = Stock, Description = Description
            });
            MessageBox.Show("Product added successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm(); LoadProducts();
        }

        private void UpdateProduct()
        {
            if (SelectedProduct == null || !ValidateForm()) return;
            DatabaseManager.UpdateProduct(new Product
            {
                Id = SelectedProduct.Id, Name = Name, Category = Category,
                Price = Price, Stock = Stock, Description = Description
            });
            MessageBox.Show("Product updated!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm(); LoadProducts();
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            var result = MessageBox.Show(
                $"Delete product '{SelectedProduct.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                DatabaseManager.DeleteProduct(SelectedProduct.Id);
                ClearForm(); LoadProducts();
            }
        }

        private void LoadToForm(Product p)
        {
            Name = p.Name; Category = p.Category;
            Price = p.Price; Stock = p.Stock; Description = p.Description;
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            Name = Category = Description = "";
            Price = 0; Stock = 0;
            IsEditing = false;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Category))
            {
                MessageBox.Show("Name and Category are required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Price <= 0)
            {
                MessageBox.Show("Price must be greater than 0.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
