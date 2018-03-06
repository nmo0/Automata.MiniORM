using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automata.MiniORM
{
    /// <summary>
    /// 表示生成SQL语句时要忽略这一个属性
    /// </summary>
    public class SqlIgnoreAttribute : Attribute { }

    /// <summary>
    /// SQL配置
    /// </summary>
    public class SqlConfigAttribute : Attribute {
        /// <summary>
        /// 小数位精度
        /// </summary>
        public int Accuracy { get; set; }
    }

    /// <summary>
    /// ORM映射表名
    /// </summary>
    public class TableNameAttribute : Attribute {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 表示自增列，那么生成insert/update SQL语句时要忽略
    /// </summary>
    public class SinceTheIncreaseAttribute: Attribute
    {

    }
}