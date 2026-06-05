using Microsoft.Data.Sqlite;
using ShopSystem.Models;

namespace ShopSystem.Database
{
    public static class DatabaseManager
    {
        private static readonly string ProductsDb  = "Data Source=products.db";
        private static readonly string CustomersDb = "Data Source=customers.db";
        private static readonly string OrdersDb    = "Data Source=orders.db";

        public static void InitializeAllDatabases()
        {
            InitializeProductsDb();
            InitializeCustomersDb();
            InitializeOrdersDb();
        }

        private static void InitializeProductsDb()
        {
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name        TEXT    NOT NULL,
                    Category    TEXT    NOT NULL,
                    Price       REAL    NOT NULL,
                    Stock       INTEGER NOT NULL DEFAULT 0,
                    Description TEXT
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT COUNT(*) FROM Products;";
            long count = (long)(cmd.ExecuteScalar() ?? 0);
            if (count == 0)
            {
                cmd.CommandText = @"
                    INSERT INTO Products (Name, Category, Price, Stock, Description) VALUES
                    ('Laptop Pro 15',    'Electronics', 35990, 15, 'High-performance laptop'),
                    ('Wireless Mouse',   'Electronics',  890,  50, 'Ergonomic wireless mouse'),
                    ('USB-C Hub',        'Electronics', 1290,  30, '7-in-1 USB-C hub'),
                    ('Notebook A5',      'Stationery',   120, 200, 'Premium lined notebook'),
                    ('Ballpoint Pen',    'Stationery',    35, 500, 'Smooth writing pen'),
                    ('Desk Lamp',        'Furniture',    690,  20, 'LED adjustable desk lamp'),
                    ('Office Chair',     'Furniture',  4990,   8, 'Ergonomic office chair'),
                    ('Mechanical Keyboard','Electronics',2490, 25, 'Tactile mechanical keyboard'),
                    ('Monitor 24in',     'Electronics',7990,  10, 'Full HD IPS monitor'),
                    ('Cable Organizer',  'Accessories',  199,  80, 'Desktop cable management');";
                cmd.ExecuteNonQuery();
            }
        }

        private static void InitializeCustomersDb()
        {
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name           TEXT NOT NULL,
                    Email          TEXT NOT NULL UNIQUE,
                    Phone          TEXT,
                    Address        TEXT,
                    RegisteredDate TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT COUNT(*) FROM Customers;";
            long count = (long)(cmd.ExecuteScalar() ?? 0);
            if (count == 0)
            {
                cmd.CommandText = @"
                    INSERT INTO Customers (Name, Email, Phone, Address, RegisteredDate) VALUES
                    ('Somchai Jaidee',   'somchai@email.com',  '081-111-1111', '10 Sukhumvit Rd, Bangkok',  '2024-01-15'),
                    ('Nipa Rakdee',      'nipa@email.com',     '082-222-2222', '25 Silom Rd, Bangkok',      '2024-02-20'),
                    ('Prasit Meechai',   'prasit@email.com',   '083-333-3333', '5 Rama 9, Bangkok',         '2024-03-10'),
                    ('Wanida Somboon',   'wanida@email.com',   '084-444-4444', '88 Lat Phrao, Bangkok',     '2024-04-05'),
                    ('Kritchai Boonma',  'kritchai@email.com', '085-555-5555', '12 Phahonyothin, Bangkok',  '2024-05-01');";
                cmd.ExecuteNonQuery();
            }
        }

        private static void InitializeOrdersDb()
        {
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId  INTEGER NOT NULL,
                    OrderDate   TEXT    NOT NULL,
                    TotalAmount REAL    NOT NULL DEFAULT 0,
                    Status      TEXT    NOT NULL DEFAULT 'Pending'
                );
                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId     INTEGER NOT NULL,
                    ProductId   INTEGER NOT NULL,
                    ProductName TEXT    NOT NULL,
                    Quantity    INTEGER NOT NULL,
                    UnitPrice   REAL    NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT COUNT(*) FROM Orders;";
            long count = (long)(cmd.ExecuteScalar() ?? 0);
            if (count == 0)
            {
                cmd.CommandText = @"
                    INSERT INTO Orders (CustomerId, OrderDate, TotalAmount, Status) VALUES
                    (1, '2024-06-01', 36880, 'Completed'),
                    (2, '2024-06-03',  1780, 'Completed'),
                    (3, '2024-06-10',  4990, 'Shipped'),
                    (4, '2024-06-15',  8889, 'Pending');
                    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice) VALUES
                    (1, 1, 'Laptop Pro 15',  1, 35990),
                    (1, 2, 'Wireless Mouse', 1,   890),
                    (2, 2, 'Wireless Mouse', 2,   890),
                    (3, 7, 'Office Chair',   1,  4990),
                    (4, 9, 'Monitor 24in',   1,  7990),
                    (4, 5, 'Ballpoint Pen',  4,    35),
                    (4, 4, 'Notebook A5',    3,   120);";
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Product> GetAllProducts()
        {
            var list = new List<Product>();
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products ORDER BY Category, Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadProduct(reader));
            return list;
        }

        public static List<Product> SearchProducts(string keyword)
        {
            var list = new List<Product>();
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products WHERE Name LIKE $kw OR Category LIKE $kw ORDER BY Name;";
            cmd.Parameters.AddWithValue("$kw", $"%{keyword}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadProduct(reader));
            return list;
        }

        public static void AddProduct(Product p)
        {
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (Name,Category,Price,Stock,Description)
                                VALUES ($n,$c,$pr,$s,$d);";
            cmd.Parameters.AddWithValue("$n",  p.Name);
            cmd.Parameters.AddWithValue("$c",  p.Category);
            cmd.Parameters.AddWithValue("$pr", p.Price);
            cmd.Parameters.AddWithValue("$s",  p.Stock);
            cmd.Parameters.AddWithValue("$d",  p.Description);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateProduct(Product p)
        {
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Products SET Name=$n,Category=$c,Price=$pr,Stock=$s,Description=$d
                                WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", p.Id);
            cmd.Parameters.AddWithValue("$n",  p.Name);
            cmd.Parameters.AddWithValue("$c",  p.Category);
            cmd.Parameters.AddWithValue("$pr", p.Price);
            cmd.Parameters.AddWithValue("$s",  p.Stock);
            cmd.Parameters.AddWithValue("$d",  p.Description);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteProduct(int id)
        {
            using var conn = new SqliteConnection(ProductsDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        private static Product ReadProduct(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(0),
            Name        = r.GetString(1),
            Category    = r.GetString(2),
            Price       = (decimal)r.GetDouble(3),
            Stock       = r.GetInt32(4),
            Description = r.IsDBNull(5) ? "" : r.GetString(5)
        };

        public static List<Customer> GetAllCustomers()
        {
            var list = new List<Customer>();
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Customers ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadCustomer(reader));
            return list;
        }

        public static List<Customer> SearchCustomers(string keyword)
        {
            var list = new List<Customer>();
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Customers WHERE Name LIKE $kw OR Email LIKE $kw OR Phone LIKE $kw;";
            cmd.Parameters.AddWithValue("$kw", $"%{keyword}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadCustomer(reader));
            return list;
        }

        public static void AddCustomer(Customer c)
        {
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Customers (Name,Email,Phone,Address,RegisteredDate)
                                VALUES ($n,$e,$ph,$a,$d);";
            cmd.Parameters.AddWithValue("$n",  c.Name);
            cmd.Parameters.AddWithValue("$e",  c.Email);
            cmd.Parameters.AddWithValue("$ph", c.Phone);
            cmd.Parameters.AddWithValue("$a",  c.Address);
            cmd.Parameters.AddWithValue("$d",  c.RegisteredDate.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();
        }

        public static void UpdateCustomer(Customer c)
        {
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Customers SET Name=$n,Email=$e,Phone=$ph,Address=$a WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", c.Id);
            cmd.Parameters.AddWithValue("$n",  c.Name);
            cmd.Parameters.AddWithValue("$e",  c.Email);
            cmd.Parameters.AddWithValue("$ph", c.Phone);
            cmd.Parameters.AddWithValue("$a",  c.Address);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteCustomer(int id)
        {
            using var conn = new SqliteConnection(CustomersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Customers WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        private static Customer ReadCustomer(SqliteDataReader r) => new()
        {
            Id             = r.GetInt32(0),
            Name           = r.GetString(1),
            Email          = r.GetString(2),
            Phone          = r.IsDBNull(3) ? "" : r.GetString(3),
            Address        = r.IsDBNull(4) ? "" : r.GetString(4),
            RegisteredDate = DateTime.Parse(r.GetString(5))
        };

        public static List<Order> GetAllOrders()
        {
            var customers = GetAllCustomers().ToDictionary(c => c.Id, c => c.Name);
            var list = new List<Order>();
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Orders ORDER BY OrderDate DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var order = ReadOrder(reader, customers);
                list.Add(order);
            }
            return list;
        }

        public static List<OrderItem> GetOrderItems(int orderId)
        {
            var list = new List<OrderItem>();
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM OrderItems WHERE OrderId=$id;";
            cmd.Parameters.AddWithValue("$id", orderId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new OrderItem
                {
                    Id          = reader.GetInt32(0),
                    OrderId     = reader.GetInt32(1),
                    ProductId   = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    Quantity    = reader.GetInt32(4),
                    UnitPrice   = (decimal)reader.GetDouble(5)
                });
            return list;
        }

        public static int AddOrder(Order order)
        {
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Orders (CustomerId,OrderDate,TotalAmount,Status)
                                VALUES ($cid,$d,$t,$s);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$cid", order.CustomerId);
            cmd.Parameters.AddWithValue("$d",   order.OrderDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$t",   order.TotalAmount);
            cmd.Parameters.AddWithValue("$s",   order.Status);
            int newId = Convert.ToInt32(cmd.ExecuteScalar());

            foreach (var item in order.Items)
            {
                var icmd = conn.CreateCommand();
                icmd.CommandText = @"INSERT INTO OrderItems (OrderId,ProductId,ProductName,Quantity,UnitPrice)
                                     VALUES ($oid,$pid,$pn,$q,$u);";
                icmd.Parameters.AddWithValue("$oid", newId);
                icmd.Parameters.AddWithValue("$pid", item.ProductId);
                icmd.Parameters.AddWithValue("$pn",  item.ProductName);
                icmd.Parameters.AddWithValue("$q",   item.Quantity);
                icmd.Parameters.AddWithValue("$u",   item.UnitPrice);
                icmd.ExecuteNonQuery();
            }
            return newId;
        }

        public static void UpdateOrderStatus(int orderId, string status)
        {
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Orders SET Status=$s WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", orderId);
            cmd.Parameters.AddWithValue("$s",  status);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteOrder(int orderId)
        {
            using var conn = new SqliteConnection(OrdersDb);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM OrderItems WHERE OrderId=$id; DELETE FROM Orders WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", orderId);
            cmd.ExecuteNonQuery();
        }

        private static Order ReadOrder(SqliteDataReader r, Dictionary<int,string> customers)
        {
            int cid = r.GetInt32(1);
            return new Order
            {
                Id           = r.GetInt32(0),
                CustomerId   = cid,
                CustomerName = customers.TryGetValue(cid, out var name) ? name : "Unknown",
                OrderDate    = DateTime.Parse(r.GetString(2)),
                TotalAmount  = (decimal)r.GetDouble(3),
                Status       = r.GetString(4)
            };
        }

        public static (int totalProducts, int lowStock, int totalCustomers, int totalOrders,
                        decimal totalRevenue, int pendingOrders) GetDashboardStats()
        {
            int totalProducts, lowStock;
            using (var conn = new SqliteConnection(ProductsDb))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*), SUM(CASE WHEN Stock < 10 THEN 1 ELSE 0 END) FROM Products;";
                using var r = cmd.ExecuteReader();
                r.Read();
                totalProducts = r.GetInt32(0);
                lowStock      = r.IsDBNull(1) ? 0 : r.GetInt32(1);
            }

            int totalCustomers;
            using (var conn = new SqliteConnection(CustomersDb))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Customers;";
                totalCustomers = Convert.ToInt32(cmd.ExecuteScalar());
            }

            int totalOrders; decimal totalRevenue; int pendingOrders;
            using (var conn = new SqliteConnection(OrdersDb))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*),
                                           COALESCE(SUM(TotalAmount),0),
                                           SUM(CASE WHEN Status='Pending' THEN 1 ELSE 0 END)
                                    FROM Orders;";
                using var r = cmd.ExecuteReader();
                r.Read();
                totalOrders   = r.GetInt32(0);
                totalRevenue  = (decimal)r.GetDouble(1);
                pendingOrders = r.IsDBNull(2) ? 0 : r.GetInt32(2);
            }

            return (totalProducts, lowStock, totalCustomers, totalOrders, totalRevenue, pendingOrders);
        }
    }
}
