using System.Collections.Generic;
using Domain.Model.Effect;

namespace Domain.Model.Character
{
    public record AffiliationMemento(
        CharacterGroup Group,
        Dictionary<int, float> Affiliations
    );
}