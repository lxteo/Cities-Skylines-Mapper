using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class MapperMod : IUserMod
    {
        public string Name
        {
            get
            {
                return "Magic Mapper Mod";
            }
        }
        public string Description
        {
            get
            {
                return "Convert from and to street map format.";
            }
        }
    }
}
