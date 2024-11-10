using Domain.Model.Entity;
using Utilities;

namespace Domain.Model.Character
{
    public record OnAffiliationChangedMessage(Id<IEntity> Target);
}