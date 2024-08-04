using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ISBN.Models;


namespace ISBN.View
{
    public partial class Main : Page
    {
        public static Data data = new Data();
        public static Server server = new Server();

        public Main()
        {
            InitializeComponent();
        }

        private void Btn_ISBN_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Camera.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void Btn_Book_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Book_Search.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void Btn_Log_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Book_Log.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}
