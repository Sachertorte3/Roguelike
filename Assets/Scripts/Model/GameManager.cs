#nullable enable
using Scripts.Model.Characters;
using Scripts.Model.Map;
using Scripts.Utilities;
using System;
using UnityEngine;
using VContainer;

namespace Scripts.Model
{
    public class GameManager
    {
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        [Inject]
        public GameManager(World world)
        {
        }
        public void Spawn(Tilemap tilemap, CharacterManager characterManager)
        {
            foreach (Vector2Int position in tilemap.GetAllPassablePositions().GetAtRandom(10))
            {
                characterManager.SpawnCharacter(position);
            }
        }
        public void Run(CharacterManager characterManager)
        {
            new TurnController(characterManager);
        }
    }
    public static class Globals
    {
        public static IWorldViewer? World { get; set; }
        public static ITilemapViewer? Map { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}
