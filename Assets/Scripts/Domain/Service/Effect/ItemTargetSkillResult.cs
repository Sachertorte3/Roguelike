#nullable enable
using Domain.Model.Effect;

namespace Domain.Service.Effect
{
    public record ItemTargetSkillResult : ISkillResult
    {
        public bool IsSuccess { get; init; }
        private ItemTargetSkillResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
        public static readonly ItemTargetSkillResult Failed = new(false);
        public static readonly ItemTargetSkillResult Success = new(true);
    }
}