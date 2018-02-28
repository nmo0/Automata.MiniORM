using Automata.MiniORM.Xml.Extension;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Automata.MiniORM.Xml
{
    public class Test
    {
        private Dictionary<string, string> sqlCode = new Dictionary<string, string>();
        //private static V8ScriptEngine engine = new V8ScriptEngine();

        public void Init()
        {
//            using (var engine = new V8ScriptEngine())
//            {

//                var stopwatch = new Stopwatch();
//                stopwatch.Start();

//                for (int i = 0; i < 10000; i++)
//                {
//                    var param = new
//                    {
//                        Type = i.ToString()
//                    };

//                    engine.Evaluate(string.Format(@"var args = {0};
//var result = false;
//if(args.Type === '4567')
//    result = true;
//else 
//    result = false;", param.ToJSON()));

//                    if ((bool)engine.Script.result)
//                    {
//                        Console.WriteLine(i);
//                    }
//                }

//                stopwatch.Stop();
//                Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
//            }
        }

        public void Start(string path)
        {
            Load(path);
        }

        public string GenerateSqlString(string key, object param)
        {
            var code = sqlCode[key];
            using (V8ScriptEngine engine = new V8ScriptEngine())
            {
                engine.Evaluate(string.Format("var args={0};{1}", param.ToJSON(), code));
                return engine.Script.sql;
            }
        }

        public void Load(string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(path);

            Console.WriteLine(xmldoc.DocumentElement.Name);

            XmlNodeList top = xmldoc.DocumentElement.ChildNodes;

            foreach (XmlElement ele in top)
            {
                var scriptCode = new StringBuilder();

                scriptCode.AppendFormat("var sql='';");

                var id = ele.GetAttribute("id");

                ReadXml(ele, scriptCode);


                sqlCode.Add(id, string.Format("var fn=function(args){{{0}return sql;}};var sql=fn(args);", scriptCode.ToString()));

                Console.WriteLine(sqlCode[id]);
            }
        }

        public void ReadXml(XmlElement ele, StringBuilder scriptCode)
        {
            foreach (var child in ele.ChildNodes)
            {
                if (child is XmlText)
                {
                    scriptCode.AppendFormat("sql=sql+' {0}';", (child as XmlText).InnerText.Replace("'", "\\'").Trim());
                }
                else if (child is XmlElement)
                {
                    var chi = child as XmlElement;
                    if (chi.Name == "trim")
                    {
                        var prefix = chi.GetAttribute("prefix");
                        var prefixOverrides = chi.GetAttribute("prefixOverrides");
                        var suffix = chi.GetAttribute("suffix");
                        var suffixOverrides = chi.GetAttribute("suffixOverrides");

                        var tempId = string.Format("sql_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8));

                        scriptCode.AppendFormat("var {0}=sql;sql='';", tempId);

                        ReadXml(chi, scriptCode);

                        if (!string.IsNullOrEmpty(prefix))
                        {
                            scriptCode.AppendFormat("sql=sql.replace(new RegExp('^\\\\'+'\\s{0}'+'+', 'g'), ' {1}');", prefixOverrides, prefix);
                        }
                        else if (!string.IsNullOrEmpty(suffix))
                        {
                            scriptCode.AppendFormat("sql=sql.replace(new RegExp('\\\\'+'\\s{0}'+'+$', 'g'), ' {1}');", suffixOverrides, suffix);
                        }

                        scriptCode.AppendFormat("sql={0}+sql;", tempId);
                    }
                    else if (chi.Name == "if")
                    {
                        var test = chi.GetAttribute("test");

                        scriptCode.AppendFormat("if({0}){{sql=sql+' {1}';}}", test, chi.InnerText.Replace("'", "\\'").Trim());

                        //ReadXml(chi, scriptCode);

                        //scriptCode.Append("}");
                    }

                }
            }
        }
    }
}
