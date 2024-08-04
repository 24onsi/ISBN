using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class Book_Log : Page
    {
        List<BookInfo> book_log = new List<BookInfo>();
        List<BookInfo> select_log = new List<BookInfo>();

        public Book_Log()
        {
            InitializeComponent();
            ShowSearchLog();
        }

        public void ShowSearchLog()
        {
            int result;
            Task.Run(async () =>
            {
                book_log.Clear();
                result = Main.server.LoadSearchLog();

                if (result == (int)TYPE.SUCCEED)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var item in Main.data.SearchLogList)
                        {
                            BookInfo bookInfo = new BookInfo();
                            bookInfo.NO = item.NO;
                            bookInfo.ISBN = item.ISBN;
                            bookInfo.Title = item.Title;
                            bookInfo.Author = item.Author;
                            bookInfo.Publisher = item.Publisher;
                            bookInfo.Place = item.Place;
                            bookInfo.Category = item.Category;

                            book_log.Add(bookInfo);
                        }
                        list_Log.ItemsSource = book_log;
                        list_Log.Items.Refresh();
                    }));
                }
                else if (result == (int)TYPE.EMPTY)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("검색 기록이 없습니다.\n메인화면으로 이동합니다.");
                        Uri uri = new Uri("/View/Main.xaml", UriKind.Relative);
                        NavigationService.Navigate(uri);
                    }));
                }
                else if (result == (int)TYPE.FAIL)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("검색 기록 불러오기 실패하였습니다.\n메인화면으로 이동합니다.");
                        Uri uri = new Uri("/View/Main.xaml", UriKind.Relative);
                        NavigationService.Navigate(uri);
                    }));
                }
            });
        }
        private void home_btn_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Main.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        public void SelectSeachLog(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (list_Log.SelectedItem is BookInfo Log)
            {
                foreach (var item in Main.data.SearchLogList)
                {
                    if (Log.NO == item.NO)
                    {
                        select_log.Add(item);
                        Main.data.BookInfoList = select_log;

                        break;
                    }
                }
                if (MessageBox.Show($"\"{Log.Title}\" 도서 정보 페이지로 이동하시겠습니까?", "도서 정보 조회 페이지 이동", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Uri uri = new Uri("/View/ISBN_Search.xaml", UriKind.Relative);
                    NavigationService.Navigate(uri);
                }
            }
        }
    }
}




