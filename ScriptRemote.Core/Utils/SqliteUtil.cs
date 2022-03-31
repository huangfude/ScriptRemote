using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using ScriptRemote.Core.Common;

namespace ScriptRemote.Core.Utils
{
    public class SqliteUtil
    {
        /*
        private static string dbfolder = AppDomain.CurrentDomain.BaseDirectory;
        private static string dbpath = dbfolder + "connect.db";
        */

        private static SQLiteConnection cn = null;

        private static SQLiteConnection getConnect()
        {
            if (cn == null)
            {
                /*
                DirectoryInfo di = new DirectoryInfo(dbfolder);
                if (!di.Exists)
                {
                    Directory.CreateDirectory(dbfolder);
                }
                */

                cn = new SQLiteConnection("data source=" + GlobalVariable.dbpath);
                cn.Open();
            }
            return cn;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public static void CloseConnect()
        {
            if (cn != null)
            {
                cn.Close();
            }
        }

        /// <summary>
        /// 删除库
        /// </summary>
        public static void DeleteDB()
        {
            if (cn != null && cn.State == ConnectionState.Open)
            {
                cn.Close();
                if (File.Exists(GlobalVariable.dbpath))
                {
                    File.Delete(GlobalVariable.dbpath);
                }
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="sql"></param>
        public static void CreateTable(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tablename"></param>
        public static void DeleteTable(string tablename)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = "DROP TABLE IF EXISTS " + tablename;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 增加/删除/修改数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static int UpdateData(string sql, Dictionary<string, object> dic)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = sql;
            foreach (KeyValuePair<string, object> kvp in dic)
            {
                if (kvp.Value is int)
                {
                    cmd.Parameters.Add(kvp.Key, DbType.Int32).Value = kvp.Value;
                }
                else if (kvp.Value is long)
                {
                    cmd.Parameters.Add(kvp.Key, DbType.Int64).Value = kvp.Value;
                }
                else if (kvp.Value is string)
                {
                    cmd.Parameters.Add(kvp.Key, DbType.String).Value = kvp.Value;
                }
            }

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 批量增加/删除/修改数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicList"></param>
        public static void UpdateDataBatch(string sql, List<Dictionary<string, object>> dicList)
        {
            SQLiteConnection conn = getConnect();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = conn;
            SQLiteTransaction tx = conn.BeginTransaction();
            cmd.Transaction = tx;
            try
            {
                foreach (var dic in dicList)
                {
                    cmd.CommandText = sql;
                    foreach (KeyValuePair<string, object> kvp in dic)
                    {
                        if (kvp.Value is int)
                        {
                            cmd.Parameters.Add(kvp.Key, DbType.Int32).Value = kvp.Value;
                        }
                        else if (kvp.Value is long)
                        {
                            cmd.Parameters.Add(kvp.Key, DbType.Int64).Value = kvp.Value;
                        }
                        else if (kvp.Value is string)
                        {
                            cmd.Parameters.Add(kvp.Key, DbType.String).Value = kvp.Value;
                        }
                    }
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch (SQLiteException E)
            {
                tx.Rollback();
                throw new Exception(E.Message);
            }
        }

        public static int UpdateData(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 清空表数据
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static int DeleteAllData(string tablename)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = "delete from " + tablename;
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> QueryData<T>(string sql) where T : class, new()
        {
            List<T> list = new List<T>();
            T t = new T();
            PropertyInfo[] properts = t.GetType().GetProperties();

            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = sql;
            SQLiteDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                T tmp = new T();
                foreach (var item in properts)
                {
                    try
                    {
                        item.SetValue(tmp, sr[item.Name], null);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Console.WriteLine($"数据库中不包含:{item.Name}");
                    }
                    catch (Exception)
                    {
                    }
                }
                list.Add(tmp);
            }
            sr.Close();

            return list;
        }

        /// <summary>
        /// 查询数据条数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int QueryCount(string sql)
        {
            int count = 0;
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = sql;
            SQLiteDataReader sr = cmd.ExecuteReader();
            sr.Read();
            count = sr.GetInt32(0);
            sr.Close();
            return count;
        }

        /// <summary>
        /// 遍历查询表结构
        /// </summary>
        public static void QueryAllTableInfo()
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConnect();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE TYPE='table' ";
            SQLiteDataReader sr = cmd.ExecuteReader();
            List<string> tables = new List<string>();
            while (sr.Read())
            {
                tables.Add(sr.GetString(0));
            }
            //datareader 必须要先关闭，否则 commandText 不能赋值
            sr.Close();
            foreach (var a in tables)
            {
                cmd.CommandText = $"PRAGMA TABLE_INFO({a})";
                Console.WriteLine($"-------- 表名：{a} --------");
                sr = cmd.ExecuteReader();
                while (sr.Read())
                {
                    Console.WriteLine($"{sr[0]} {sr[1]} {sr[2]} {sr[3]}");
                }
                sr.Close();
                Console.WriteLine("");
            }
        }
    }
}
