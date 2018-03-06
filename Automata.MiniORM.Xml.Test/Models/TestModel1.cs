using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata.MiniORM.Xml.Test.Models
{
    public class TestModel1: BaseModel
    {
        [Key]
        public int ID { get; set; }


        public string Name { get; set; }

        [StringLength(500)]
        public string Display{ get; set; }

        [StringLength(-1)]
        public string Description { get; set; }

        [StringLength(12)]
        [SqlConfig(Accuracy = 5)]
        public decimal Weight { get; set; }

        [StringLength(5)]
        public float Star { get; set; }

        public DateTime Birthday { get; set; }

        public bool HasChild { get; set; }
    }
}
