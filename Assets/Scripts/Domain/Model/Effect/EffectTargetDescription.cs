namespace Domain.Model.Effect
{
    /// <summary>スキル説明の「〜を対象に」行。位置と範囲の <see cref="IHasInfo.Info"/> 文言から組み立てる。</summary>
    public static class EffectTargetDescription
    {
        public const string AtFeetPositionInfo = "発動場所";
        public const string SelfAreaInfo = "その場";

        /// <summary>使用時。範囲がその場でも、着弾地点など足元以外なら位置を明示する。</summary>
        public static string OnUse(string positionInfo, string areaInfo, bool useOrThrowCombinedTargets)
        {
            if (areaInfo == SelfAreaInfo)
            {
                if (positionInfo == AtFeetPositionInfo)
                    return useOrThrowCombinedTargets ? "使用者/命中地点を対象に" : "使用者を対象に";
                return $"{positionInfo}を対象に";
            }

            if (positionInfo == AtFeetPositionInfo)
                return $"{areaInfo}を対象に";
            return $"{positionInfo}の{areaInfo}を対象に";
        }

        public static string OnThrow(string positionInfo, string areaInfo)
        {
            if (areaInfo == SelfAreaInfo)
                return "命中地点を対象に";
            if (positionInfo == AtFeetPositionInfo)
                return $"{areaInfo}を対象に";
            return $"{positionInfo}の{areaInfo}を対象に";
        }
    }
}
