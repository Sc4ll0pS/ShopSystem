using System.Windows;
using ShopSystem.Database;

namespace ShopSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseManager.InitializeAllDatabases();
        }
    }
}
