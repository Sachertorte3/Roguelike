using System.Text;
using System.Text.RegularExpressions;
using Domain.Model.Effect;

namespace Domain.Model.Item
{
    /// <summary>TMP 向け色タグの定数・ルール・ヘルパー。文言そのものは <see cref="ItemDescriptionPhrases"/> 等で持つ。</summary>
    public static class ItemDescriptionRichText
    {
        public const string HeaderHex = "#C8A85C";

        /// <summary>確率・％表記。</summary>
        public const string ProbabilityHex = "#E8C04D";

        /// <summary>回復量など。</summary>
        public const string HealAmountHex = "#58D698";

        /// <summary>攻撃威力（属性＋数値のまとまり）。</summary>
        public const string PowerHex = "#E8945C";

        /// <summary>距離・射程・範囲のマス数・対象体数など。</summary>
        public const string SpatialHex = "#67B4E8";

        /// <summary>チャージ・クールなどターン数。</summary>
        public const string TimingHex = "#A890E0";

        /// <summary>消費HP。</summary>
        public const string HpCostHex = "#DC8068";

        /// <summary>残り回数・強化・繰り返し回数などメタ系。</summary>
        public const string MetaHex = "#8FA0CC";

        public const string GoodHex = "#6CCF8A";
        public const string BadHex = "#E07070";

        public static string Wrap(string hex, string text) => $"<color={hex}>{text}</color>";

        public static string HeaderLine(string plainText) => Wrap(HeaderHex, plainText);

        public static string HarmfulLine(string plainText) => Wrap(BadHex, plainText);

        private static readonly Regex PercentageToken = new(@"\d+\s*[％%]", RegexOptions.Compiled);

        /// <summary>プレーンな1行に対し、%表記だけを着色（行内に既存の &lt;color&gt; がないこと）。</summary>
        public static string ColorPercentagesInPlainText(string text) =>
            PercentageToken.Replace(text, m => Wrap(ProbabilityHex, m.Value));

        public static string RichHealAmount(int value) => Wrap(HealAmountHex, value.ToString());

        public static string RichSpatial(int value) => Wrap(SpatialHex, value.ToString());

        public static string RichTurns(int value) => Wrap(TimingHex, value.ToString());

        public static string RichHpCost(int value) => Wrap(HpCostHex, value.ToString());

        public static string RichMeta(int value) => Wrap(MetaHex, value.ToString());

        private static readonly Regex ColorTag = new(@"</?color[^>]*>", RegexOptions.Compiled);

        /// <summary>半角・全角の括弧付き数値ブロック（Split だと括弧が落ちる環境があるため Matches で走査）。</summary>
        private static readonly Regex PassiveParenthetical =
            new(@"\([^)]*\)|（[^）]*）", RegexOptions.Compiled);

        /// <summary>パッシブ条件名。括弧外は見出し色、括弧内は効果値（倍率・数値。確率用の黄とは別）。</summary>
        public static string RichPassiveConditionName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return displayName;
            var sb = new StringBuilder();
            var last = 0;
            foreach (Match m in PassiveParenthetical.Matches(displayName))
            {
                if (m.Index > last)
                    sb.Append(Wrap(HeaderHex, displayName.Substring(last, m.Index - last)));
                sb.Append(Wrap(MetaHex, m.Value));
                last = m.Index + m.Length;
            }

            if (last < displayName.Length)
                sb.Append(Wrap(HeaderHex, displayName.Substring(last)));
            return sb.ToString();
        }

        /// <summary>属性＋威力など、攻撃の威力表示用（数値・属性まとまり）。</summary>
        public static string RichAttackPowerSummary(string plainPowerSegment) => Wrap(PowerHex, plainPowerSegment);

        /// <summary>状態異常名のみ有利・不利色で強調（効果ブロック全体の着色とは別）。</summary>
        public static string RichBracketedConditionName(string conditionName, Impact impact)
        {
            var bracketed = $"[{conditionName}]";
            if (impact == Impact.Neutral)
                return bracketed;
            var hex = impact == Impact.Beneficial ? GoodHex : BadHex;
            return Wrap(hex, bracketed);
        }

        /// <summary>TMP の色タグを除いたプレーン断片（テンプレが威力部分を取り出す用）。</summary>
        public static string StripColorTags(string text) =>
            string.IsNullOrEmpty(text) ? text : ColorTag.Replace(text, "");

        /// <summary>効果説明をそのまま返す（有利・不利によるブロック着色は行わない）。</summary>
        public static string StyleEffectInfo(IEffect _, string rawFromInfo) =>
            string.IsNullOrEmpty(rawFromInfo) ? rawFromInfo : rawFromInfo;
    }
}
