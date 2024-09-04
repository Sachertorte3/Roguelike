using Cysharp.Threading.Tasks;
using Domain.Model.Map;

namespace Domain.Service.Events
{
    public interface IGameManager
    {
        public UniTask<int> GetChoice(string text, params string[] choices);
        public void LoadMap(Location location);
    }
}