using R3;

namespace Data.Effect
{
    public interface IAffiliation
    {
        public CharacterGroup Group { get; }
        public Observable<OnAffectionChangedMessage> OnAffectionChanged { get; }
        public bool IsAlly(IAffiliation other);
        public bool IsEnemy(IAffiliation other);
        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target);
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target);
    }
    public record OnAffectionChangedMessage(IAffiliation Target, float Affection, bool IsEnemy, bool IsAlly);
}

