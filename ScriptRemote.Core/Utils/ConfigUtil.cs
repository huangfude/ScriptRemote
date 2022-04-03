using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Utils
{
    public class ConfigUtil
    {
		/// <summary>
		/// 创建表
		/// </summary>
		public static void OnCreate()
		{
			string tableSql = @"CREATE TABLE IF NOT EXISTS config(
									Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
									Name TEXT,
									Value TEXT,
									Type TEXT)";
			// 创建
			SqliteUtil.CreateTable(tableSql.Replace("\t", ""));
		}

		/// <summary>
		/// 设置默认值
		/// </summary>
		public static void OnDefault()
		{
			if(!ExistName("theme"))
            {
				string sql = @"INSERT INTO config
								(Name,Value,Type)
								VALUES
								('theme','Light.Cyan','')";

				SqliteUtil.UpdateData(sql);
			}
		}

		/// <summary>
		/// 保存信息
		/// </summary>
		public static long OnSave(Config config)
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("Name", config.Name);
			dic.Add("Value", config.Value);
			dic.Add("Type", config.Type);

			string insertSql = @"INSERT INTO config
								(Name,Value,Type)
								VALUES
								(@Name,@Value,@Type)";

			// 添加select last_insert_rowid()
			insertSql = insertSql.Replace("\t", "") + ";select last_insert_rowid() from config;";
			return SqliteUtil.UpdateData(insertSql, dic);
		}

		/// <summary>
		/// 更新属性
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void UpdateNameValue(string name, string value)
		{
			string sql = "UPDATE config SET Value = '" + value + "' WHERE Name='" + name + "'";
			SqliteUtil.UpdateData(sql);
		}

		/// <summary>
		/// 查询属性
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Config FindByName(string name)
		{
			string sql = "SELECT * FROM config WHERE Name='" + name + "'";
			List<Config> list = SqliteUtil.QueryData<Config>(sql);
			if (list.Count > 0)
			{
				return list[0];
			}
			return new Config();
		}

		/// <summary>
		/// 是否存在
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool ExistName(string name)
		{
			string sql = "SELECT COUNT(*) FROM config WHERE Name='" + name + "'";
			int count = SqliteUtil.QueryCount(sql);
			if (count > 0) 
				return true;
			return false;
		}

	}
}
