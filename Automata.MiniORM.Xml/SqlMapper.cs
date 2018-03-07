using Automata.MiniORM.Xml.Extension;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string GetScript(string key)
        {
            return _SqlCache[key].ScriptCode;
        }

        public static string GetScript(string key, object param)
        {
            return string.Format("var args={0};{1}", param.ToJSON(), GetScript(key));
        }

        public static string Get(string key, object param)
        {
            var code = _SqlCache[key];

            using (V8ScriptEngine engine = new V8ScriptEngine())
            {
                var sql = (string)engine.Evaluate(string.Format("var args={0};{1}", param.ToJSON(), code.ScriptCode));

                return sql.Trim();
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

        private static string FilterExpression(string text)
        {
            text = text.Replace("'", "\\'").Replace("\r\n", " ").Trim();

            var reg = new Regex(@"#\{\w+(\.\w+)?\}");

            text = reg.Replace(text, match => {
                return string.Format("'+{0}+'", match.Value.Trim('#', '{', '}'));
            });

            return text;

            //var matches = reg.Matches(text);
            //if (matches != null && matches.Count > 0)
            //{
            //}
        }

        private static void ReadXml(XmlElement ele, StringBuilder scriptCode)
        {
            foreach (var child in ele.ChildNodes)
            {
                if (child is XmlText)
                {
                    if (ele.ChildNodes.Count == 1)
                    {
                        scriptCode.AppendFormat("sql=sql+'{0}';", FilterExpression((child as XmlText).InnerText));
                    }
                    else
                    {
                        scriptCode.AppendFormat("sql=sql+' {0}';", FilterExpression((child as XmlText).InnerText));
                    }
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

                        scriptCode.AppendFormat("if({0}){{sql=sql+' {1}';}}", test, FilterExpression(chi.InnerText));

                    }
                    else if (chi.Name == "foreach")
                    {
                        var collection = chi.GetAttribute("collection");
                        var item = chi.GetAttribute("item");
                        var index = chi.GetAttribute("index");
                        var open = chi.GetAttribute("open");
                        var close = chi.GetAttribute("close");
                        var separator = chi.GetAttribute("separator");

                        if (string.IsNullOrEmpty(index))
                        {
                            index = "i";
                        }

                        if (!string.IsNullOrEmpty(open))
                        {
                            scriptCode.AppendFormat("sql=sql+'{0}';", open.Replace("'", "\\'"));
                        }

                        var tempId = string.Format("sql_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8));

                        scriptCode.AppendFormat("var {0}=sql;sql='';", tempId);

                        scriptCode.AppendFormat("for(var {0} in {1}){{var {2} = {1}[{0}];", index, collection, item);

                        ReadXml(chi, scriptCode);

                        scriptCode.AppendFormat("sql=sql+'{0}';}}", separator.Replace("'", "\\'"));

                        scriptCode.AppendFormat("sql=sql.replace(new RegExp('^\\\\s', 'g'), '');");

                        scriptCode.AppendFormat("sql={0}+sql;", tempId);

                        scriptCode.AppendFormat("sql=sql.replace(new RegExp('\\\\'+'{0}'+'+$', 'g'), '{1}');", separator.Replace("'","\\'"), string.Empty);

                        if (!string.IsNullOrEmpty(close))
                        {
                            scriptCode.AppendFormat("sql=sql+'{0}';", close.Replace("'", "\\'"));
                        }
                    }

                }
            }
        }
    }
}
