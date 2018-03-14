using System.Text;
using System.Xml;

namespace Automata.MiniORM.Xml
{
    public interface ISqlMapper
    {
        string FilterExpression(string text);
        string Get(string key);
        string Get(string key, object param);
        string Get<T>(string key, T param) where T : class;
        string GetScript(string key);
        string GetScript(string key, object param);
        void Init(string root);
        void Init(string root, string[] xmlPath);
        void Init(string root, string[] dllPath, string[] xmlPath);
        void Load(string path);
        void ReadXml(XmlElement ele, StringBuilder scriptCode);
    }
}