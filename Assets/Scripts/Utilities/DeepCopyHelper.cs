using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utilities
{
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(this T target)
        {
            T result;
            BinaryFormatter b = new();
            MemoryStream mem = new();
            try
            {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = (T)b.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;
        }
    }
}