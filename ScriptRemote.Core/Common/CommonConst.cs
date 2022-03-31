using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Common
{
    /// <summary>
    /// 全局常量
    /// </summary>
    public class CommonConst
    {
        // 配置文件
        public const string configPath = "connect.cfg";

        public const int DefaultTerminalCols = 160;
        public const int DefaultTerminalRows = 40;

    }

    /// <summary>
    /// 全局变量
    /// </summary>
    public static class GlobalVariable
    {

        // 数据库文件
        public static string dbpath = AppDomain.CurrentDomain.BaseDirectory + "connect.db";

    }

}
