using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using Comm.Commons.Extensions;
namespace Comm.Commons
{
    public class Connection
    {
        SqlConnection _connection = null;
        public Connection()
        {
            try
            {
                //con = new SqlConnection(WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString);
            }
            catch (Exception er)
            {
                String dasds = er.Message.ToString();
            }
        }    

        public DataTable QueryBySQLCode(String SQL)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                this.OpenConn();
                Console.WriteLine(SQL);

                SqlDataAdapter da = new SqlDataAdapter(SQL, _connection);

                ds.Reset();

                da.Fill(ds);

                dt = ds.Tables[0];
            }
            catch (Exception er)
            {
                String dasd = er.Message.ToString();
                //MessageBox.Show(er.Message);
            }
            this.CloseConn();
            return dt;
        }




        public DataTableCollection QueryBySQLCodeMTbl(String SQL)
        {
            DataTableCollection dtc = null;
            DataSet ds = new DataSet();
            try
            {
                this.OpenConn();
                //Console.WriteLine(SQL);
                using (SqlDataAdapter da = new SqlDataAdapter($"{ SQL }", _connection))
                {
                    ds.Reset();
                    da.Fill(ds);

                    dtc =  ds.Tables;
                }
            }
            catch (Exception er)
            {
                String dasd = er.Message.ToString();
            }
            this.CloseConn();

            return dtc;
        }



        public void OpenConn()
        {
            try { _connection.Open(); }
            catch { }
        }

        public void CloseConn()
        {
            try { _connection.Close(); }
            catch  { }
        }
        public Boolean InsertSP(String value)
        {
            Boolean flag = false;
            try
            {
                this.OpenConn();

                string SQL = "EXEC sp_loans " + value + "";
                //MessageBox.Show(SQL);
                SqlCommand command = new SqlCommand(SQL, _connection);

                Int32 rowsaffected = command.ExecuteNonQuery();


                flag = true;
            }
            catch
            {
                flag = false;
                //MessageBox.Show(er.Message);
            }

            this.CloseConn();
            return flag;
        }
        public string GetLastInsert(String table, String column)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            String id = null;

            try
            {
                this.OpenConn();
                string SQL = "SELECT " + column + " FROM " + table + " ORDER BY " + column + " DESC";
                Console.WriteLine(SQL);
                SqlDataAdapter da = new SqlDataAdapter(SQL, _connection);

                ds.Reset();

                da.Fill(ds);

                dt = ds.Tables[0];

                id = dt.Rows[0][column].ToString();
            }
            catch { }
            
            this.CloseConn();
            return id;
        }

        public Boolean InsertOnTable(String table, String column, String value)
        {
            Boolean flag = false;

            try
            {
                this.OpenConn();
                string SQL = "INSERT INTO " + table + "(" + column + ") VALUES (" + value + ")";

                SqlCommand command = new SqlCommand(SQL, _connection);

                Int32 rowsaffected = command.ExecuteNonQuery();

                flag = true;
            }
            catch (Exception e) { }

            this.CloseConn();
            return flag;
        }
        public Boolean SPEXEC(String query)
        {
            Boolean flag = false;
            
            try
            {
                this.OpenConn();

                string SQL = query;
                //MessageBox.Show(SQL);
                SqlCommand command = new SqlCommand(SQL, _connection);

                Int32 rowsaffected = command.ExecuteNonQuery();


                flag = true;
            }
            catch { }

            this.CloseConn();

            return flag;
        }
        public Boolean InsertOnTableImage(String table, String column, byte[] value)
        {
            Boolean flag = false;

            try
            {
                this.OpenConn();

                string SQL = "INSERT INTO " + table + "(" + column + ") VALUES (" + value + ")";
                //MessageBox.Show(SQL);
                SqlCommand command = new SqlCommand(SQL, _connection);

                Int32 rowsaffected = command.ExecuteNonQuery();
                
                flag = true;
            }
            catch { }


            this.CloseConn();

            return flag;
        }
        public Boolean UpdateOnTable(String table, String col_upd, String cond)
        {
            Boolean flag = false;
            try
            {
                this.OpenConn();
                if (!cond.IsEmpty()) cond = $" WHERE { cond } ";

                string SQL = $"UPDATE { table } SET { col_upd } { cond };";

                SqlCommand command = new SqlCommand(SQL, _connection);
                Int32 rowsaffected = command.ExecuteNonQuery();

                flag = true;
            }
            catch { }
            this.CloseConn();

            return flag;
        }
        public DataTable QueryOnTableWithParams(string table, String param, String cond, String addcode)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                this.OpenConn();
                if (!cond.IsEmpty()) cond = $" WHERE { cond } ";

                string SQL = $"SELECT { param } FROM { table } { cond } { addcode };";
                //MessageBox.Show(SQL);
                //Console.WriteLine(SQL);
                SqlDataAdapter da = new SqlDataAdapter(SQL, _connection);

                ds.Reset();
                da.Fill(ds);

                dt = ds.Tables[0];
            }
            catch { }

            this.CloseConn();

            return dt;
        }

        public String get_colval(String table, String col, String cond)
        {
            String pk = "";
            DataTable dt = this.QueryOnTableWithParams(table, $"TOP(1) { col } ", cond, ""); //$"ORDER BY { col } ASC"
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    DataColumn column = dt.Columns[0];
                    pk = dt.Rows[0][column.ColumnName].ToString();
                }
            }
            return pk;
        }
        public Boolean DeleteOnTable(String table, String cond)
        {
            Boolean flag = false;

            try
            {
                this.OpenConn();
                string SQL = "DELETE FROM " + table + " WHERE " + cond + ";";
                //MessageBox.Show(SQL);
                SqlCommand command = new SqlCommand(SQL, _connection);

                Int32 rowsaffected = command.ExecuteNonQuery();
                
                flag = true;
            }
            catch { }

            this.CloseConn();

            return flag;
        }


        // Custom
        public Dictionary<string, string> Entry()
        {
            return new Dictionary<string, string>();
        }

        public bool UpdateEntry(String table, Dictionary<string, string> entries)
        {
            bool flag = false;
            int entries_cnt = entries.Count;
            String whereBase = "W->";
            String insertValueData = "", whereValueData = "";
            foreach (String key in entries.Keys)
            {
                if (key.IndexOf(whereBase) > -1)
                {
                    String key_ = key.Replace(whereBase, "");
                    whereValueData += (key_ + "=@W" + key_) + " AND ";
                }
                else insertValueData += (key + "=@U" + key) + ",";
            }
            if (!String.IsNullOrEmpty(insertValueData) && !String.IsNullOrEmpty(whereValueData))
            {
                insertValueData = insertValueData.Remove(insertValueData.Length - 1);
                whereValueData = whereValueData.Remove(whereValueData.Length - 5);

                try
                {
                    this.OpenConn();
                    using (SqlCommand command = new SqlCommand("UPDATE " + table + " SET " + insertValueData + " WHERE " + whereValueData + ";", _connection))
                    {
                        foreach (var entry in entries)
                        {
                            String key = entry.Key;
                            if (key.IndexOf(whereBase) > -1) key = "W" + key.Replace(whereBase, "");
                            else key = "U" + key;

                            command.Parameters.AddWithValue("@" + key, (entry.Value ?? ""));
                        }
                        Int32 rowsaffected = command.ExecuteNonQuery();
                    }
                    flag = true;
                }
                catch (Exception e){ }
                this.CloseConn();
            }
            return flag;
        }
        public bool InsertEntry(String table, Dictionary<string, string> entries)
        {
            bool flag = false;
            int entries_cnt = entries.Count;
            String insertValue = "", insertData = "";
            foreach (String key in entries.Keys)
            {
                insertValue += key; insertData += "@" + key;
                if (1 < entries_cnt)
                {
                    insertValue += ",";
                    insertData += ",";
                }
                entries_cnt--;
            }
            if (!String.IsNullOrEmpty(insertValue))
            {
                try
                {
                    this.OpenConn();
                    using (SqlCommand command = new SqlCommand("INSERT INTO " + table + "(" + insertValue + ") VALUES (" + insertData + ")", _connection))
                    {
                        foreach (var entry in entries)
                        {
                            command.Parameters.AddWithValue("@" + entry.Key, (entry.Value ?? ""));
                        }
                        Int32 rowsaffected = command.ExecuteNonQuery();
                    }
                    flag = true;
                }
                catch (Exception _e){ }
                this.CloseConn();
            }
            return flag;
        }

        public String Count(String table, String cond = "")
        {
            if (!String.IsNullOrEmpty(cond)) cond += " WHERE " + cond;
            DataTable dt = this.QueryBySQLCode("SELECT COUNT(1) as count FROM " + table + " " + cond);
            if (dt != null && dt.Rows.Count == 1)
            {
                return dt.Rows[0]["count"].ToString();
            }
            return "0";
        }
        public Dictionary<string, string> Get_ColVal(String table, String columns, String Where = "")
        {
            DataTable dt = this.QueryOnTableWithParams(table, columns, Where, "");
            if (dt != null)
            {
                var entries = Entry();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        entries[column.ColumnName] = dt.Rows[0][column.ColumnName].ToString();
                    }
                }
                return entries;
                /*
                var entries = Entry();
                if (dt.Rows.Count > 0)
                {
                    String col = ""; String[] cols = columns.Split(',');
                    for (int j = 0; j < cols.Length; j++)
                    {
                        col = cols[j].Trim().Replace(" ", "");
                        entries[col] = dt.Rows[0][col].ToString();
                    }
                }*/
            }
            return null;
        }
        /*public Dictionary<string, object> Get_SingleRow(String sql, object dic = null)
        {
            DataTable dt = (dic == null ? this.QueryBySQLCode(sql): this.QueryBySQLCode(sql, dic));
            if (dt != null)
            {
                var entries = new Dictionary<string, object>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        entries[column.ColumnName] = dt.Rows[0][column.ColumnName].ToString();
                    }
                }
                else
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        entries[column.ColumnName] = column.DefaultValue;
                    }
                }
                return entries;
            }
            return null;
        }*/

        public string IDENT_CURRENT(String tableName)
        {
            String str = "", cnt = "";
            DataTable dt = this.QueryBySQLCode("SELECT IDENT_CURRENT('" + tableName + "') AS ID, (SELECT Count(1) as cnt FROM " + tableName + ") AS CNT");
            if (dt != null)
            {
                if (dt.Rows.Count == 1)
                {
                    cnt = dt.Rows[0]["CNT"].ToString();
                    if (cnt == "0") str = "0";
                    else str = dt.Rows[0]["ID"].ToString();
                }
            }
            return str;
        }

/*
        public DataTable QueryBySQLCode(String SQL, object dic)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                this.OpenConn();

                Console.WriteLine(SQL);

                using (SqlDataAdapter da = new SqlDataAdapter(SQL, _connection))
                {
                    var entries = dic.ToDictionary();
                    foreach (var entry in entries)
                    {
                        da.SelectCommand.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                    }
                    ds.Reset();
                    da.Fill(ds);
                }

                dt= ds.Tables[0];
            }
            catch { }
            this.CloseConn();

            return dt;
        }
*/
        private List<string> listStr = new List<string>();
        public List<string> GetNames(SqlDataReader reader)
        {
            listStr.Clear();
            for (int i = 0; i < reader.FieldCount; i++)
                listStr.Add(reader.GetName(i));
            return listStr;
        }
/*
        public List<Dictionary<string, object>> _QueryBySQLCode(String SQL, object dic, Action<int, Dictionary<string, object>> action = null)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            try
            {
                this.OpenConn();

                using (SqlCommand command = new SqlCommand(SQL, con))
                {
                    if (dic != null)
                    {
                        var entries = dic.ToDictionary();
                        foreach (var entry in entries) command.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                    }
                    SqlDataReader retrieve = command.ExecuteReader();

                    while (retrieve.Read()) list.Add(retrieve.ToDictionary());

                    retrieve = null;

                    if (action != null) for (int i = 0; i < list.Count; action(i, list[i]), i++) ;

                }
            }
            catch { }
            this.CloseConn();

            return list;
        }


        public List<Dictionary<string, object>> _QueryBySQLCode(String SQL, Action<int, Dictionary<string, object>> action = null)
        {
            return _QueryBySQLCode(SQL, null, action);
        }*/
    

        // use id only
        public string In(string [] arr)
        {
            string strin = "";
            for (int i = 0; i < arr.Length; i++)
                strin += $"'{arr[i].TDoQuotes()}',";
            if (!strin.IsEmpty()) strin = strin.Substring(0, strin.Length - 1);
            else strin = "''";
            return $"({strin})";
        }

/*

        public Boolean DeleteOnTable(String table, String cond, object dic)
        {
            Boolean flag = false;
            try
            {
                this.OpenConn();
                using (SqlCommand command = new SqlCommand("DELETE FROM " + table + " WHERE " + cond + ";", con))
                {
                    var entries = dic.ToDictionary();
                    foreach (var entry in entries)
                    {
                        command.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                    }
                    Int32 rowsaffected = command.ExecuteNonQuery();
                }
                flag = true;
            }
            catch { }
            this.CloseConn();
            return flag;
        }
*/
        /// Extra Methods
        /// 
/*
        public Boolean stored_procedure(String sp_string)
        {
            Boolean flag = false;
            try
            {
                this.OpenConn();

                string SQL = sp_string;
                //MessageBox.Show(SQL);
                SqlCommand command = new SqlCommand(SQL, con);

                Int32 rowsaffected = command.ExecuteNonQuery();

                flag = true;
            }
            catch  { }
            this.CloseConn();

            return flag;
        }
*/
    }

}