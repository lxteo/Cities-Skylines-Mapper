using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MapperTests
{
    class Program
    {
        static void Main(string[] args)
        {

            var test = new Mapper.OSMInterface("100.89844,4.2372423,101.25000,5.9879921", 1, 24, 6, 6);            
            
        }
    }
}
