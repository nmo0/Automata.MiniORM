using Automata.MiniORM.Xml.Extension;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Automata.MiniORM.Xml
{
    /// <summary>
    /// Sql Xml Mapper Manager
    /// </summary>
    public class SqlMapper
    {
        private static Dictionary<string, SqlInfo> _SqlCache;

        private SqlMapper()
        {

        }

        public static string Get(string key)
        {
            return Get(key, new { });
        }

        public static string Get(string key, object param)
        {
            var code = _SqlCache[key];

            using (V8ScriptEngine engine = new V8ScriptEngine())
            {
                return (string)engine.Evaluate(string.Format("var args={0};{1}", param.ToJSON(), code.ScriptCode));
                //return engine.Script.sql;
            }
        }

        public static void Init(string root, params string[] xmlPath)
        {
            _SqlCache = new Dictionary<string, SqlInfo>();

            foreach (var item in xmlPath)
            {
                Load(System.IO.Path.Combine(root, item));
            }
        }

        private static void Load(string path)
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

                _SqlCache.Add(id, new SqlInfo() {
                    Id = id,
                    Element = ele,
                    FileName = path,
                    Type = (SqlType)Enum.Parse(typeof(SqlType), ele.Name, true),
                    ScriptCode = string.Format("var fn=function(args){{{0}return sql;}};var sql=fn(args);sql;", scriptCode.ToString())
                });
            }
        }

        private static void ReadXml(XmlElement ele, StringBuilder scriptCode)
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
