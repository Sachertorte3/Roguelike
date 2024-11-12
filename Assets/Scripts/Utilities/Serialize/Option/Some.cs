namespace Utilities.Serialize.Option
{
    public class Some<T> : Option<T>
    {
        public Some(T value) : base(value)
        {
        }
    }
}