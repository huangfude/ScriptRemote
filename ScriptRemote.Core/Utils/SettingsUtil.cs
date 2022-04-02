using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Utils
{
    public class SettingsUtil
    {
		/// <summary>
		/// 创建表
		/// </summary>
		public static void OnCreate()
        {
			string tableSql = @"CREATE TABLE IF NOT EXISTS settings(
									Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
									ConnectName TEXT,
									ServerAddress TEXT,
									ServerPort INTEGER,
									Username TEXT,
									Password TEXT, 
									KeyFilePath TEXT, 
									KeyFilePassphrase TEXT,
									Sort INTEGER)";
			// 创建
			SqliteUtil.CreateTable(tableSql.Replace("\t", ""));
		}

		/// <summary>
		/// 列表
		/// </summary>
		/// <returns></returns>
		public static List<ConnectionSettings> List()
        {
			return SqliteUtil.QueryData<ConnectionSettings>("SELECT * FROM settings ORDER BY Sort");
		}

		/// <summary>
		/// 查询详情
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public ConnectionSettings Find(long Id)
        {
			string sql = "SELECT * FROM settings WHERE Id="+Id;
			List<ConnectionSettings>  list = SqliteUtil.QueryData<ConnectionSettings>(sql);
			if(list.Count > 0)
            {
				return list[0];
            } 
			return new ConnectionSettings();
		}

		/// <summary>
		/// 排序最大数
		/// </summary>
		/// <returns></returns>
		public static int sortMax()
        {
			return SqliteUtil.QueryCount("SELECT MAX(Sort) FROM settings");
		}

		/// <summary>
		/// 保存信息
		/// </summary>
		public static long OnSave(ConnectionSettings settings)
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("ConnectName", settings.ConnectName);
			dic.Add("ServerAddress", settings.ServerAddress);
			dic.Add("ServerPort", settings.ServerPort);
			dic.Add("Username", settings.Username);
			dic.Add("Password", settings.Password);
			dic.Add("KeyFilePath", settings.KeyFilePath);
			dic.Add("KeyFilePassphrase", settings.KeyFilePassphrase);
			//dic.Add("Sort", settings.Sort);
			dic.Add("Sort", sortMax() + 1);

			string insertSql = @"INSERT INTO settings
								(ConnectName,ServerAddress,ServerPort,Username,Password,KeyFilePath,KeyFilePassphrase,Sort)
								VALUES
								(@ConnectName,@ServerAddress,@ServerPort,@Username,@Password,@KeyFilePath,@KeyFilePassphrase,@Sort)";

			// 添加select last_insert_rowid()
			insertSql = insertSql.Replace("\t", "") + ";select last_insert_rowid() from settings;";
			return SqliteUtil.UpdateData(insertSql, dic);
		}

		/// <summary>
		/// 更新信息
		/// </summary>
		/// <param name="settings"></param>
		public static void OnUpdate(ConnectionSettings settings)
		{
			string updateSql = "UPDATE settings SET ";

			Dictionary<string, object> dic = new Dictionary<string, object>();
			if(settings.ConnectName != null)
            {
				updateSql += "ConnectName='" + settings.ConnectName + "',";
			}
			if (settings.ServerAddress != null)
			{
				updateSql += "ServerAddress='" + settings.ServerAddress + "',";
			}
			if (settings.ServerPort > 0)
			{
				updateSql += "ServerPort=" + settings.ServerPort + ",";
			}
			if (settings.Username != null)
			{
				updateSql += "Username='" + settings.Username + "',";
			}
			if (settings.Password != null)
			{
				updateSql += "Password='" + settings.Password + "',";
			}
			if (settings.KeyFilePath != null)
			{
				updateSql += "KeyFilePath='" + settings.KeyFilePath + "',";
			}
			if (settings.KeyFilePassphrase != null)
			{
				updateSql += "KeyFilePassphrase='" + settings.KeyFilePassphrase + "',";
			}
			if (settings.Sort > 0)
			{
				updateSql += "Sort=" + settings.Sort + ",";
			}
			// 去掉最后一个逗号
			updateSql = updateSql.TrimEnd(',');
			updateSql += " WHERE Id=" + settings.Id;
			SqliteUtil.UpdateData(updateSql);
		}

		/// <summary>
		/// 更新排序
		/// </summary>
		/// <param name="id"></param>
		/// <param name="sort"></param>
		public static void OnUpdateSort(long id, long sort)
		{
			string updateSql = "UPDATE settings SET Sort=" + sort;
			updateSql += " WHERE Id=" + id;
			SqliteUtil.UpdateData(updateSql);
		}

		/// <summary>
		/// 删除信息
		/// </summary>
		/// <param name="Id"></param>
		public static void OnDelete(long Id)
        {
			string deleteSql = "DELETE FROM settings WHERE Id=" + Id;
			SqliteUtil.UpdateData(deleteSql);
		}

	}
}
