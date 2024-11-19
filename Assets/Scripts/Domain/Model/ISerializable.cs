namespace Domain.Model
{
    public interface ISerializable<TData>
    {
        TData Serialize();
    }
}