using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior : IItemSelector
    {
        public Option<Vector2Int> HomePosition { get; }
        public BehaviorData BehaviorData { get; }
        public Observable<OnItemSelectMessage> OnItemSelect { get; }
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap world, IInput input);
    }
}