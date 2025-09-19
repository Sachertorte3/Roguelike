#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model
{
    public interface IGameManager
    {
        public bool IsEventExecuting { get; }
        public Guid StartEvent();
        public void EndEvent(Guid eventId);
        public UniTask<int> GetChoice(string? text, params string[] choices);
        public UniTask<string> GetTextInput();
        public void MoveMap(Id<IMap> destination, Id<IEntity> from);
        public void PlayBGM(BGM bgm);
        public void PlaySE(SE se);
        public void Save();
        public void SaveLight();
    }
}