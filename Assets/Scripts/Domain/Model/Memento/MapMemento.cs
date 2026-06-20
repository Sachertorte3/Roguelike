#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MapMemento
    {
        [SerializeField] private string _id;
        public Id<IMap> Id => new(_id);
        [field: SerializeField] public TilemapMemento Tilemap { get; private set; }
        [field: SerializeField] public EntitiesMemento Entities { get; private set; }
        [field: SerializeField] public Option<RoomMemento> MonsterHouse { get; private set; }
        [field: SerializeField] public Option<ShopMemento> Shop { get; private set; }

        /// <summary>
        /// 新ゲーム開始時のプレイヤー初期スポーン位置。マップ生成時(MapBuilder)に1回だけ設定される。
        /// 新ゲームの最初のマップ構築でのみ読まれ、セーブ復帰や階段遷移では使われないため、
        /// 2回目以降の保存（MapManager.Serialize）では空(zero)のまま。
        /// </summary>
        [field: SerializeField] public Vector2Int InitialPlayerPosition { get; private set; }

        public MapMemento(
            Id<IMap> id,
            TilemapMemento tilemap,
            EntitiesMemento entities,
            Option<RoomMemento> monsterHouse,
            Option<ShopMemento> shop,
            Vector2Int initialPlayerPosition)
        {
            _id = id.ToString();
            Tilemap = tilemap;
            Entities = entities;
            MonsterHouse = monsterHouse;
            Shop = shop;
            InitialPlayerPosition = initialPlayerPosition;
        }
    }
}