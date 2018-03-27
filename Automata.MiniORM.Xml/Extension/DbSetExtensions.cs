using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Extension
{
    public static class DbSetExtensions
    {
        private static DbContext DbContext = DbContext.Instance;
        private static ISqlMapper SqlMapper;

        public static void SetSqlMapper(ISqlMapper sqlMapper)
        {
            SqlMapper = sqlMapper;
        }

        /// <summary>
        /// 使用Sql Mapper 执行sql查询语句
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TParams"></typeparam>
        /// <param name="that"></param>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<TQuery> Query<TQuery, TParams>(this BaseModel that, string key, TParams param) where TParams : class
        {
            var sql = SqlMapper.Get(key, param);

            return DbContext.Query<TQuery>(sql, param);
        }

        /// <summary>
        /// 使用Sql Mapper 执行SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int Execute<TParams>(this BaseModel that, string key, TParams param) where TParams : class
        {
            var sql = SqlMapper.Get(key, param);

            return DbContext.Execute(sql, param);
        }
    }
}
