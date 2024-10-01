#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using UnityEngine;

namespace Domain.Service.Effect
{
    public record SpawnEffectSkillResult : ISkillResult
    {
        public SkillResult Result { get; init; }
        public Color Color { get; init; }
        public IEnumerable<Vector2Int> Area { get; init; }
        private SpawnEffectSkillResult(Color color, IEnumerable<Vector2Int> area, SkillResult result)
        {
            Color = color;
            Area = area;
            Result = result;
        }
        public static readonly SpawnEffectSkillResult Failed = new(new Color(), Enumerable.Empty<Vector2Int>(), SkillResult.Failed);
        public static readonly SpawnEffectSkillResult Cancelled = new(new Color(), Enumerable.Empty<Vector2Int>(), SkillResult.Cancelled);
        public static SpawnEffectSkillResult Success(Color color, IEnumerable<Vector2Int> area) => new(color, area, SkillResult.Success);
    }
}