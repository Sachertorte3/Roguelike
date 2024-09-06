#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using UnityEngine;

namespace Domain.Service.Effect
{
    public record SpawnEffectSkillResult : ISkillResult
    {
        public bool IsSuccess { get; init; }
        public Color Color { get; init; }
        public IEnumerable<Vector2Int> Area { get; init; }
        private SpawnEffectSkillResult(Color color, IEnumerable<Vector2Int> area, bool isSuccess)
        {
            Color = color;
            Area = area;
            IsSuccess = isSuccess;
        }
        public static readonly SpawnEffectSkillResult Failed = new(new Color(), Enumerable.Empty<Vector2Int>(), false);
        public static SpawnEffectSkillResult Success(Color color, IEnumerable<Vector2Int> area) => new(color, area, true);
    }
}