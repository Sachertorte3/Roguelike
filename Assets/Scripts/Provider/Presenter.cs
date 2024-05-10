#nullable enable
using R3;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Map;
using Scripts.Utilities;
using Scripts.View;
using Sirenix.Utilities;
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
        public Presenter(TileMaskController tileMask, GameManager gameManager, Tilemap tilemap, CharacterManager characterManager)
        {
            LoggerInit();

            gameManager.Spawn(tilemap, characterManager);

            characterManager.Player.Area.OnVisibleAreaChanged.Subscribe(area =>
            {
                tileMask.SetTilesTranslucent(area.AreaExited);
                tileMask.SetTilesVisible(area.AreaEntered);
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.AreaExited.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.AreaEntered.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(true));
            });
            ObjectsManager.ObserveAdd<SpriteView>().Subscribe(view => view.SetVisibility(characterManager.Player.Area.Get().Contains(Vector2Int.RoundToInt(view.Position()))));

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