using ICities;

namespace Mapper
{
    public class MapperMod : IUserMod
    {
        public string Name
        {
            get
            {
                return "Cimtographer";
            }
        }
        public string Description
        {
            get
            {
                return "Convert from and to OpenStreetMap format.";
            }
        }
    }
}
