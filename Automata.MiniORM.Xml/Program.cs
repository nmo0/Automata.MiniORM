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
            SqlMapper.Init(@"D:\GitProject\mini-orm\Automata.MiniORM.Xml\Xml\Sample", "MySqlMapper.xml");

            var sql = SqlMapper.Get("testsql", new { name = "123456789", keyword = (string)null });

            Console.WriteLine(sql);
        }
    }
}
