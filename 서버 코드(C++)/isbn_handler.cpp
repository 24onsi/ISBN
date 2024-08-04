#include "isbn_handler.h"


DB::DB() { }

DB::~DB() { }

sql::Connection* DB::ConnectDB()
{
    try
    {
        sql::Driver* driver = sql::mariadb::get_driver_instance();
        sql::SQLString url = "jdbc:mariadb://10.10.21.114:3306/ISBN";
        sql::Properties properties({{"user", "OPERATOR"}, {"password", "1234"}});
        std::cout << "DB 접속 성공" << std::endl;

        return driver->connect(url, properties);   
     }
    catch(sql::SQLException& e)
    {
        std::cerr << "DB 접속 실패: " << e.what() << std::endl;
        exit(1);
    }
}

void DB::DisconnectDB(sql::Connection* conn)
{
    if (!conn->isClosed())
    {
        conn->close();
        std::cout << "DB 접속 해제" << std::endl;
    }
}

Handler::Handler(int sock)
{
    int iSock = sock;
}

Handler::~Handler() { }

void Handler::API(Info &info, int sock)
{
    CURL *curl;
    CURLcode res;

    std::string sendData;
    std::string chunk_;

    json js;
    int bytesSent = 0;

    curl_global_init(CURL_GLOBAL_ALL); // libcurl 초기화

    curl = curl_easy_init();
    if (curl)
    {
        std::string url = "https://www.nl.go.kr/NL/search/openApi/search.do?key=6f5d9d613276488a053d079fc4358702656ea71085f9f3259f0a99e10196bd3e";
        url += "&apiType=json";
        url += "&detailSearch=true";
        url += "&pageSize=10";
        url += "&pageNum=1";
        url += "&srchTarget=total";
        url += "&kwd=" + url_encode(info.Keyword);

        std::cout << info.Keyword << std::endl;

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str()); // std::string을 C 스타일 문자열로 변환하여 사용
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_callback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &chunk_);

        res = curl_easy_perform(curl); // HTTP 요청 실행
        if (res != CURLE_OK)
        {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        }

        curl_easy_cleanup(curl);
    }

    curl_global_cleanup();

    try
    {
        // 요청 성공 시 JSON 파싱
        json result = json::parse(chunk_);

        if (result.contains("result"))
        {
            js = {
                {"Type", SUCCEED},
                {"BookInfoList", json::array()}};

            auto items = result["result"];
            // 필요한 정보 추출
            for (const auto &item : items)
            {
                info.Title = item.value("titleInfo", "");
                info.Author = item.value("authorInfo", "");
                info.Publisher = item.value("pubInfo", "");
                info.ISBN = item.value("isbn", "");
                info.Place = item.value("placeInfo", "");
                info.Image = item.value("imageUrl", "");
                info.Category = item.value("kdcName1s", "");

                // HTML 태그 제거
                std::string title_ = RemoveTag(info.Title);
                std::string author_ = RemoveTag(info.Author);
                info.Title = title_;
                info.Author = author_;

                js["BookInfoList"].push_back({{"Title", info.Title},
                        {"Author", info.Author},
                        {"Publisher", info.Publisher},
                        {"ISBN", info.ISBN},
                        {"Place", info.Place},
                        {"Image", info.Image},
                        {"Category", info.Category}});

            }

                sendData = js.dump();
                std::cout << sendData << std::endl;

                bytesSent = write(sock, sendData.c_str(), sendData.length());
                std::cout << bytesSent << std::endl;

                if(info.Type == DB_SAVE)
                {    
                    InsertDB_BookInfo(info);
                }

                return;
        }
        else
        {
            std::cout << "EMPTY" << std::endl;

            js = json{{"Type", EMPTY}};
            sendData = js.dump();
            bytesSent = write(sock, sendData.c_str(), sendData.length());

            std::cout << bytesSent << std::endl;

            return;
        }
    }
    catch (json::parse_error &e)
    {
        std::cerr << "JSON 파싱 에러 : FAIL" << e.what();

        js = json{{"Type", FAIL}};
        sendData = js.dump();
        bytesSent = write(sock, sendData.c_str(), sendData.length());

        std::cout << bytesSent << std::endl;

    }
    catch (json::type_error &e)
    {
        std::cerr << "JSON 타입 에러 : FAIL" << e.what();
        
        js = json{{"Type", FAIL}};
        sendData = js.dump();
        bytesSent = write(sock, sendData.c_str(), sendData.length());

        std::cout << bytesSent << std::endl;
    }
}

size_t Handler::write_callback(char *ptr, size_t size, size_t nmemb, std::string *data)
{
    data->append(ptr, size * nmemb);
    return size * nmemb;
}

std::string Handler::url_encode(const std::string &value)
{
    std::ostringstream escaped;
    escaped << std::hex << std::uppercase;
    for (char c : value)
    {
        if (isalnum(c) || c == '-' || c == '_' || c == '.' || c == '~')
        {
            escaped << c;
        }
        else
        {
            escaped << '%' << std::setw(2) << int((unsigned char)c);
        }
    }
    return escaped.str();
}

std::string Handler::RemoveTag(const std::string &html)
{
    std::regex regex("<.*?>");
    return std::regex_replace(html, regex, "");
}


void Handler::InsertDB_BookInfo(const Info & info)
{
    try
    {
        DB db;
        sql::Connection*con = db.ConnectDB();
        sql::PreparedStatement*BookInfo 
        = con->prepareStatement("INSERT INTO BOOK_LOG VALUES (DEFAULT, ?, ?, ?, ?, ?, ?, ?)");

        BookInfo->setString(1, info.ISBN);
        BookInfo->setString(2, info.Title);
        BookInfo->setString(3, info.Image);
        BookInfo->setString(4, info.Author);
        BookInfo->setString(5, info.Publisher);
        BookInfo->setString(6, info.Category);
        BookInfo->setString(7, info.Place);

        BookInfo->executeQuery();

        std::cout << "DB 기록 저장" << std::endl;

        sql::PreparedStatement*clearNO1 = con->prepareStatement("ALTER TABLE BOOK_LOG AUTO_INCREMENT=1");
        clearNO1->executeQuery();
        sql::PreparedStatement*clearNO2 = con->prepareStatement("SET @COUNT = 0");
        clearNO2->executeQuery();
        sql::PreparedStatement*clearNO3 = con->prepareStatement("UPDATE BOOK_LOG SET NO = @COUNT:=@COUNT+1");
        clearNO3->executeQuery();
    
        db.DisconnectDB(con);
    }
    catch(const sql::SQLException& e)
    {
        std::cerr << "DB 기록 저장 실패 : " << e.what() << std::endl;
    }
}

void Handler::Search_Log(int sock)
{
    Info sendInfo;
    json js;
    std::string sendData;
    int bytesSent = 0;

    try
    {
        DB db;
        sql::Connection*con = db.ConnectDB();
        sql::PreparedStatement*BookInfo = con->prepareStatement("SELECT * FROM BOOK_LOG ORDER BY NO DESC LIMIT 10");
        sql::ResultSet*Log = BookInfo->executeQuery();

        // 결과 값이 1줄이라도 있다면
        if(Log->rowsCount())
        {
            std::cout << "SUCCEED" << std::endl;

            js = {
                {"Type", SUCCEED},
                {"SearchLogList", json::array()}
            };

            while (Log->next())
            {
                sendInfo.NO = std::to_string(Log->getInt(1));
                sendInfo.ISBN = Log->getString(2);
                sendInfo.Title = Log->getString(3);
                sendInfo.Image = Log->getString(4);
                sendInfo.Author = Log->getString(5);
                sendInfo.Publisher = Log->getString(6);
                sendInfo.Category = Log->getString(7);
                sendInfo.Place = Log->getString(8);
                
                js["SearchLogList"].push_back({
                                            {"NO", sendInfo.NO},
                                            {"ISBN", sendInfo.ISBN},
                                            {"Title", sendInfo.Title},
                                            {"Image", sendInfo.Image},
                                            {"Author", sendInfo.Author},
                                            {"Publisher", sendInfo.Publisher},
                                            {"Category", sendInfo.Category},
                                            {"Place", sendInfo.Place}
                });
            }

            sendData = js.dump();
            bytesSent = write(sock, sendData.c_str(), sendData.length());

            std::cout << sendData << std::endl;
            std::cout << bytesSent << std::endl;

            db.DisconnectDB(con);

        }
        else
        {
            std::cout << "EMPTY" << std::endl;

            js = {"Type", EMPTY};
            sendData = js.dump();
            bytesSent = write(sock, sendData.c_str(), sendData.length());

            std::cout << bytesSent << std::endl;
        }
    }
    catch(const sql::SQLException& e)
    {
        std::cout << "FAIL" << std::endl;

        js = {"Type", FAIL};
        sendData = js.dump();
        bytesSent = write(sock, sendData.c_str(), sendData.length());

        std::cout << bytesSent << std::endl;
    }

    
}