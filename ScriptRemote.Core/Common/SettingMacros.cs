using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Common
{
    public class SettingMacros
    {
        public long Id
        { get; set; }

        /// <summary>
        /// Setting配置关联的Id
        /// </summary>
        public long SettingId
        { get; set; }

        public string Pattern
        { get; set; }

        public string Command
        { get; set; }

        /// <summary>
		/// 是否执行完成，不保存到数据库
		/// </summary>
		public bool exec
        { get; set; } = true;
    }
}
