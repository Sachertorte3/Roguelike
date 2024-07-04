using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record ItemEntityMemento(
        ItemMemento Item,
        EntityMemento Entity
    );
}