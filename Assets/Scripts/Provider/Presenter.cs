#nullable enable
using System.Linq;
using Model;
using Model.Characters;
using Model.Items;
using Model.Map;
using R3;
using Sirenix.Utilities;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using Utilities.ObjectsManager;
using VContainer;
using View;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(TileMaskController tileMask, GameManager gameManager, Tilemap tilemap,
            CharacterManager characterManager, ItemManager itemManager)
        {
            LoggerInit();

            gameManager.Spawn(tilemap, characterManager, itemManager);

            characterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(area =>
            {
                tileMask.SetTilesTranslucent(area.AreaExited);
                tileMask.SetTilesVisible(area.AreaEntered);
                ObjectsManager.GetObjectsByType<SpriteView>()
                    .Where(view => area.AreaExited.Contains(Vector2Int.RoundToInt(view.Position())))
                    .ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>()
                    .Where(view => area.AreaEntered.Contains(Vector2Int.RoundToInt(view.Position())))
                    .ForEach(view => view.SetVisibility(true));
            });
            ObjectsManager.ObserveAdd<SpriteView>().Subscribe(view =>
                view.SetVisibility(characterManager.Player.Area.Get()
                    .Contains(Vector2Int.RoundToInt(view.Position()))));

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