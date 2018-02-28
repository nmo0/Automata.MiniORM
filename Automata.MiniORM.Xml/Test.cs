using Automata.MiniORM.Xml.Extension;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml
{
    public class Test
    {
        public void Init()
        {
            using (var engine = new V8ScriptEngine())
            {

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < 10000; i++)
                {
                    var param = new
                    {
                        Type = i.ToString()
                    };

                    engine.Evaluate(string.Format(@"var args = {0};
var result = false;
if(args.Type === '4567')
    result = true;
else 
    result = false;", param.ToJSON()));

                    if ((bool)engine.Script.result)
                    {
                        Console.WriteLine(i);
                    }
                }

                stopwatch.Stop();
                Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
