using System.Collections.ObjectModel;
using System.Windows;
using ShopSystem.Database;
using ShopSystem.Helpers;
using ShopSystem.Models;

namespace ShopSystem.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private ObservableCollection<Order>    _orders    = new();
        private ObservableCollection<Customer> _customers = new();
        private ObservableCollection<Product>  _products  = new();
        private ObservableCollection<OrderItem> _cartItems = new();
        private Order?    _selectedOrder;
        private Customer? _selectedCustomer;
        private Product?  _selectedProduct;
        private int _quantity = 1;
        private string _selectedStatus = "Pending";
        private decimal _cartTotal;

        public ObservableCollection<Order>    Orders    { get => _orders;    set => SetProperty(ref _orders, value); }
        public ObservableCollection<Customer> Customers { get => _customers; set => SetProperty(ref _customers, value); }
        public ObservableCollection<Product>  Products  { get => _products;  set => SetProperty(ref _products, value); }
        public ObservableCollection<OrderItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set { SetProperty(ref _selectedOrder, value); LoadOrderItems(); }
        }

        public Customer? SelectedCustomer { get => _selectedCustomer; set => SetProperty(ref _selectedCustomer, value); }
        public Product?  SelectedProduct  { get => _selectedProduct;  set => SetProperty(ref _selectedProduct, value); }
        public int       Quantity         { get => _quantity;  set => SetProperty(ref _quantity, value); }
        public string    SelectedStatus   { get => _selectedStatus; set => SetProperty(ref _selectedStatus, value); }
        public decimal   CartTotal        { get => _cartTotal; set => SetProperty(ref _cartTotal, value); }

        public List<string> StatusOptions { get; } = new() { "Pending", "Shipped", "Completed", "Cancelled" };

        public RelayCommand LoadCommand         { get; }
        public RelayCommand AddToCartCommand    { get; }
        public RelayCommand RemoveFromCartCommand { get; }
        public RelayCommand PlaceOrderCommand   { get; }
        public RelayCommand UpdateStatusCommand { get; }
        public RelayCommand DeleteOrderCommand  { get; }
        public RelayCommand ClearCartCommand    { get; }

        public OrderViewModel()
        {
            LoadCommand          = new RelayCommand(_ => LoadAll());
            AddToCartCommand     = new RelayCommand(_ => AddToCart(),      _ => SelectedProduct != null && Quantity > 0);
            RemoveFromCartCommand= new RelayCommand(item => RemoveFromCart(item as OrderItem), _ => CartItems.Count > 0);
            PlaceOrderCommand    = new RelayCommand(_ => PlaceOrder(),     _ => SelectedCustomer != null && CartItems.Count > 0);
            UpdateStatusCommand  = new RelayCommand(_ => UpdateStatus(),   _ => SelectedOrder != null);
            DeleteOrderCommand   = new RelayCommand(_ => DeleteOrder(),    _ => SelectedOrder != null);
            ClearCartCommand     = new RelayCommand(_ => CartItems.Clear());
            LoadAll();
        }

        public void LoadAll()
        {
            Customers = new ObservableCollection<Customer>(DatabaseManager.GetAllCustomers());
            Products  = new ObservableCollection<Product>(DatabaseManager.GetAllProducts());
            Orders    = new ObservableCollection<Order>(DatabaseManager.GetAllOrders());
        }

        private void LoadOrderItems()
        {
            if (SelectedOrder == null) return;
            var items = DatabaseManager.GetOrderItems(SelectedOrder.Id);
            SelectedOrder.Items = items;
        }

        private void AddToCart()
        {
            if (SelectedProduct == null) return;
            var existing = CartItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
            if (existing != null)
                existing.Quantity += Quantity;
            else
                CartItems.Add(new OrderItem
                {
                    ProductId   = SelectedProduct.Id,
                    ProductName = SelectedProduct.Name,
                    Quantity    = Quantity,
                    UnitPrice   = SelectedProduct.Price
                });
            // Force UI refresh
            CartItems = new ObservableCollection<OrderItem>(CartItems);
            RecalculateTotal();
        }

        private void RemoveFromCart(OrderItem? item)
        {
            if (item == null) return;
            CartItems.Remove(item);
            CartItems = new ObservableCollection<OrderItem>(CartItems);
            RecalculateTotal();
        }

        private void RecalculateTotal()
            => CartTotal = CartItems.Sum(i => i.Subtotal);

        private void PlaceOrder()
        {
            if (SelectedCustomer == null || CartItems.Count == 0) return;
            var order = new Order
            {
                CustomerId  = SelectedCustomer.Id,
                OrderDate   = DateTime.Today,
                TotalAmount = CartTotal,
                Status      = "Pending",
                Items       = CartItems.ToList()
            };
            DatabaseManager.AddOrder(order);
            MessageBox.Show("Order placed successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            CartItems.Clear(); CartTotal = 0;
            LoadAll();
        }

        private void UpdateStatus()
        {
            if (SelectedOrder == null) return;
            DatabaseManager.UpdateOrderStatus(SelectedOrder.Id, SelectedStatus);
            MessageBox.Show("Order status updated!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAll();
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null) return;
            var r = MessageBox.Show($"Delete Order #{SelectedOrder.Id}?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Yes)
            {
                DatabaseManager.DeleteOrder(SelectedOrder.Id);
                LoadAll();
            }
        }
    }
}
