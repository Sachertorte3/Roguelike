#nullable enable
using System;
using Data;
using Model.Characters;
using Model.Items;
using Model.Map;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model
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
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(10))
                characterManager.SpawnCharacter(position);
            var data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(30))
                itemManager.SpawnItem(new Item(data.Items.GetAtRandom()), position);
        }

        public void Run(CharacterManager characterManager)
        {
            new TurnController(characterManager);
        }
    }
}