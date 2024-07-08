using R3;

namespace Domain.Model.Effect
{
    public interface IAffiliation
    {
        public int Id { get; }
        public CharacterGroup Group { get; }
        public Observable<OnAffectionChangedMessage> OnAffectionChanged { get; }
        public bool IsAlly(IAffiliation other);
        public bool IsEnemy(IAffiliation other);
        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact);
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target, float impact);
    }
}