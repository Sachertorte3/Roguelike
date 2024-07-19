using Utilities;

namespace Domain.Model.Effect
{
    public record OnAffectionChangedMessage(Id<IEntity> Target, float Affection);
}