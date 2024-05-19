using Cysharp.Threading.Tasks;
using Model.Characters;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;

namespace Model
{
    public sealed class TurnController
    {
        private readonly World _world;
        private IEnumerable<Character> GetCharacters() => _world.ActiveMap.CurrentValue.CharacterManager.Characters;
        private int _turn = 1;

        public TurnController(World world)
        {
            _world = world;
            Run();
        }

        public async void Run()
        {
            while (true)
            {
                Log.Debug($"Start turn {_turn}");
                IEnumerable<Character> characterList = GetCharacters();
                foreach (var character in characterList.ToList())
                {
                    character.UpdateTurn();
                    if (character.CanAct && !character.IsDead)
                    {
                        character.State = CharacterState.Think;
                        await character.DoNextAction();
                    }
                }
                await characterList.Select(character =>
                    UniTask.WaitUntil(() => character.State == CharacterState.Wait));
                _turn++;
            }
        }
    }
}