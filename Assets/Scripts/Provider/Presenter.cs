#nullable enable
using Cysharp.Threading.Tasks;
using Game;
using R3;
using Unity.Logging;
using VContainer;
using View.UI;

namespace Provider
{
    public class Presenter
    {

        [Inject]
        public Presenter(GameManager gameManager, World world, SynchronizedIconEntityView _, SynchronizedThrowAnimationEntityView _2,
            SynchronizedFireEntityView _3, MenuController menuController)
        {
            gameManager.State.Subscribe(state =>
            {
                switch (state)
                {
                    case GameState.Title:
                        Log.Debug("[Game]Change to title scene.");
                        gameManager.Title().Forget();
                        var player = world.ActiveMap.CurrentValue.Player;
                        var destroyLog = player.Character.Entity.DestroyLog;
                        if (destroyLog != null)
                        {
                            var maxMapLevel = gameManager.ActiveStatistics.CurrentValue.MaxMapLevel;
                            var causeOfDeathLog = player.Character.GetNameIgnoreVisibility(player) + destroyLog;
                            menuController.TitleMenuWhenGameOver(
                                maxMapLevel,
                                causeOfDeathLog);
                        }
                        else
                            menuController.TitleMenu();
                        break;
                    case GameState.Dungeon:
                        Log.Debug("[Game]Change to dungeon scene.");
                        menuController.DungeonMenu();
                        break;
                }
            });
        }
    }
}