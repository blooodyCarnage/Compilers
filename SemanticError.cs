using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comp
{
    public class SemanticError
    {
        public string Message { get; set; }
        public string Location { get; set; }
        public string Fragment { get; set; }
    }
}
