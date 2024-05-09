#nullable enable
using Codice.Client.BaseCommands;
using R3;
using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Map;
using Scripts.Utilities;
using Scripts.View;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using VContainer;
using Logger = Unity.Logging.Logger;

namespace Scripts.Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(TileMaskController tileMask, GameManager gameManager, Tilemap tilemap, CharacterManager characterManager, VisibleArea visibleArea)
        {
            LoggerInit();

            gameManager.Spawn(tilemap, characterManager);

            visibleArea.OnVisibleAreaChanged.Pairwise().Subscribe(area =>
            {
                area.Previous.ExceptWith(area.Current);
                area.Current.ExceptWith(area.Previous);
                tileMask.SetTilesTranslucent(area.Previous);
                tileMask.SetTilesVisible(area.Current);
                IEnumerable<Character> previousVisibleCharacter = characterManager.Characters.Where(character => area.Previous.Contains(character.CurrentPosition));
                IEnumerable<Character> currentVisibleCharacter = characterManager.Characters.Where(character => area.Current.Contains(character.CurrentPosition));
                previousVisibleCharacter.ForEach(character => character.VisibleByPlayer = false);
                currentVisibleCharacter.ForEach(character => character.VisibleByPlayer = true);
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Previous.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Current.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(true));
            });
            ObjectsManager.ObserveAdd<SpriteView>().Subscribe(view => view.SetVisibility(visibleArea.Get().Contains(Vector2Int.RoundToInt(view.Position()))));

            characterManager.Player.Area.Refrash(characterManager.Player.CurrentPosition);

            gameManager.Run(characterManager);
        }
        private void LoggerInit()
        {
            Log.Logger = new Logger(new LoggerConfig()
                .MinimumLevel.Debug()
                .OutputTemplate("[{Timestamp}] {Level} | {Message}{NewLine}{Stacktrace}")
                .WriteTo.UnityDebugLog());
            Log.Debug("Init Logger");
        }
    }
}