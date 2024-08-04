using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using ISBN.View;


namespace ISBN.Models
{

    
    public class Data
    {
        public int Type { get; set; }
        public string Keyword { get; set; }
        public List<BookInfo> BookInfoList { get; set; }
        public List<BookInfo> SearchLogList { get; set; }
    }

    public class BookInfo
    {
        public string NO { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string ISBN { get; set; }
        public string Place { get; set; }
        public string Image { get; set; }
        public string Category { get; set; }
    }


    public enum TYPE
    {

        // 0번
        CONNECT_FAIL = 0,

        // 10번
        SEARCH = 10,

        // 20번
        LOG = 20,

        // 25번
        DB_SAVE = 25,

        // 30번
        SUCCEED = 30,

        // 40번
        FAIL = 40,

        // 50번
        EMPTY = 50,

    }
}
