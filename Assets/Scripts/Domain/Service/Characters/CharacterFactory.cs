#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Effect.Area;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static CharacterMemento BuildPlayer(string Name, Vector2Int spawnPosition)
        {
            return new CharacterMemento(
                Name,
                new Human(Addressables
                    .LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion()),
                true,
                CharacterStatusManager.Build(100, 1, 10, false),
                new EntityMemento(spawnPosition, EntityLayer.Middle),
                new[]
                {
                    new Skill(new SkillData(new AtFeet(), new LineArea(1, false),
                        new AttackEffect(1, new List<AdditionalConditionData>()), "は殴りかかった")).Serialize()
                },
                null,
                new InventoryMemento(new ItemMemento[10]),
                CharacterAffiliationManager.Build(CharacterGroup.Player, null),
                Aggression.AttackAnyone,
                0,
                true,
                false,
                false
            );
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition, bool isSleeped, bool isShiney, AffiliationMemento? affiliation=null)
        {
            return new CharacterMemento(
                isShiney ? "☆" + data.Name : data.Name,
                data.CharacterType,
                data.WanderAround,
                CharacterStatusManager.Build(isShiney ? data.Hp*3 : data.Hp, 0, 8, isSleeped),
                new EntityMemento(spawnPosition, EntityLayer.Middle),
                data.Skills.Select(x => new Skill(x).Serialize()).ToArray(),
                data.HasLastSkill ? new Skill(data.LastSkill).Serialize() : null,
                new InventoryMemento(new ItemMemento[10]),
                CharacterAffiliationManager.Build(data.Group, affiliation),
                data.Aggression,
                0,
                false,
                isShiney,
                data.IsBoss
            );
        }

        public ICharacter CreatePlayer(CharacterMemento playerData, CharacterControllInputReceiver receiver,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(playerData, new PlayerBehavior(receiver), canIgnoreWall, world);
        }

        public ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(data, behavior, canIgnoreWall, world);
        }
    }
}