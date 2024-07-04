using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record DownStairsMemento(
        int DestinationMapId,
        EntityMemento Entity
    );
}