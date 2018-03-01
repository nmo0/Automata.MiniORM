using Automata.MiniORM.Xml;
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

            var sql = SqlMapper.Get("SampleSqlMapper_03", new string[] { "1", "2", "3", "4" });

            Assert.AreEqual(sql, "select * from table_c where no in('1','2','3','4')");
        }
    }
}
