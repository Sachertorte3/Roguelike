namespace Domain.Model.Item
{
    public enum DirectWeaponFeature
    {
        //MEMO: Due to the merge specifications, it should not affect anything other than Skill.
        TwoRangeAttack,      // 2マス攻撃
        FanAttack,           // 扇型攻撃
        SpinAttack,          // 回転攻撃
        DoubleAttack,        // 2回攻撃
        Knockback,            // 吹き飛ばし
        Critical,             // クリティカル
        Dig,                  // 掘る
        Absorbing,            // 吸収
        GuaranteedHit,        // 必中
        ThrowEnhance,         // 投擲強化
    }
}
