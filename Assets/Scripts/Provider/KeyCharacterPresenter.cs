#nullable enable
using System.Linq;
using Game;
using ObservableCollections;
using R3;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Provider
{
    public class KeyCharacterPresenter
    {
        public KeyCharacterPresenter(World world, SynchronizedCharacterView characters,
            SynchronizedIconEntityView iconEntities)
        {
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
            {
                var movementEntities = map.EventEntityManager.Stairs.Select(iconEntities.Get);
                var lockPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Lock.prefab")
                    .WaitForCompletion();
                foreach (var movementEntity in movementEntities)
                {
                    var movementLock = Object.Instantiate(lockPrefab, movementEntity.transform)
                        .GetComponent<StairsLock>();
                    movementLock.SetVisibility(movementEntity.GetComponent<SpriteRenderer>().enabled);
                    movementLock.SetCount(map.KeyCharacters.Count);
                    map.KeyCharacters.ObserveCountChanged().Subscribe(count => movementLock.SetCount(count))
                        .AddTo(movementLock);
                    map.MovementEntityLocked.Where(isLocked => !isLocked).Subscribe(_ => { movementLock.UnLock(); })
                        .AddTo(movementLock);
                }

                foreach (var character in map.KeyCharacters.Select(character => characters.Get(character)))
                {
                    var keyPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Key.prefab")
                        .WaitForCompletion();
                    var key = Object.Instantiate(keyPrefab, character.transform);
                    key.GetComponent<SpriteRenderer>().enabled = character.GetComponent<SpriteRenderer>().enabled;
                }
            });
        }
    }
}