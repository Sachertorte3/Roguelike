#nullable enable
using Domain.Model.Character;
using Domain.Service.Characters.Behavior;
using R3;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public Character CreatePlayer(CharacterMemento playerData, CharacterControllInputReceiver receiver,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(playerData, new PlayerBehavior(receiver), canIgnoreWall, world);
        }

        public Character CreateCharacter(CharacterMemento data, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(data, behavior, canIgnoreWall, world);
        }
    }
}