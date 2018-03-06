using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml
{
    public class Columns
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 特性
        /// </summary>
        public IEnumerable<string> Attributes { get; set; }

        /// <summary>
        /// 精度
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }
    }
}
