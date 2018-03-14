using Automata.MiniORM.Xml.Extension;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Automata.MiniORM.Xml
{
    public class DotNetSqlMapper : ISqlMapper
    {
        private Dictionary<string, SqlInfo> _SqlCache;
        private string[] _Dll;

        public DotNetSqlMapper()
        {
            _SqlCache = new Dictionary<string, SqlInfo>();
        }

        public string FilterExpression(string text)
        {
            //text = text.Replace("'", "\\'").Replace("\r\n", " ").Trim();
            text = text.Replace("\r\n", " ").Trim();

            var reg = new Regex(@"#\{\w+(\.\w+)?\}");

            text = reg.Replace(text, match => {
                return string.Format("\"+{0}+\"", match.Value.Trim('#', '{', '}'));
            });

            return text;
        }

        public string Get(string key)
        {
            return Get(key, null);
        }

        public string Get(string key, object param)
        {
            throw new NotImplementedException();
        }

        public string Get<T>(string key, T param) where T : class
        {
            var code = _SqlCache[key];

            if (code.Assembly == null)
            {
                CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();
                CompilerParameters objCompilerParameters = new CompilerParameters();
                //添加引用
                //data.ReferencedAssemblies.ForEach(m => objCompilerParameters.ReferencedAssemblies.Add(m));

                objCompilerParameters.ReferencedAssemblies.Add("System.dll");//System.Xml.Linq.dll 
                objCompilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");//System.Xml.Linq.dll 
                objCompilerParameters.ReferencedAssemblies.Add("System.Core.dll");//
                objCompilerParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");//

                if (_Dll != null)
                {
                    objCompilerParameters.ReferencedAssemblies.AddRange(_Dll);
                }

                objCompilerParameters.ReferencedAssemblies.Add("Automata.MiniORM.Xml.dll");


                objCompilerParameters.GenerateExecutable = false;
                objCompilerParameters.GenerateInMemory = true;
                //objCompilerParameters.GenerateExecutable = true;
                //objCompilerParameters.OutputAssembly = outputAssembly;
                //objCompilerParameters.MainClass

                // 4.CompilerResults
                CompilerResults cr = objCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, code.ScriptCode);


                if (cr.Errors.HasErrors)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("========== 生成: 成功或最新 0 个，失败 1 个，跳过 0 个 ========== ");
                    stringBuilder.AppendLine("编译错误：");
                    foreach (CompilerError err in cr.Errors)
                    {
                        stringBuilder.AppendLine("" +
                            "Line: " + err.Line +
                            "\tColumn: " + err.Column +
                            "\tNumber: " + err.ErrorNumber +
                            "\t" + err.ErrorText);
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    code.Assembly = cr.CompiledAssembly;
                }


            }

            var type = code.Assembly.GetType("Automata.MiniORM.Xml.Render_" + code.Id);


            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("Render");

            var result = method.Invoke(instance, new object[] { param });

            return result.ToString();
        }

        public string GetScript(string key)
        {
            return _SqlCache[key].ScriptCode;
        }

        public string GetScript(string key, object param)
        {
            return _SqlCache[key].ScriptCode;
        }

        public void Init(string root)
        {
            var files = System.IO.Directory.GetFiles(root, "*.xml", System.IO.SearchOption.AllDirectories);

            foreach (var item in files)
            {
                Load(System.IO.Path.Combine(root, item));
            }

            DataBaseExtensions.SetSqlMapper(this);
            DbSetExtensions.SetSqlMapper(this);
        }

        public void Init(string root, string[] xmlPath)
        {

            if (xmlPath != null)
            {

                foreach (var item in xmlPath)
                {
                    Load(System.IO.Path.Combine(root, item));
                }
            }

            Init(root);
        }

        public void Init(string root, string[] dllPath, string[] xmlPath)
        {
            _Dll = dllPath;

            Init(root, xmlPath);
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

                //scriptCode.AppendFormat("var sql = String.Empty;");

                var id = ele.GetAttribute("id");
                var type = ele.GetAttribute("parameterType");

                ReadXml(ele, scriptCode);

                _SqlCache.Add(id, new SqlInfo()
                {
                    Id = id,
                    Element = ele,
                    FileName = path,
                    ScriptType = ScriptType.CSharp,
                    Type = (SqlType)Enum.Parse(typeof(SqlType), ele.Name, true),
                    ScriptCode = string.Format(@"using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
namespace Automata.MiniORM.Xml
{{
    public class Render_{1}
    {{
        public string Render({2} model)
        {{
            var result = string.Empty;

            Func<{2}, string> fn = ({2} args) => {{
                var sql = string.Empty;
                {0}
                return sql;
            }};

            result = fn(model);

            return result;
        }}
    }}
}}", scriptCode.ToString(), id, type)
                });
            }
        }

        public void ReadXml(XmlElement ele, StringBuilder scriptCode)
        {
            var eachIndex = 0;
            foreach (var child in ele.ChildNodes)
            {
                if (child is XmlText)
                {
                    if (ele.ChildNodes.Count == 1 || eachIndex == 0)
                    {
                        scriptCode.AppendFormat("sql=sql+\"{0}\";", FilterExpression((child as XmlText).InnerText));
                    }
                    else
                    {
                        scriptCode.AppendFormat("sql=sql+\" {0}\";", FilterExpression((child as XmlText).InnerText));
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

                        scriptCode.AppendFormat("var {0}=sql;sql=string.Empty;", tempId);

                        ReadXml(chi, scriptCode);

                        if (!string.IsNullOrEmpty(prefix))
                        {
                            scriptCode.AppendFormat(@"sql = new Regex(@""^\\\s{0}+"").Replace(sql, "" {1}"");", prefixOverrides, prefix);
                        }
                        else if (!string.IsNullOrEmpty(suffix))
                        {
                            scriptCode.AppendFormat(@"sql = new Regex(@""\\\s{0}+$"").Replace(sql, "" {1}"");", suffixOverrides, suffix);
                        }

                        scriptCode.AppendFormat("sql={0}+sql;", tempId);
                    }
                    else if (chi.Name == "if")
                    {
                        var test = chi.GetAttribute("test");

                        scriptCode.AppendFormat("if({0}){{sql=sql+\" {1}\";}}", test, FilterExpression(chi.InnerText));

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
                            scriptCode.AppendFormat("sql=sql+\"{0}\";", open.Replace("\"", "\\\""));
                        }

                        var tempId = string.Format("sql_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8));

                        scriptCode.AppendFormat("var {0}=sql;sql=string.Empty;", tempId);

                        scriptCode.AppendFormat("for(var {0} = 0; {0} < {1}.Count; {0}++){{var {2} = {1}[{0}];", index, collection, item);

                        ReadXml(chi, scriptCode);

                        scriptCode.AppendFormat("sql=sql+\"{0}\";}}", separator.Replace("\"", "\\\""));

                        scriptCode.AppendFormat(@"sql = new Regex(@""^\\\s"").Replace(sql, """");");
                        //scriptCode.AppendFormat("sql=sql.replace(new RegExp('^\\\\s', 'g'), '');");

                        scriptCode.AppendFormat("sql={0}+sql;", tempId);

                        scriptCode.AppendFormat(@"sql = new Regex(@""{0}$"").Replace(sql, ""{1}"");", separator.Replace("'", "\\'"), string.Empty);
                        //scriptCode.AppendFormat("sql=sql.replace(new RegExp('\\\\'+'{0}'+'+$', 'g'), '{1}');", separator.Replace("'", "\\'"), string.Empty);

                        if (!string.IsNullOrEmpty(close))
                        {
                            scriptCode.AppendFormat("sql=sql+\"{0}\";", close.Replace("\"", "\\\""));
                        }
                    }

                }

                eachIndex++;
            }
        }
    }
}
