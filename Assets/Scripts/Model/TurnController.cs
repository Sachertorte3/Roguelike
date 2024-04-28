using Cysharp.Threading.Tasks;
using Scripts.Model.Characters;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Unity.Logging;
using UnityEngine;

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
                IEnumerable<Character> characterList = _characterManager.Characters.Where(character => character.CanAct);
                characterList.ForEach(character => character.State = CharacterState.Think);
                await characterList.Select(character => character.DoNextAction());
                await characterList.Select(character => UniTask.WaitUntil(() => character.State == CharacterState.Wait));
                _turn++;
            }
        }
    }
}
