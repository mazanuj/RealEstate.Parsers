using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RealEstate.Parsing.Parsers;

namespace RealEstate.Parsing.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            AvitoParser parser = new AvitoParser();
            parser.Test();
        }
    }
}
