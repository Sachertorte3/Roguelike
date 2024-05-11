#nullable enable
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Map;
using Scripts.Utilities;
using System;
using System.Linq;
using UnityEngine;
using VContainer;
using R3;

namespace Scripts.Model
{
    public class GameManager
    {
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        [Inject]
        public GameManager(Tilemap tilemap, CharacterManager characterManager, ItemManager itemManager)
        {
            new World(tilemap, characterManager);
            characterManager.CharacterEvents.OnPositionChanged.Subscribe(move =>
            {
                ItemEntity? item = itemManager.TryPickUp(move.Position);
                if (item != null)
                {
                    move.Character.PickUp(item.Item);
                }
            });
        }
        public void Spawn(Tilemap tilemap, CharacterManager characterManager, ItemManager itemManager)
        {
            foreach (Vector2Int position in tilemap.GetAllPassablePositions().GetAtRandom(10))
            {
                characterManager.SpawnCharacter(position);
            }
            foreach (Vector2Int position in tilemap.GetAllPassablePositions().GetAtRandom(10))
            {
                itemManager.SpawnItem(position);
            }
        }
        public void Run(CharacterManager characterManager)
        {
            new TurnController(characterManager);
        }
    }
    public static class Globals
    {
        internal static IWorldViewer? World { get; set; }
        internal static ITilemapViewer? Map { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}
