#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Events
{
    public interface IGameManager
    {
        public UniTask<int> GetChoice(string? text, params string[] choices);
        public void LoadMap(Location location, Id<IEntity> from);
    }
}