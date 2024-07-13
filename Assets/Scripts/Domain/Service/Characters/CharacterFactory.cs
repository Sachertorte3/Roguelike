#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Area;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Effect.Position;
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
                CharacterStatusManager.Build(20, 20, 10, false, false),
                new EntityMemento(spawnPosition, EntityLayer.Middle),
                new[]
                {
                    new Skill(new SkillData(new AtFeet(), new LineArea(1, false),
                        new AttackEffect(1, new List<AdditionalConditionData>()), "は殴りかかった")).Serialize()
                },
                new InventoryMemento(new ItemMemento[10]),
                CharacterAffiliationManager.Build(CharacterGroup.Player),
                Aggression.AttackAnyone,
                0,
                true,
                false
            );
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition, bool isSleeped, bool isShiney)
        {
            return new CharacterMemento(
                data.Name,
                data.CharacterType,
                data.WanderAround,
                CharacterStatusManager.Build(data.Hp, data.Hp, 5, isSleeped, isShiney),
                new EntityMemento(spawnPosition, EntityLayer.Middle),
                data.Skills.Select(x => new Skill(x).Serialize()).ToArray(),
                new InventoryMemento(new ItemMemento[10]),
                CharacterAffiliationManager.Build(data.Group),
                data.Aggression,
                0,
                false,
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