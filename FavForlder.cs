using System;
using System.Collections.Generic;
using System.Text;

namespace 浏览器
{
    public class FavForlder
    {
        public int id;
        public string forlder_name;
        public object tag;
        public FavForlder(int id_, string forlder_name_)
        {
            id = id_;
            forlder_name = forlder_name_;
        }
    }
    public class FavSiteurl
    {
        public int id;
        public int forlder_id;
        public string siteurl_name;
        public string siteurl_value;
        public object tag;
        public FavSiteurl(int id_,int forlder_id_, string siteurl_name_, string siteurl_value_)
        {
            id = id_;
            forlder_id = forlder_id_;
            siteurl_name = siteurl_name_;
            siteurl_value = siteurl_value_;
        }
    }
}
