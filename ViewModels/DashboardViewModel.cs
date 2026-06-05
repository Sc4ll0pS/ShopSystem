using ShopSystem.Database;
using ShopSystem.Helpers;

namespace ShopSystem.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private int     _totalProducts, _lowStock, _totalCustomers, _totalOrders, _pendingOrders;
        private decimal _totalRevenue;
        private string  _reportText = "";

        public int     TotalProducts  { get => _totalProducts;  set => SetProperty(ref _totalProducts, value); }
        public int     LowStock       { get => _lowStock;       set => SetProperty(ref _lowStock, value); }
        public int     TotalCustomers { get => _totalCustomers; set => SetProperty(ref _totalCustomers, value); }
        public int     TotalOrders    { get => _totalOrders;    set => SetProperty(ref _totalOrders, value); }
        public int     PendingOrders  { get => _pendingOrders;  set => SetProperty(ref _pendingOrders, value); }
        public decimal TotalRevenue   { get => _totalRevenue;   set => SetProperty(ref _totalRevenue, value); }
        public string  ReportText     { get => _reportText;     set => SetProperty(ref _reportText, value); }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ReportCommand  { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Refresh());
            ReportCommand  = new RelayCommand(_ => GenerateReport());
            Refresh();
        }

        private void Refresh()
        {
            var (tp, ls, tc, to, tr, po) = DatabaseManager.GetDashboardStats();
            TotalProducts  = tp;
            LowStock       = ls;
            TotalCustomers = tc;
            TotalOrders    = to;
            TotalRevenue   = tr;
            PendingOrders  = po;
        }

        private void GenerateReport()
        {
            Refresh();
            var products  = DatabaseManager.GetAllProducts();
            var customers = DatabaseManager.GetAllCustomers();
            var orders    = DatabaseManager.GetAllOrders();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════╗");
            sb.AppendLine("║        SHOP SUMMARY REPORT           ║");
            sb.AppendLine($"║  Generated: {DateTime.Now:yyyy-MM-dd HH:mm}       ║");
            sb.AppendLine("╚══════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine("── OVERVIEW ──────────────────────────");
            sb.AppendLine($"  Total Products   : {TotalProducts}");
            sb.AppendLine($"  Low Stock Items  : {LowStock}");
            sb.AppendLine($"  Total Customers  : {TotalCustomers}");
            sb.AppendLine($"  Total Orders     : {TotalOrders}");
            sb.AppendLine($"  Pending Orders   : {PendingOrders}");
            sb.AppendLine($"  Total Revenue    : {TotalRevenue:N2} ฿");
            sb.AppendLine();

            // Top products by category
            sb.AppendLine("── PRODUCTS BY CATEGORY ──────────────");
            foreach (var grp in products.GroupBy(p => p.Category).OrderBy(g => g.Key))
            {
                sb.AppendLine($"  [{grp.Key}]");
                foreach (var p in grp.OrderBy(x => x.Name))
                    sb.AppendLine($"    • {p.Name,-25} {p.Price,8:N0} ฿  Stock: {p.Stock}");
            }
            sb.AppendLine();

            // Recent orders
            sb.AppendLine("── RECENT ORDERS ─────────────────────");
            foreach (var o in orders.Take(10))
                sb.AppendLine($"  #{o.Id,-4} {o.CustomerName,-20} {o.OrderDate:MM/dd}  {o.TotalAmount,9:N0} ฿  [{o.Status}]");
            sb.AppendLine();

            // Order status breakdown
            var statusGroups = orders.GroupBy(o => o.Status);
            sb.AppendLine("── ORDER STATUS BREAKDOWN ────────────");
            foreach (var grp in statusGroups)
                sb.AppendLine($"  {grp.Key,-12}: {grp.Count()} orders");

            ReportText = sb.ToString();
        }
    }
}
