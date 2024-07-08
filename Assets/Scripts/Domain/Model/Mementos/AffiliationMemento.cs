using System.Collections.Generic;
using Domain.Model.Effect;

namespace Domain.Model.Character
{
    public record AffiliationMemento(
        int Id,
        CharacterGroup Group,
        Dictionary<int, float> Affiliations
    );
}