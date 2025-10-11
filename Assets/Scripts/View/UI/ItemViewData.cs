#nullable enable
using UnityEngine;

namespace View.UI
{
    public record ItemViewData(
        string name,
        Sprite icon,
        bool canSelect,
        int? count,
        bool isCursed,
        bool isShiny,
        bool isCountIdentified,
        bool isCurseIdentified,
        string info
    );
}