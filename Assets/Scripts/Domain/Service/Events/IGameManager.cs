using Cysharp.Threading.Tasks;

namespace Domain.Service.Events
{
    public interface IGameManager
    {
        public UniTask<int> GetChoice(string text, params string[] choices);
        public void LoadMap(int destinationMapId);
    }
}