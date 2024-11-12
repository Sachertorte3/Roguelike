#nullable enable
namespace Utilities.Serialize.Result
{
    public class Ok<T> : Result<T>
    {
        public Ok(T value) : base(value)
        {
        }
    }
}