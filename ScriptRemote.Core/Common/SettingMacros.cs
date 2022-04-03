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

    }
}
