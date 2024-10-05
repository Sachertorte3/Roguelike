using System;
using System.Collections.Generic;
using Domain.Model.Character;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class FloorData
    {
        [MinValue(1)] public int Depth;
        [Required] public FieldBluePrint Field;
        public FloorTemplates Template = FloorTemplates.Default;
        public bool IsCustom => Template == FloorTemplates.Custom;
        public bool IsBoss => Template == FloorTemplates.Boss;

        public enum FloorTemplates
        {
            Default,
            Boss,
            Shop,
            Custom
        }

        public const float DefaultPrefixChance = 0.2f;
        public const float DefaultShinyChance = 0.01f;
        public const float DefaultSleepChance = 0.5f;
        public const float DefaultMimicChance = 0.1f;
        public const float DefaultGrassChance = 0.3f;
        public const float DefaultShopChance = 0.1f;
        public const float DefaultMonsterHouseChance = 0.1f;
        public const float DefaultRestRoomChance = 0.1f;
        public readonly RoomData DefaultRoom = new(0, 1, 0.5f, 1, 0.5f);
        public readonly RoomData EmptyRoom = new(0, 0, 0, 0, 0);

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _prefixChance = DefaultPrefixChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _shinyChance = DefaultShinyChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _sleepChance = DefaultSleepChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _mimicChance = DefaultMimicChance;

        [ShowIf("@!IsBoss")] [Range(0, 1)] [SerializeField]
        private float _grassChance = DefaultGrassChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _shopChance = DefaultShopChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _monsterHouseChance = DefaultMonsterHouseChance;

        [ShowIf("IsCustom")] [Range(0, 1)] [SerializeField]
        private float _restRoomChance = DefaultRestRoomChance;

        [ShowIf("IsCustom")] [SerializeField] private RoomData _room;
        [ShowIf("IsCustom")] [SerializeField] private bool _existBoss;

        [ShowIf("@(IsCustom && ExistBoss) || IsBoss")] [SerializeField] [RequiredListLength(1, null)]
        private List<EnemyData> _boss;

        public float PrefixChance => Template switch
        {
            FloorTemplates.Default => DefaultPrefixChance,
            FloorTemplates.Boss => DefaultPrefixChance,
            FloorTemplates.Shop => DefaultPrefixChance,
            FloorTemplates.Custom => _prefixChance,
            _ => throw new NotImplementedException()
        };

        public float ShinyChance => Template switch
        {
            FloorTemplates.Default => DefaultShinyChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => DefaultShinyChance,
            FloorTemplates.Custom => _shinyChance,
            _ => throw new NotImplementedException()
        };

        public float SleepChance => Template switch
        {
            FloorTemplates.Default => DefaultSleepChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => DefaultSleepChance,
            FloorTemplates.Custom => _sleepChance,
            _ => throw new NotImplementedException()
        };

        public float MimicChance => Template switch
        {
            FloorTemplates.Default => DefaultMimicChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => DefaultMimicChance,
            FloorTemplates.Custom => _mimicChance,
            _ => throw new NotImplementedException()
        };

        public float GrassChance => Template switch
        {
            FloorTemplates.Default => _grassChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => _grassChance,
            FloorTemplates.Custom => _grassChance,
            _ => throw new NotImplementedException()
        };

        public float ShopChance => Template switch
        {
            FloorTemplates.Default => DefaultShopChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => 1,
            FloorTemplates.Custom => _shopChance,
            _ => throw new NotImplementedException()
        };

        public float MonsterHouseChance => Template switch
        {
            FloorTemplates.Default => DefaultMonsterHouseChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => DefaultMonsterHouseChance,
            FloorTemplates.Custom => _monsterHouseChance,
            _ => throw new NotImplementedException()
        };

        public float RestRoomChance => Template switch
        {
            FloorTemplates.Default => DefaultRestRoomChance,
            FloorTemplates.Boss => 0,
            FloorTemplates.Shop => DefaultRestRoomChance,
            FloorTemplates.Custom => _restRoomChance,
            _ => throw new NotImplementedException()
        };

        public RoomData Room => Template switch
        {
            FloorTemplates.Default => DefaultRoom,
            FloorTemplates.Boss => EmptyRoom,
            FloorTemplates.Shop => DefaultRoom,
            FloorTemplates.Custom => _room,
            _ => throw new NotImplementedException()
        };

        public bool ExistBoss => Template switch
        {
            FloorTemplates.Default => false,
            FloorTemplates.Boss => true,
            FloorTemplates.Shop => false,
            FloorTemplates.Custom => _existBoss,
            _ => throw new NotImplementedException()
        };

        public List<EnemyData> Boss => Template switch
        {
            FloorTemplates.Default => new List<EnemyData>(),
            FloorTemplates.Boss => _boss,
            FloorTemplates.Shop => new List<EnemyData>(),
            FloorTemplates.Custom => _boss,
            _ => throw new NotImplementedException()
        };
    }
}