#ifndef DATA_H
#define DATA_H
#include <string>

struct Info
{

int Type;
std::string Keyword;


std::string NO;
std::string Title;
std::string Author;
std::string Publisher;
std::string Category;
std::string Place;
std::string Image;
std::string ISBN;

};

enum TYPE
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

};

#endif