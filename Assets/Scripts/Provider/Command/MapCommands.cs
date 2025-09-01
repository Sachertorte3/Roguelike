#nullable enable
using System;
using Domain.Model.Memento;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using VContainer;

namespace Provider
{
    public class MapCommands
    {
        private readonly GameManager _gameManager;
/*
        [Inject]
        public MapCommands(GameManager gameManager)
        {
            _gameManager = gameManager;

            DebugLogConsole.AddCommandInstance(
                "moveLevelTo",
                "指定したマップに移動します。",
                "MoveLevelTo",
                this);
        }

        private void MoveLevelTo(string mapName, int level)
        {
            try
            {
                _gameManager.MoveMap(new Location(mapName, level));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
*/
    }
}