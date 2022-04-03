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
		/// <param name="settingId"></param>
		/// <returns></returns>
		public static List<SettingMacros> List(long settingId)
		{
			string sql = "SELECT * FROM macros WHERE SettingId=" + settingId;
			return SqliteUtil.QueryData<SettingMacros>(sql);
		}

		/// <summary>
		/// 保存信息
		/// </summary>
		public static long OnSave(SettingMacros macro)
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("SettingId", macro.SettingId);
			dic.Add("Pattern", macro.Pattern);
			dic.Add("Command", macro.Command);

			string insertSql = @"INSERT INTO macros
								(SettingId,Pattern,Command)
								VALUES
								(@SettingId,@Pattern,@Command)";

			// 添加select last_insert_rowid()
			insertSql = insertSql.Replace("\t", "") + ";select last_insert_rowid() from macros;";
			return SqliteUtil.UpdateData(insertSql, dic);
		}


		/// <summary>
		/// 批量存储
		/// </summary>
		/// <param name="settingMacros"></param>
		/// <param name="settingId"></param>
		public static void OnSaveList(List<SettingMacros> settingMacros, long settingId)
		{
			foreach (SettingMacros macro in settingMacros)
			{
				macro.SettingId = settingId;
				OnSave(macro);
			}
		}

		/// <summary>
		/// 按settingId删除
		/// </summary>
		/// <param name="settingId"></param>
		public static void OnDeleteBySettingId(long settingId)
		{
			string deleteSql = "DELETE FROM macros WHERE SettingId=" + settingId;
			SqliteUtil.UpdateData(deleteSql);
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
