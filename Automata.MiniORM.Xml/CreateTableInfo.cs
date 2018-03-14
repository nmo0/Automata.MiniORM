using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml
{

    public class CreateTableInfo
    {
        public string TableName { get; set; }
        public List<Columns> Columns { get; set; }
    }
}
