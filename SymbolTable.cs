using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comp
{
    public class SymbolInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public int Line { get; set; }
        public int Position { get; set; }
    }

    public class SymbolTable
    {
        private readonly Dictionary<string, SymbolInfo> symbols =
            new Dictionary<string, SymbolInfo>();

        public bool CheckDuplicate(string name)
        {
            return symbols.ContainsKey(name.ToLower());
        }

        public bool Declare(SymbolInfo symbol)
        {
            string key = symbol.Name.ToLower();

            if (symbols.ContainsKey(key))
                return false;

            symbols.Add(key, symbol);
            return true;
        }

        public SymbolInfo Lookup(string name)
        {
            string key = name.ToLower();

            if (symbols.ContainsKey(key))
                return symbols[key];

            return null;
        }
    }
}
