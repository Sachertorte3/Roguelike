using Domain.Model.Character;
using Domain.Model.Memento;
using R3;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IAffiliation : ISerializable<AffiliationMemento>
    {
        public Id<IEntity> Id { get; }
        public CharacterGroup Group { get; }
        public Observable<OnAffectionChangedMessage> OnAffectionChanged { get; }
        public bool IsAlly(IAffiliation other);
        public bool IsEnemy(IAffiliation other);
        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact);
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target, float impact);
    }
}