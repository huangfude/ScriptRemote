using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Utils
{
    public class MacrosUtil
    {

		/// <summary>
		/// 创建表
		/// </summary>
		public static void OnCreate()
		{
			string tableSql = @"CREATE TABLE IF NOT EXISTS macros(
									Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
									SettingId INTEGER,
									Pattern TEXT,
									Command TEXT)";
			// 创建
			SqliteUtil.CreateTable(tableSql.Replace("\t", ""));
		}

		/// <summary>
		/// 列表
		/// </summary>
		/// <returns></returns>
		public static List<SettingMacros> List()
		{
			return SqliteUtil.QueryData<SettingMacros>("SELECT * FROM macros");
		}

		/// <summary>
		/// 保存信息
		/// </summary>
		public static void OnSave(SettingMacros macro)
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("SettingId", macro.SettingId);
			dic.Add("Pattern", macro.Pattern);
			dic.Add("Command", macro.Command);

			string insertSql = @"INSERT INTO macros
								(SettingId,Pattern,Command)
								VALUES
								(@SettingId,@Pattern,@Command)";

			SqliteUtil.UpdateData(insertSql.Replace("\t", ""), dic);
		}

		/// <summary>
		/// 删除信息
		/// </summary>
		/// <param name="Id"></param>
		public static void OnDelete(long Id)
		{
			string deleteSql = "DELETE FROM macros WHERE Id=" + Id;
			SqliteUtil.UpdateData(deleteSql);
		}

	}
}
