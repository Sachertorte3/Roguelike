#nullable enable
using R3;
using Scripts.Data;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Map;
using Scripts.Utilities;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public void Spawn(Tilemap tilemap, CharacterManager characterManager, ItemManager itemManager)
        {
            foreach (Vector2Int position in tilemap.GetAllPassablePositions().GetAtRandom(10))
            {
                characterManager.SpawnCharacter(position);
            }
            DungeonData data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
            foreach (Vector2Int position in tilemap.GetAllPassablePositions().GetAtRandom(30))
            {
                itemManager.SpawnItem(new Item(data.Items.GetAtRandom()), position);
            }
        }
        public void Run(CharacterManager characterManager)
        {
            new TurnController(characterManager);
        }
    }
}
