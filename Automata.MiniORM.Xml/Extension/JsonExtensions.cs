using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Extension
{
    public static class JsonExtensions
    {

        public static string ToJSON<T>(this T t)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(t);
        }

        public static T ParseJSON<T>(this string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}
