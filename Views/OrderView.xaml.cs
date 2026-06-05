using ShopSystem.ViewModels;
using System.Windows.Controls;

namespace ShopSystem.Views
{
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();

            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                    ((OrderViewModel)DataContext).LoadAll();
            };
        }
    }
}