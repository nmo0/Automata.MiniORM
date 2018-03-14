using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Extension
{
    /// <summary>
    /// 数据库表的基本操作方法
    /// </summary>
    public static class DataBaseExtensions
    {
        private static DbContext DbContext = DbContext.Instance;
        private static ISqlMapper SqlMapper;

        public static void SetSqlMapper(ISqlMapper sqlMapper)
        {
            SqlMapper = sqlMapper;
        }

        /// <summary>
        /// 重置数据表
        /// </summary>
        /// <param name="that"></param>
        public static void Truncate(this BaseModel that)
        {
            var sql = SqlMapper.Get("sys_truncate_table", DbContext.GetTableName(that));

            DbContext.Execute(string.Empty, null);
        }

        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="that"></param>
        public static void CreateIfNotExist(this BaseModel that)
        {
            var sql = SqlMapper.Get("sys_create_table", new CreateTableInfo
            {
                TableName = DbContext.GetTableName(that),
                Columns = GetColumns(that)
            });

            DbContext.Execute(string.Empty, null);
        }

        //public static void Create(this BaseModel that)
        //{
        //    var sql = SqlMapper.Get(key, param);

        //    DbContext.Execute(string.Empty, null);
        //}

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="that"></param>
        public static void Drop(this BaseModel that)
        {
            var sql = SqlMapper.Get("sys_drop_table", DbContext.GetTableName(that));
            
            DbContext.Execute(sql, null);
        }

        public static List<Columns> GetColumns(this BaseModel model)
        {
            var columns = new List<Columns>();

            var properties = model.GetType().GetProperties();

            foreach (var item in properties)
            {
                var attributes = item.GetCustomAttributes(false);
                var sqlConfigAttribute = attributes.SingleOrDefault(m=>m is SqlConfigAttribute);
                var stringLengthAttribute = attributes.SingleOrDefault(m=>m is StringLengthAttribute);

                var type = item.PropertyType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                    type = type.GetGenericArguments()[0];
                }

                columns.Add(new Columns() {
                    Name = item.Name,
                    Type = type.Name,
                    Attributes = attributes.Select(m=>m.GetType().Name).ToList(),
                    Accuracy = sqlConfigAttribute != null ? (sqlConfigAttribute as SqlConfigAttribute).Accuracy : 2,
                    Length = stringLengthAttribute != null ? (stringLengthAttribute as StringLengthAttribute).MaximumLength : 200
                });
            }

            return columns;
        }
    }
}
