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
    public class GenerateSimpleSqlTest
    {
        [TestMethod]
        public void SimpleSql_Query()
        {
            var model = new TestModel1() {
                Star = 5
            };

            var sql = DbContext.Instance.GenerateSql(model, DbAction.Query, false);
            var actualSql = "select [ID],[Name],[Display],[Description],[Weight],[Star],[Birthday],[HasChild] from TestModel1 where Star=@Star";
            Assert.AreEqual(sql, actualSql);
        }

        [TestMethod]
        public void SimpleSql_QueryNoWhere()
        {
            var model = new TestModel1();

            var sql = DbContext.Instance.GenerateSql(model, DbAction.Query, false);
            var actualSql = "select [ID],[Name],[Display],[Description],[Weight],[Star],[Birthday],[HasChild] from TestModel1";
            Assert.AreEqual(sql, actualSql);
        }
    }
}
