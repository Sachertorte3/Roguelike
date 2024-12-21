#nullable enable
using Cysharp.Threading.Tasks;
using Game;
using R3;
using Unity.Logging;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class Presenter
    {

        [Inject]
        public Presenter(GameManager gameManager, SynchronizedIconEntityView _, SynchronizedThrowAnimationEntityView _2,
            SynchronizedFireEntityView _3, MenuController menuController)
        {
            gameManager.State.Subscribe(state =>
            {
                switch (state)
                {
                    case GameState.Title:
                        Log.Debug("[Game]Change to title scene.");
                        gameManager.Title().Forget();
                        menuController.TitleMenu(gameManager.CauseOfDeath);
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