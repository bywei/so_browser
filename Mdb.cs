using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data;

namespace 浏览器
{
    class Mdb
    {
        public static readonly string conString =string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\db1.mdb", Application.StartupPath);
        public static int ExeOleDbNonQuery(string OleDb)
        {
            OleDbConnection con = new OleDbConnection(conString);
            con.Open();
            int result = 0;
            try
            {
                OleDbCommand cmd = new OleDbCommand(OleDb, con);
                result = cmd.ExecuteNonQuery();
            }
            catch { }
            con.Close();
            return result;
        }
        public static object ExeOleDbScalar(string OleDb)
        {
            OleDbConnection con = new OleDbConnection(conString);
            con.Open();
            object result = null;
            try
            {
                OleDbCommand cmd = new OleDbCommand(OleDb, con);
                result = cmd.ExecuteScalar();
            }
            catch { }
            con.Close();
            return result;
        }
        public static object[] ExeOleDbReader(string OleDb)
        {
            object[] results = new object[2];
            OleDbConnection con = new OleDbConnection(conString);
            con.Open();
            OleDbCommand cmd = new OleDbCommand(OleDb, con);
            return new object[] { cmd.ExecuteReader(), con };
        }

        public static bool SetOther(string name, string value)
        {
            string OleDb = string.Format("update others set [value]='{1}' where name='{0}'", name, value);
            return ExeOleDbNonQuery(OleDb) > 0;
        }
        public static string GetOther(string name)
        {
            string OleDb = string.Format("select [value] from others where name='{0}'", name);
            object result = ExeOleDbScalar(OleDb);
            return result == null ? "" : (string)result;
        }

        public static List<object[]> GetSearches()
        {
            string sql = string.Format("select [title],[image],[search] from searches");
            object[] drcon = ExeOleDbReader(sql);
            OleDbDataReader dr = (OleDbDataReader)drcon[0];
            List<object[]> results = new List<object[]>();
            while (dr.Read())
                results.Add(new object[] { dr[0], dr[1], dr[2] });
            dr.Close();
            ((OleDbConnection)drcon[1]).Close();
            return results;
        }

        public static List<FavForlder> GetFavForlders()
        {
            string sql = string.Format("select id,forlder_name from fav_forlders");
            object[] drcon = ExeOleDbReader(sql);
            OleDbDataReader dr = (OleDbDataReader) drcon[0];
            List<FavForlder> forlders = new List<FavForlder>();
            while (dr.Read())
                forlders.Add(new FavForlder((int)dr[0], (string)dr[1]));
            dr.Close();
            ((OleDbConnection)drcon[1]).Close();
            return forlders;
        }

        public static List<FavSiteurl> GetFavSiteurls(int forlder_id)
        {
            string sql = string.Format("select id,forlder_id,siteurl_name,siteurl_value from fav_siteurls where forlder_id={0}", forlder_id);
            object[] drcon = ExeOleDbReader(sql);
            OleDbDataReader dr = (OleDbDataReader)drcon[0];
            List<FavSiteurl> siteurls = new List<FavSiteurl>();
            while (dr.Read())
                siteurls.Add(new FavSiteurl((int)dr[0], (int)dr[1],(string)dr[2],(string)dr[3]));
            dr.Close();
            ((OleDbConnection)drcon[1]).Close();
            return siteurls;
        }

        public static bool DeleteSiteurl(int id)
        {
            string sql = string.Format("delete from fav_siteurls where id={0}", id);
            return ExeOleDbNonQuery(sql) != 0;
        }

        public static FavSiteurl AddSiteurl(int forlder_id, string siteurl_name, string siteurl_value)
        {
            string sql = string.Format("insert into fav_siteurls(forlder_id,siteurl_name,siteurl_value) values({0},'{1}','{2}')",forlder_id, siteurl_name, siteurl_value);
            ExeOleDbNonQuery(sql);
            sql = string.Format("select id from fav_siteurls where forlder_id={0} and siteurl_name='{1}' and siteurl_value='{2}' order by id desc", forlder_id, siteurl_name, siteurl_value);
            object id = ExeOleDbScalar(sql);
            return new FavSiteurl((int)id, forlder_id, siteurl_name, siteurl_value);
        }

        public static FavSiteurl UpdateSiteurl(int id, int forlder_id, string siteurl_name, string siteurl_value)
        {
            string sql = string.Format("update fav_siteurls set siteurl_name='{1}',siteurl_value='{2}' where id={0}", id, siteurl_name, siteurl_value);
            ExeOleDbNonQuery(sql);
            return new FavSiteurl((int)id, forlder_id, siteurl_name, siteurl_value);
        }

        public static bool DeleteForlder(int id)
        {
            string sql = string.Format("delete from fav_siteurls where forlder_id={0}", id);
            ExeOleDbNonQuery(sql);
            sql = string.Format("delete from fav_forlders where id={0}", id);
            return ExeOleDbNonQuery(sql) != 0;
        }

        public static FavForlder UpdateForlder(int id, string forlder_name)
        {
            string sql = string.Format("update fav_forlders set forlder_name='{1}' where id={0}", id, forlder_name);
            ExeOleDbNonQuery(sql);
            return new FavForlder(id, forlder_name);
        }

        public static FavForlder AddForlder(string forlder_name)
        {
            string sql = string.Format("insert into fav_forlders(forlder_name) values('{0}')", forlder_name);
            ExeOleDbNonQuery(sql);
            sql = string.Format("select id from fav_forlders where forlder_name='{0}' order by id desc", forlder_name);
            object id = ExeOleDbScalar(sql);
            return new FavForlder((int)id, forlder_name);
        }
    }
}
