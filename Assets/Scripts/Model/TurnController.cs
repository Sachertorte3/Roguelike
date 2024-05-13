using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Characters;
using Unity.Logging;

namespace Scripts.Model
{
    public sealed class TurnController
    {
        private CharacterManager _characterManager;
        private int _turn = 1;
        public TurnController(CharacterManager characterManager)
        {
            _characterManager = characterManager;
            Run();
        }
        public async void Run()
        {
            while (true)
            {
                Log.Debug($"Start turn {_turn}");
                IEnumerable<Character> characterList = _characterManager.Characters;
                foreach (Character character in characterList.ToList())
                {
                    if (character.CanAct && !character.IsDead)
                    {
                        character.State = CharacterState.Think;
                        await character.DoNextAction();
                    }
                }
                await characterList.Select(character => UniTask.WaitUntil(() => character.State == CharacterState.Wait));
                _turn++;
            }
        }
    }
}
