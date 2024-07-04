namespace Domain.Model.Effect
{
    public record OnAffectionChangedMessage(int Target, float Affection);
}