using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Service.Action;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class Chase : IDiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            return GenerateMoveActionsDoable(character, targetPosition, world).Cast<IAction>()
                .Concat(GenerateUseSkillActionsDoable(character, world))
                .Concat(GenerateUseItemActionsDoable(character, world))
                .Concat(GenerateThrowItemActionsDoable(character, world));
        }

        private IEnumerable<Move> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap world)
        {
            var route = new AStar(world.GetAllPassablePositions()).Calc(character.CurrentPosition, targetPosition);
            if (route.Count < 2)
            {
                Log.Debug($"Already reached the target position");
                return Enumerable.Empty<Move>();
            }

            var direction = DirectionMethods.FromVector(route[1] - route[0]);

            var move = new Move(direction, 0.01f);
            if (move.Doable(character, world))
            {
                return new List<Move> { move };
            }

            Log.Debug($"Move to {direction} is not doable");
            return Enumerable.Empty<Move>();
        }

        private IEnumerable<UseSkill> GenerateUseSkillActionsDoable(IHasBehavior character, IMap world)
        {
            return character.Skills
                .SelectMany(
                    skill => DirectionMethods.AllDirections
                        .Select(direction => new UseSkill(skill, direction))
                )
                .Where(action => action.Doable(character, world));
        }

        private IEnumerable<UseItem> GenerateUseItemActionsDoable(IHasBehavior character, IMap world)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<UseItem>();
            }
            
            return character.Inventory.AllItems
                .SelectMany(
                    item => DirectionMethods.AllDirections
                        .Select(direction => new UseItem(item, direction))
                )
                .Where(action => action.Doable(character, world));
        }
        
        private IEnumerable<ThrowItem> GenerateThrowItemActionsDoable(IHasBehavior character, IMap world)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<ThrowItem>();
            }
            
            return character.Inventory.AllItems
                .SelectMany(
                    item => DirectionMethods.AllDirections
                        .Select(direction => new ThrowItem(item, direction))
                )
                .Where(action => action.Doable(character, world));
        }
    }
}