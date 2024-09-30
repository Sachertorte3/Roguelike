using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior : ISerializable<BehaviorMemento>, IItemSelector
    {
        public Option<Vector2Int> HomePosition { get; }
        public BehaviorData BehaviorData { get; }
        public Observable<OnItemSelectMessage> OnItemSelect { get; }
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap world, IInput input);
    }
}