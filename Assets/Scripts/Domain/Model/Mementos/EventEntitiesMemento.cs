using System.Collections.Generic;

namespace Domain.Model.Map
{
    public record EventEntitiesMemento
    (
        DownStairsMemento DownStairs,
        UpStairsMemento? UpStairs,
        List<ChestMemento> Chests
    );
}