using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record UpStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    );
}