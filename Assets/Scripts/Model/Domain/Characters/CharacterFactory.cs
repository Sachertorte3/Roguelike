#nullable enable
using Data.Character;
using Model.Domain.Characters.Behavior;
using R3;

namespace Model.Domain.Characters
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