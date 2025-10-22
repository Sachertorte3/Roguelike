#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Service.Events;
using Game;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using View;

namespace Provider
{
    public class KeyCharacterPresenter
    {
        private readonly GameObject _lockPrefab;
        private readonly GameObject _keyPrefab;
        private readonly CompositeDisposable _disposables = new();
        public KeyCharacterPresenter(World world, SynchronizedCharacterView characters,
            SynchronizedIconEntityView iconEntities)
        {
            _lockPrefab = ObjectLoader.LoadPrefab("Lock");
            _keyPrefab = ObjectLoader.LoadPrefab("Key");
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                _disposables.Clear();

                var map = mapChanged.Map;
                foreach (var stairs in map.Stairs)
                {
                    SetLockIcon(characters, iconEntities, map, stairs, stairs.KeyCharacters);
                }
            });
        }

        private void SetLockIcon(SynchronizedCharacterView characters, SynchronizedIconEntityView iconEntities, MapManager map, IEntity lockedEntity, List<Id<IEntity>> keyCharacterIds)
        {
            var keyCharacters = new ObservableList<ICharacter>(keyCharacterIds
                                    .Select(keyCharacterId => map.Characters.ById(keyCharacterId))
                                    .OfType<ICharacter>());
            if (!keyCharacterIds.Any())
                return;
            var lockedEntityView = iconEntities.Get(lockedEntity);
            var lockIcon = Object.Instantiate(_lockPrefab, lockedEntityView.transform)
                .GetComponent<StairsLock>();
            lockIcon.SetVisibility(lockedEntityView.GetComponent<SpriteRenderer>().enabled);
            lockIcon.SetCount(keyCharacterIds.Count);

            map.Characters.ObserveRemove()
                .Where(removed => keyCharacters.Contains(removed.Value))
                .Subscribe(removed => keyCharacters.Remove(removed.Value))
                .AddTo(_disposables);
            keyCharacters.ObserveCountChanged()
                .Subscribe(count => lockIcon.SetCount(count))
                .AddTo(_disposables);
            keyCharacters.ObserveCountChanged()
                .Where(count => count == 0)
                .Subscribe(_ => { lockIcon.UnLock(); })
                .AddTo(_disposables);

            foreach (var keyCharacterId in keyCharacterIds)
            {
                var keyCharacter = map.Characters.ById(keyCharacterId);
                var character = characters.Get(keyCharacter);
                var key = Object.Instantiate(_keyPrefab, character.transform);
                key.GetComponent<SpriteRenderer>().enabled = character.GetComponent<SpriteRenderer>().enabled;
            }
        }
    }
}