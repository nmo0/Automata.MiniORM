using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Automata.MiniORM.Xml
{
    public class SqlInfo
    {
        /// <summary>
        /// sql类型
        /// </summary>
        public SqlType Type { get; set; }

        /// <summary>
        /// sql id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 来源文件名（FullName）
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 命名空间（文件名）
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Sql执行脚本
        /// </summary>
        public string ScriptCode { get; set; }

        /// <summary>
        /// XML节点
        /// </summary>
        public XmlElement Element { get; set; }
    }

    public enum SqlType
    {
        Insert = 1,
        Select = 2,
        Update = 4,
        Delete = 8
    }
}
