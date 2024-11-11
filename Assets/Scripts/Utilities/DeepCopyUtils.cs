using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utilities
{
    public static class DeepCopyUtils
    {
        public static object DeepCopy(this object target)
        {
            object result;
            BinaryFormatter b = new();
            MemoryStream mem = new();
            try
            {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = b.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;
        }
    }
}