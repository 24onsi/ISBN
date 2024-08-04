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
using static MaterialDesignThemes.Wpf.Theme;

namespace ISBN.View
{
    public partial class Book_Search : Page
    {
        List<BookInfo> book_info = new List<BookInfo>();

        public Book_Search()
        {
            InitializeComponent();
        }

        public void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Main.data.Keyword = tbx_search.Text.ToString();
                Main.data.Type = (int)TYPE.SEARCH;

                Search();
            }
        }

        public void Search()
        {
            int result;
            Task.Run(async () =>
            {
                book_info.Clear();
                Main.data.Type = (int)TYPE.SEARCH;
                result = Main.server.SearchBookInfo();

                if (result == (int)TYPE.SUCCEED)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var item in Main.data.BookInfoList)
                        {
                            BookInfo bookInfo = new BookInfo();
                            bookInfo.ISBN = item.ISBN;
                            bookInfo.Title = item.Title;
                            bookInfo.Author = item.Author;
                            bookInfo.Publisher = item.Publisher;
                            bookInfo.Place = item.Place;
                            bookInfo.Category = item.Category;

                            book_info.Add(bookInfo);
                        }
                        Search_view.ItemsSource = book_info;
                        Search_view.Items.Refresh();
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

        public void Search_view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Search_view.SelectedItems.Count == 1)
            {
                BookInfo selectedBook = Search_view.SelectedItem as BookInfo;

                if (selectedBook != null)
                {
                    int index = Main.data.BookInfoList.FindIndex(book => book.ISBN == selectedBook.ISBN);
                    if (index >= 0)
                    {
                        Main.data.BookInfoList[index] = selectedBook;
                    }
                  
                }
            }

            Main.data.Type = (int)TYPE.DB_SAVE;
            int result = Main.server.SearchBookInfo();
            if (result == (int)TYPE.SUCCEED)
            {
                if (MessageBox.Show("해당 도서 정보 페이지로 이동하시겠습니까?", "도서 정보 조회 페이지 이동", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Uri uri = new Uri("/View/ISBN_Search.xaml", UriKind.Relative);
                    NavigationService.Navigate(uri);
                }
            }
        }

        private void home_btn_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/View/Main.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}
