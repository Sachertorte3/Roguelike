#nullable enable
using System.Linq;
using Model.Game;
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
        public KeyCharacterPresenter(World world, SynchronizedCharacterView characters, SynchronizedIconEntityView iconEntities)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    var downStairs = map.EventEntityManager.Stairs.Select(iconEntities.Get);
                    var lockPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Lock.prefab")
                        .WaitForCompletion();
                    foreach (var stairs in downStairs)
                    {
                        var stairsLock = Object.Instantiate(lockPrefab, stairs.transform).GetComponent<StairsLock>();
                        stairsLock.SetVisibility(stairs.GetComponent<SpriteRenderer>().enabled);
                        stairsLock.SetCount(map.KeyCharacters.Count);
                        map.KeyCharacters.ObserveCountChanged().Subscribe(count => stairsLock.SetCount(count)).AddTo(stairsLock);
                        map.DownStairsLocked.Where(isLocked => !isLocked).Subscribe(_ =>
                        {
                            stairsLock.UnLock();
                        }).AddTo(stairsLock);
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