namespace Data.Effect
{
    public interface IAffiliation
    {
        CharacterGroup Group { get; }
        bool IsAlly(IAffiliation other);
        bool IsEnemy(IAffiliation other);
    }
}