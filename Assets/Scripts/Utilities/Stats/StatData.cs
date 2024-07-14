namespace Stats
{
    public record StatData(float BaseValue, float AdditiveValue = 0, float MultiplicativeValue = 1);
    public record ResourceData(StatData Max, int Value);
}