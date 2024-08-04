using ISBN.Models;
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
using Tesseract;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Drawing;

namespace ISBN.View
{
    public partial class Camera : System.Windows.Controls.Page
    {
        public VideoCapture video;
        public Mat frame;
        public string imagefile;

        public Camera()
        {
            InitializeComponent();
            StartCamera();
        }

        public void StartCamera()
        {
            Task.Run(async () =>
            {
                video = new VideoCapture(0);

                while (true)
                {
                    frame = new Mat();
                    video.Read(frame);

                    if (frame.Empty())
                        break;

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        cam_img.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
                    }));

                    if (Cv2.WaitKey(33) > 0)
                        break;
                }
            });
        }

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                imagefile = @"C:\Users\uy122\OneDrive\바탕 화면\MAT0704\ISBN\Resources\ISBN_CODE.png";

                Cv2.ImWrite(imagefile, frame);

                frame.Dispose();
                video.Release();

                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    cam_img.Source = new BitmapImage(new Uri(imagefile, UriKind.Absolute));
                })); ;

                var engine = new TesseractEngine(@"C:\Users\uy122\OneDrive\바탕 화면\MAT0704\ISBN\bin\Debug\tessdata", "eng", EngineMode.TesseractAndLstm);
                var image = Pix.LoadFromFile(imagefile);
                var page = engine.Process(image);
                string code = page.GetText();
                code = ISBN_CODE(code);

                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (code == null)
                    {
                        MessageBox.Show("ISBN 코드를 읽지 못하였습니다. 다시 조회 부탁드립니다.");
                        StartCamera();
                    }
                    else
                    {
                        if (MessageBox.Show("ISBN 코드 : " + code + "\n조회하시겠습니까?", "ISBN코드 확인", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            tbx_isbncode.Text = code;
                            Main.data.Keyword = code;
                            Main.data.Type = (int)TYPE.DB_SAVE;
                        }
                        else
                        {
                            StartCamera();
                        }
                    }
                }));
            });
        }

        public string ISBN_CODE(string code)
        {
            string[] str;
            string isbn = null;

            if (code.Contains("ISBN") || code.Contains("SBN"))
            {
                str = code.Split("SBN");
                isbn = str[1];
                isbn = Regex.Replace(isbn, @"[^0-9]", "");

                return isbn;
            }
            return isbn;
        }

        private void btn_Info_Click(object sender, RoutedEventArgs e)
        {
            int result = Main.server.SearchBookInfo();
            if (result == (int)TYPE.SUCCEED)
            {
                Uri uri = new Uri("/View/ISBN_Search.xaml", UriKind.Relative);
                NavigationService.Navigate(uri);
            }
            else if (result == (int)TYPE.EMPTY)
            {
                MessageBox.Show("책 정보가 없습니다. 다시 조회 부탁드립니다.");
                StartCamera();
            }
            else if (result == (int)TYPE.FAIL)
            {
                MessageBox.Show("오류가 발생하였습니다. 죄송합니다.");
                StartCamera();
            }
        }
    }
}