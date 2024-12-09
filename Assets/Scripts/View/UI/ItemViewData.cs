#nullable enable
using UnityEngine;

namespace View.UI
{
    public record ItemViewData(
        Sprite icon,
        int? count,
        bool isCursed,
        bool isShiny,
        bool isCountIdentified,
        bool isCurseIdentified,
        int storageSize,
        string info
    );
}