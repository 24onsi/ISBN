using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Navigation;
using System.IO;
using ISBN.View;
using System.Diagnostics;
using System.Net;

namespace ISBN.Models
{
    public class Server
    {
        private TcpClient Client;
        private NetworkStream Stream;
        
        private string IP = "10.10.21.114";
        private int Port = 5001;

        public Server() { }

        private int ConnectServer()
        {
            try
            {
                Client = new TcpClient(IP, Port);
                Stream = Client.GetStream();    
            }
            catch(Exception ex)
            {
                return 0;
            }

            return 1;
        }

        private void DisconnectServer()
        {
            Stream.Close();
            Client.Close();
        }

        public int SearchBookInfo()
        {
            if (ConnectServer() == 0)
            {
                return 0;
            }

            Data Send = new Data()
            {
                Type = Main.data.Type,
                Keyword = Main.data.Keyword
            };

            string SendJson = JsonSerializer.Serialize(Send);
            byte[] SendByte = Encoding.UTF8.GetBytes(SendJson);
            Stream.Write(SendByte, 0, SendByte.Length);

            byte[] Buffer = new byte[8192];
            int ReadByte = Stream.Read(Buffer, 0, Buffer.Length);
            string ReadJson = Encoding.UTF8.GetString(Buffer, 0, ReadByte);

            try
            {
                Data? Result = JsonSerializer.Deserialize<Data>(ReadJson);

                if (Result.Type == (int)TYPE.SUCCEED)
                {
                    Main.data.BookInfoList = Result.BookInfoList;
                }

                DisconnectServer();

                return Result.Type;
            }
            catch (JsonException jsonEx)
            {
                DisconnectServer();

                Debug.WriteLine($"JSON Exception: {jsonEx.Message}");
                
                return 40;
            }
        }

        public BitmapImage LoadImage(string url)
        {
            string df = @"C:\Users\uy122\OneDrive\바탕 화면\MAT0704\ISBN\Properties\Image\no-photos.png";
            
            Byte[] MyData;
            BitmapImage bimgTemp;

            try
            {
                WebClient wc = new WebClient();

                if (string.IsNullOrEmpty(url))
                {
                    MyData = wc.DownloadData(df);
                    wc.Dispose();
                    bimgTemp = new BitmapImage();
                    bimgTemp.BeginInit();
                    bimgTemp.StreamSource = new MemoryStream(MyData);
                    bimgTemp.EndInit();
                    return bimgTemp;
                }
                else
                {
                    string newurl = "https://cover.nl.go.kr/" + url;
                    MyData = wc.DownloadData(newurl);
                    wc.Dispose();
                    bimgTemp = new BitmapImage();
                    bimgTemp.BeginInit();
                    bimgTemp.StreamSource = new MemoryStream(MyData);
                    bimgTemp.EndInit();
                    return bimgTemp;
                }
            }
            catch
            {
                return null;
            }
        }

        public int LoadSearchLog()
        {
            if (ConnectServer() == 0)
            {
                return 0;
            }

            Data Send = new Data()
            {
                Type = (int)TYPE.LOG
            };

            string SendJson = JsonSerializer.Serialize(Send);
            byte[] SendByte = Encoding.UTF8.GetBytes(SendJson);
            Stream.Write(SendByte, 0, SendByte.Length);

            byte[] Buffer = new byte[8192];
            int ReadByte = Stream.Read(Buffer, 0, Buffer.Length);
            string ReadJson = Encoding.UTF8.GetString(Buffer, 0, ReadByte);

            try
            {
                Data? Result = JsonSerializer.Deserialize<Data>(ReadJson);

                if (Result.Type == (int)TYPE.SUCCEED)
                {
                    Main.data.SearchLogList = Result.SearchLogList;
                }

                DisconnectServer();

                return Result.Type;
            }
            catch (JsonException jsonEx)
            {
                DisconnectServer();

                Debug.WriteLine($"JSON Exception: {jsonEx.Message}");

                return (int)TYPE.FAIL;
            }
        }



    }
}
