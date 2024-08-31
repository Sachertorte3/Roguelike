#if UNITY_EDITOR
#endif

namespace Domain.Model
{
    public interface ITable<T>
    {
        T GetRandomItem();
    }
}