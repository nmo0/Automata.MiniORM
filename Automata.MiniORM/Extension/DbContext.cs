using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM
{
    public class DbContext
    {
        #region Instance

        private static DbContext instance = new DbContext();
        private static string sqlConnectionStr = null;
        private static Dictionary<string, string> sqlCache = new Dictionary<string, string>();

        private static int _timeOut;

        private static int TimeOut {
            get {
                if (_timeOut == 0)
                {
                    return 30;
                }
                return _timeOut;
            }
            set { _timeOut = value; }
        }

        public static DbContext Instance {
            get {
                //if (string.IsNullOrEmpty(sqlConnectionStr))
                //{
                //    //sqlConnectionStr = DBConfig.EPConnection();
                //    throw new Exception("No SqlConnectionString configuration");
                //}
                return instance;
            }
        }

        public static void SetConfig(string str)
        {
            sqlConnectionStr = str;
        }

        public static void SetConfig(string str, int timeOut)
        {
            sqlConnectionStr = str;
            TimeOut = TimeOut;
        }

        static DbContext()
        {

        }

        private DbContext()
        {
        }

        #endregion

        /// <summary>
        /// 获取SQL，如果不存在则生成一个并缓存起来
        /// <para>只有isEntire为true 全量更新时，才会缓存生成的SQL语句，否则是动态的</para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="action"></param>
        /// <param name="isEntire">是否全量更新（对于Update操作）</param>
        /// <returns></returns>
        private string GetOrSetSql(BaseModel model, DbAction action, bool isEntire)
        {
            var sql = string.Empty;
            var key = string.Format("_sql_{0}_{1}", action, model);

            lock (sqlCache)
            {
                if (sqlCache.ContainsKey(key) && isEntire)
                {
                    sql = sqlCache[key];
                }
                else
                {
                    switch (action)
                    {
                        case DbAction.Insert:
                            sql = GenerateInsertSql(model);
                            break;
                        case DbAction.Update:
                            sql = GenerateUpdateSql(model, isEntire);
                            break;
                        case DbAction.Delete:
                            sql = GenerateDeleteSql(model);
                            break;
                        case DbAction.Get:
                        case DbAction.Query:
                            sql = GenerateQuerySql(model, action);
                            break;
                        default:
                            break;
                    }

                    if (isEntire)
                    {
                        sqlCache.Add(key, sql);
                    }
                }
            }

            return sql;
        }

        /// <summary>
        /// 生成参数SQL，默认忽略主键|自增列
        /// </summary>
        /// <param name="model"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="propertys"></param>
        /// <returns></returns>
        private string GenerateParamsSql(BaseModel model, string prefix, string suffix, PropertyInfo[] propertys)
        {
            return GenerateParamsSql(model, prefix, suffix, propertys, true);
        }

        /// <summary>
        /// 生成参数SQL
        /// </summary>
        /// <param name="model"></param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <param name="propertys">属性</param>
        /// <param name="ignoreKey">是否忽略主键|自增列</param>
        /// <returns></returns>
        private string GenerateParamsSql(BaseModel model, string prefix, string suffix, PropertyInfo[] propertys, bool ignoreKey)
        {
            if (propertys == null)
            {
                propertys = model.GetType().GetProperties();
            }

            var sb = new StringBuilder();
            foreach (var item in propertys)
            {
                var customAttributes = item.GetCustomAttributes(true);

                //忽略自增列
                if (customAttributes.FirstOrDefault(m => ignoreKey && (m is SinceTheIncreaseAttribute || m is KeyAttribute) || m is SqlIgnoreAttribute) != null)
                {
                    continue;
                }

                sb.Append(prefix);
                sb.Append(item.Name);
                sb.Append(suffix);
            }
            return sb.ToString().TrimEnd(',');
        }

        /// <summary>
        /// 获取主键名
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetPrimaryKeyName(BaseModel model, PropertyInfo[] propertys)
        {
            if (propertys == null)
            {
                propertys = model.GetType().GetProperties();
            }

            var keyName = string.Empty;
            var type = model.GetType();
            var hasID = false;
            var hasDocumentNo = false;

            foreach (var item in propertys)
            {
                if (item.GetCustomAttributes(true).FirstOrDefault(m => m is KeyAttribute) != null)
                {
                    keyName = item.Name;
                }

                if ("ID".ToLower().Equals(item.Name.ToLower()))
                {
                    hasID = true;
                }

                if ("DocumentNo".ToLower().Equals(item.Name.ToLower()))
                {
                    hasDocumentNo = true;
                }
            }

            if (string.IsNullOrEmpty(keyName) && hasID)
            {
                keyName = "ID";
            }

            if (string.IsNullOrEmpty(keyName) && hasDocumentNo)
            {
                keyName = "DocumentNo";
            }

            return keyName;
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetTableName(BaseModel model)
        {
            var tableName = string.Empty;
            var type = model.GetType();
            tableName = type.Name;

            var tableNameAttribute = type.GetCustomAttributes(true).FirstOrDefault(m => m is TableNameAttribute);

            if (tableNameAttribute != null && !string.IsNullOrEmpty(((TableNameAttribute)tableNameAttribute).Name))
            {
                tableName = ((TableNameAttribute)tableNameAttribute).Name;
            }

            return tableName;
        }

        private string GenerateInsertSql(BaseModel model)
        {
            var properties = model.GetType().GetProperties();

            var tableName = GetTableName(model);

            var sqlBuilder = new StringBuilder();

            var sqlParamsBuilder = GenerateParamsSql(model, "[", "],", properties);
            var sqlValuesBuilder = GenerateParamsSql(model, "@", ",", properties);

            sqlBuilder.Append("Insert Into ");
            sqlBuilder.Append(tableName);
            sqlBuilder.Append(" (");
            sqlBuilder.Append(sqlParamsBuilder);
            sqlBuilder.Append(") Values (");
            sqlBuilder.Append(sqlValuesBuilder);
            sqlBuilder.Append(") ");

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isEntire">是否全量更新，如果是true，表示所有属性都会被更新；否则只更新不为null的</param>
        /// <returns></returns>
        private string GenerateUpdateSql(BaseModel model, bool isEntire)
        {
            var properties = model.GetType().GetProperties();

            var tableName = GetTableName(model);

            var sqlBuilder = new StringBuilder();

            var keyName = GetPrimaryKeyName(model, properties);

            sqlBuilder.Append("Update ");
            sqlBuilder.Append(tableName);

            sqlBuilder.Append(" set ");

            foreach (var item in properties)
            {
                var customAttributes = item.GetCustomAttributes(true);

                //忽略自增列
                if (customAttributes.FirstOrDefault(m => m is SinceTheIncreaseAttribute || m is SqlIgnoreAttribute || m is KeyAttribute) != null)
                {
                    continue;
                }

                //忽略列表里的字段，以及为NULL的字段，不会被更新
                if (item.GetValue(model) != null || isEntire)
                {
                    sqlBuilder.AppendFormat("{0}=@{0},", item.Name);
                }
            }

            if (string.IsNullOrEmpty(keyName))
            {
                throw new Exception("Can not found PrimaryKey");
            }

            sqlBuilder.Remove(sqlBuilder.Length - 1, 1).AppendFormat(" where {0}=@{0}", keyName);

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 生成Delete SQL语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GenerateDeleteSql(BaseModel model)
        {
            var properties = model.GetType().GetProperties();

            var tableName = GetTableName(model);

            var keyName = GetPrimaryKeyName(model, properties);

            if (string.IsNullOrEmpty(keyName))
            {
                throw new Exception("Can not found PrimaryKey");
            }

            return string.Format("Delete from {0} where {1}=@{1}", tableName, keyName);
        }

        /// <summary>
        /// 生成Query SQL语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GenerateQuerySql(BaseModel model, DbAction action)
        {
            var properties = model.GetType().GetProperties();

            var tableName = GetTableName(model);

            var keyName = GetPrimaryKeyName(model, properties);

            var sqlParamsBuilder = GenerateParamsSql(model, "[", "],", properties, false);

            if (string.IsNullOrEmpty(keyName))
            {
                throw new Exception("Can not found PrimaryKey");
            }

            var whereSql = new StringBuilder();

            if (action == DbAction.Get)
            {
                whereSql.AppendFormat("{0}=@{0}", keyName);
            }
            else if (action == DbAction.Query)
            {
                foreach (var item in properties)
                {
                    var customAttributes = item.GetCustomAttributes(true);

                    //忽略自增列
                    if (customAttributes.FirstOrDefault(m => m is SqlIgnoreAttribute) != null)
                    {
                        continue;
                    }

                    //忽略列表里的字段，以及为NULL的字段，不会被更新
                    if (item.GetValue(model) != null)
                    {
                        whereSql.AppendFormat("{0}=@{0} and ", item.Name);
                    }
                }

                whereSql.Remove(whereSql.Length - 5, 5);
            }

            return string.Format("select {0} from {1} where {2}", sqlParamsBuilder, tableName, whereSql);
        }

        /// <summary>
        /// 生成SQL语句
        /// </summary>
        /// <param name="model"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private string GenerateSql(BaseModel model, DbAction action, bool isEntire)
        {
            var sql = string.Empty;

            if (model == null)
            {
                throw new Exception("Count of DbSet Must be greater than 0");
            }

            sql = GetOrSetSql(model, action, isEntire);

            return sql;
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        /// <param name="dbSet">数据集</param>
        /// <param name="action">操作</param>
        /// <param name="isEntire">是否全量更新（对于Update操作）</param>
        /// <returns></returns>
        public int Execute(IEnumerable<BaseModel> dbSet, DbAction action, bool isEntire)
        {
            int executeCount = 0;

            using (var conn = new SqlConnection(sqlConnectionStr))
            {
                conn.Open();
                var trans = conn.BeginTransaction();

                try
                {
                    var sql = GenerateSql(dbSet.FirstOrDefault(), action, isEntire);

                    executeCount += conn.Execute(sql, dbSet, trans, TimeOut, CommandType.Text);

                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    throw e;
                }
            }

            return executeCount;
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        /// <param name="dbModel">数据实体</param>
        /// <param name="action">操作</param>
        /// <param name="isEntire">是否全量更新（对于Update操作）</param>
        /// <returns></returns>
        public int Execute(BaseModel dbModel, DbAction action, bool isEntire)
        {
            int executeCount = -1;

            using (var conn = new SqlConnection(sqlConnectionStr))
            {
                conn.Open();
                var trans = conn.BeginTransaction();

                try
                {
                    var sql = GenerateSql(dbModel, action, isEntire);

                    executeCount += conn.Execute(sql, dbModel, trans, TimeOut, CommandType.Text);

                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    throw e;
                }
            }

            return executeCount;
        }

        public IEnumerable<T> Query<T>(BaseModel dbModel, DbAction action)
        {
            using (var conn = new SqlConnection(sqlConnectionStr))
            {
                try
                {
                    var sql = GenerateSql(dbModel, action, false);

                    return conn.Query<T>(sql, dbModel);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Execute Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param)
        {
            using (var conn = new SqlConnection(sqlConnectionStr))
            {
                try
                {
                    return conn.Query<T>(sql, param);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Execute Sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int Execute(string sql, object param)
        {
            using (var conn = new SqlConnection(sqlConnectionStr))
            {
                try
                {
                    return conn.Execute(sql, param);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }

    public enum DbAction
    {
        Insert,
        Update,
        Delete,
        Query,
        Get
    }
}
