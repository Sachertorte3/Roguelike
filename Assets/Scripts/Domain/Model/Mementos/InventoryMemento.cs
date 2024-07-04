namespace Domain.Model.Character
{
    public record InventoryMemento(
        ItemMemento?[] Items
    );
}