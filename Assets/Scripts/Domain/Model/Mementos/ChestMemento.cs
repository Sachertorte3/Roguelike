using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record ChestMemento(
        ItemData Item,
        EntityMemento Entity
    );
}