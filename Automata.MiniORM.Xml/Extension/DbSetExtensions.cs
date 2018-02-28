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
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this BaseModel that, object param)
        {
            var sql = string.Empty;

            return DbContext.Query<T>(sql, param);
        }
    }
}
