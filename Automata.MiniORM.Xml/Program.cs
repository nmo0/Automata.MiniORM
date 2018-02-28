using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var test = new Test();
            test.Load(@"C:\Users\visten\Source\Repos\Automata.MiniORM\Automata.MiniORM.Xml\Xml\Sample\MySqlMapper.xml");
            var sql = test.GenerateSqlString("testsql", new { name = "1234567890", keyword = (string)null });
            Console.WriteLine(sql);

            var sql2 = test.GenerateSqlString("testsql", new { name = "123456789" });
            Console.WriteLine(sql2);
        }
    }
}
