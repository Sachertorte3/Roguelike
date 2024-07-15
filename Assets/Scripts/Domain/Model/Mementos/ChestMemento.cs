using Domain.Model.Character;
using Domain.Model.Item;

namespace Domain.Model.Map
{
    public record ChestMemento(
        ItemData Item,
        EntityMemento Entity
    );
}