using Automata.MiniORM.Xml;
using Automata.MiniORM.Xml.Extension;
using Automata.MiniORM.Xml.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Test2
{
    [TestClass]
    public class SqlMapperTest
    {
        [TestMethod]
        public void Sample01Script()
        {
            SqlMapper.Init(@"../../Xml/Sample", "SampleSqlMapper_01.xml");

            var script = SqlMapper.GetScript("SampleSqlMapper_01");

            Assert.AreEqual(script, @"var fn=function(args){var sql='';sql=sql+' select a.* from table_a as a';if(args.no !== null && args.no !== ''){sql=sql+' left join table_b as b on b.sno = a.no where a.no like \'%\' + @no + \'%\'';}if(!(args.no !== null && args.no !== '')){sql=sql+' where a.status in (\'Complete\') and code = @code';}sql=sql+' and user = @user';if(args.date !== null){sql=sql+' ';}sql=sql+' order by date desc';return sql;};var sql=fn(args);sql;");
        }

        [TestMethod]
        public void Sample01Sql()
        {
            SqlMapper.Init(@"../../Xml/Sample", "SampleSqlMapper_01.xml");

            var sql = SqlMapper.Get("SampleSqlMapper_01", new { no = "NO123456" });

            Assert.AreEqual(sql, "select a.* from table_a as a left join table_b as b on b.sno = a.no where a.no like '%' + @no + '%' and user = @user  order by date desc");

            var sql2 = SqlMapper.Get("SampleSqlMapper_01", new { no = (string)null });

            Assert.AreEqual(sql2, "select a.* from table_a as a where a.status in ('Complete') and code = @code and user = @user  order by date desc");
            
        }


        [TestMethod]
        public void Sample02Sql()
        {
            SqlMapper.Init(@"../../Xml/Sample", "SampleSqlMapper_02.xml");

            var sql = SqlMapper.Get("SampleSqlMapper_02", new { no = "'a', 'b', 'c', 'd'" });

            Assert.AreEqual(sql, "select * from table_c where no in ('a', 'b', 'c', 'd')");
        }


        [TestMethod]
        public void Sample03Sql()
        {
            SqlMapper.Init(@"../../Xml/Sample", "SampleSqlMapper_03.xml");

            var script = SqlMapper.GetScript("SampleSqlMapper_03", new string[] { "1", "2", "3", "4" });

            var sql = SqlMapper.Get("SampleSqlMapper_03", new string[] { "1", "2", "3", "4" });

            Assert.AreEqual(sql, "select * from table_c where no in('1','2','3','4')");
        }

        [TestMethod]
        public void DataBase01Sql()
        {
            SqlMapper.Init(@"../../Xml", "DataBase.xml");

            var model = new TestModel1();

            var param = new
            {
                TableName = DbContext.Instance.GetTableName(model),
                Columns = model.GetColumns()
            };

            var sql = SqlMapper.Get("sys_create_table", param);
            
            var acutalSql = "if object_id(N'TestModel1',N'U') is null begin create table dbo.TestModel1( ID int primary key, Name nvarchar(200), Display nvarchar(500), Description nvarchar(max), Weight decimal(12, 5), Star decimal(5, 2), Birthday datetime, HasChild bit) end";

            Assert.AreEqual(acutalSql, sql);
        }

        [TestMethod]
        public void DataBase02Sql()
        {
            SqlMapper.Init(@"../../Xml", "DataBase.xml");

            var model = new TestModel1();

            var sql = SqlMapper.Get("sys_truncate_table", DbContext.Instance.GetTableName(model));

            var acutalSql = "truncate table dbo.TestModel1";

            Assert.AreEqual(acutalSql, sql);
        }

        [TestMethod]
        public void DataBase03Sql()
        {
            SqlMapper.Init(@"../../Xml", "DataBase.xml");

            var model = new TestModel1();

            var sql = SqlMapper.Get("sys_drop_table", DbContext.Instance.GetTableName(model));

            var acutalSql = "drop table dbo.TestModel1";

            Assert.AreEqual(acutalSql, sql);
        }
    }
}
