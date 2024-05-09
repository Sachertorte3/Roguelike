#nullable enable
using RandomDungeonWithBluePrint;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using System;
using UnityEngine;
using Scripts.Utilities;

namespace Scripts.Model
{
    public class GameManager
    {
        public World World;
        public CharacterManager CharacterManager;
        public TurnController TurnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        public GameManager(FieldBluePrint bluePrint)
        {
            Tilemap tilemap = new Tilemap(bluePrint);
            CharacterManager = new CharacterManager();
            World = new World(tilemap, CharacterManager);
            Globals.World = World;
        }
        public void Spawn(CharacterControllInputReceiver receiver)
        {
            CharacterManager.SpawnPlayer(World.Map.GetAllPassablePositions().GetAtRandom(), receiver);
            foreach (Vector2Int position in World.Map.GetAllPassablePositions().GetAtRandom(10))
            {
                CharacterManager.SpawnCharacter(position);
            }
        }
        public void Run()
        {
            TurnController = new TurnController(CharacterManager);
        }
    }
    public static class Globals
    {
        public static IWorldViewer? World { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}
