using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM
{
    public static class DbSet
    {
        public static DbContext DbContext = DbContext.Instance;

        public static void Insert(this IEnumerable<BaseModel> that)
        {
            DbContext.Execute(that, DbAction.Insert, true);
        }

        /// <summary>
        /// 执行修改操作。默认执行全量修改操作
        /// <para>update sql 语句将会被缓存</para>
        /// </summary>
        /// <param name="that"></param>
        public static void Update(this IEnumerable<BaseModel> that)
        {
            DbContext.Execute(that, DbAction.Update, true);
        }

        /// <summary>
        /// 执行修改操作
        /// </summary>
        /// <param name="that"></param>
        /// <param name="isEntire">是否全量更新，如果为true，则所有属性无论是否为null值，都会执行更新；否则，只更新不为null的属性,并且SQL不会被缓存</param>
        public static void Update(this IEnumerable<BaseModel> that, bool isEntire)
        {
            DbContext.Execute(that, DbAction.Update, isEntire);
        }

        public static void Delete(this IEnumerable<BaseModel> that)
        {
            DbContext.Execute(that, DbAction.Delete, true);
        }


        public static void Insert(this BaseModel that)
        {
            DbContext.Execute(that, DbAction.Insert, true);
        }

        /// <summary>
        /// 执行修改操作。默认执行全量修改操作
        /// <para>update sql 语句将会被缓存</para>
        /// </summary>
        /// <param name="that"></param>
        public static void Update(this BaseModel that)
        {
            DbContext.Execute(that, DbAction.Update, true);
        }

        /// <summary>
        /// 执行修改操作
        /// </summary>
        /// <param name="that"></param>
        /// <param name="isEntire">是否全量更新，如果为true，则所有属性无论是否为null值，都会执行更新；否则，只更新不为null的属性,并且SQL不会被缓存</param>
        public static void Update(this BaseModel that, bool isEntire)
        {
            DbContext.Execute(that, DbAction.Update, isEntire);
        }

        public static void Delete(this BaseModel that)
        {
            DbContext.Execute(that, DbAction.Delete, true);
        }

        /// <summary>
        /// 根据主键查询单个实体详情
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <returns></returns>
        public static T Get<T>(this BaseModel that)
        {
            return DbContext.Query<T>(that, DbAction.Get).FirstOrDefault();
        }

        /// <summary>
        /// 使用有值的属性作为参数（where的=条件），并返回查询出来的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="that"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this BaseModel that)
        {
            return DbContext.Query<T>(that, DbAction.Query);
        }
    }
}
