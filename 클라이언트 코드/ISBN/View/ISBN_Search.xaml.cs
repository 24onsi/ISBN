using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public partial class ISBN_Search : Page
    {
        public ISBN_Search()
        {
            InitializeComponent();
            InitBookInfo();
        }

        public void InitBookInfo()
        {
            Task.Run(async () =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {

                    tbx_title.Text = Main.data.BookInfoList[0].Title;

                    tbx_author.Text = Main.data.BookInfoList[0].Author;

                    tbx_publisher.Text = Main.data.BookInfoList[0].Publisher;

                    tbx_category.Text = Main.data.BookInfoList[0].Category;

                    tbx_place.Text = Main.data.BookInfoList[0].Place;
    
                    img_title.Source = Main.server.LoadImage(Main.data.BookInfoList[0].Image);

                }));
            });
        }

        private void back_btn_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Main.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

    }
}
