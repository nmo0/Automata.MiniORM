using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Extension
{
    public static class DbSetExtensions
    {
        public static DbContext DbContext = DbContext.Instance;

        /// <summary>
        /// 使用Sql Mapper 执行sql查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this BaseModel that, string key, object param)
        {
            var sql = SqlMapper.Get(key, param);

            return DbContext.Query<T>(sql, param);
        }

        /// <summary>
        /// 使用Sql Mapper 执行SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int Execute(this BaseModel that, string key, object param)
        {
            var sql = SqlMapper.Get(key, param);

            return DbContext.Execute(sql, param);
        }
    }
}
