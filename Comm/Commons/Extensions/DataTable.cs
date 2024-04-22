using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Comm.Commons.Extensions
{
    public static class DataTableExt
    {
        public static Dictionary<string, string> ToDictionary(this DataTable dt)
        {
            var dic = new Dictionary<string, string>();
            if (dt != null && dt.Rows.Count != 0)
            {
                DataColumnCollection columns = dt.Columns;
                bool hasName = (columns.Contains("Value") && columns.Contains("Key"));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (hasName) dic[dt.Rows[i]["Key"].ToString()] = dt.Rows[i]["Value"].ToString();
                    else dic[dt.Rows[i][0].ToString()] = dt.Rows[i][1].ToString();
                }
            }
            return dic;
        }

        /*public static dynamic ToDynamic(this DataRow row)
        {
            dynamic dyn = Dynamic.Object;
            var dic = (IDictionary<string, object>)dyn;
            foreach (DataColumn column in row.Table.Columns) dic[column.ColumnName] = row[column];
            return dyn;
        }*/

        public static Dictionary<string, object> ToDictionary(this DataRow row)
        {
            var dic = new Dictionary<string, object>();
            foreach (DataColumn column in row.Table.Columns) dic[column.ColumnName] = (object)row[column];
            return dic;
        }



        public static DataRow FirstRow(this DataTable dt)
        {
            return (dt.Rows.Count == 0 ? dt.NewRow() : dt.Rows[0]);
        }


        public static T ConvertAs<T>(this DataRow dataRow) where T : new()
        {
            T item = new T();
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                if (dataRow[column] != DBNull.Value)
                {
                    PropertyInfo prop = item.GetType().GetProperty(column.ColumnName);
                    if (prop != null)
                    {
                        object result = Convert.ChangeType(dataRow[column], prop.PropertyType);
                        prop.SetValue(item, result, null);
                        continue;
                    }
                    else
                    {
                        FieldInfo fld = item.GetType().GetField(column.ColumnName);
                        if (fld != null)
                        {
                            object result = Convert.ChangeType(dataRow[column], fld.FieldType);
                            fld.SetValue(item, result);
                        }
                    }
                }
            }
            return item;
        }



        /*

            public static Dictionary<string, string> ToDictionary(this DataTable dt)
            {
                var dic = new Dictionary<string, string>();
                if (dt != null && dt.Rows.Count != 0)
                {
                    DataColumnCollection columns = dt.Columns;
                    bool hasName = columns.Contains("Value") && columns.Contains("Key");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (hasName) dic[dt.Rows[i]["Key"].ToString()] = dt.Rows[i]["Value"].ToString();
                        else dic[dt.Rows[i][0].ToString()] = dt.Rows[i][1].ToString();
                    }
                }
                return dic;
            }




        foreach(DataRow row in dt.Rows)
        {
            TextBox1.Text = row["ImagePath"].ToString();
        }
        */

    }
}