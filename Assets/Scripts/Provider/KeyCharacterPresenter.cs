#nullable enable
using System.Linq;
using Game;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using View;

namespace Provider
{
    public class KeyCharacterPresenter
    {
        public KeyCharacterPresenter(World world, SynchronizedCharacterView characters,
            SynchronizedIconEntityView iconEntities)
        {
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                var movementEntities = map.EventEntityManager.Stairs.Select(iconEntities.Get);
                var lockPrefab = ScriptableObjectLoader.LoadPrefab("Lock");
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
                    var keyPrefab = ScriptableObjectLoader.LoadPrefab("Key");
                    var key = Object.Instantiate(keyPrefab, character.transform);
                    key.GetComponent<SpriteRenderer>().enabled = character.GetComponent<SpriteRenderer>().enabled;
                }
            });
        }
    }
}