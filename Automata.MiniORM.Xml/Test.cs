using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml
{
    public class Test
    {
        public string Render(dynamic model)
        {
            var result = string.Empty;

            Func<dynamic, string> fn = (dynamic args) => {
                var sql = string.Empty;

                return sql;
            };

            result = fn(model);

            return result;
        }
    }
}
