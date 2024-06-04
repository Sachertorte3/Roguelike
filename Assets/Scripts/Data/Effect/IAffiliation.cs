using R3;

namespace Data.Effect
{
    public interface IAffiliation
    {
        public CharacterGroup Group { get; }
        public Observable<OnAffectionChangedMessage> OnAffectionChanged { get; }
        public bool IsAlly(IAffiliation other);
        public bool IsEnemy(IAffiliation other);
    }
    public record OnAffectionChangedMessage(IAffiliation Target, int Affection, bool IsEnemy, bool IsAlly);
}