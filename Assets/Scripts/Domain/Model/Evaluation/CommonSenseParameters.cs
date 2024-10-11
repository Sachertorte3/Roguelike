namespace Domain.Model.Evaluation
{
    public static class CommonSenseParameters
    {
        public const int PlayerMaxHealth = 30;
        public const int MonsterMaxHealth = 15;
        public const int AttacksToDefeatMonster = 2;
        public const int AttacksToDefeatPlayer = 10;
        public const float AttacksPerTurn = 0.05f;
        public const float HpReductionPerTurn = 1 / AttacksToDefeatMonster;
        public const float DamagePerAttack = MonsterMaxHealth / AttacksToDefeatMonster;
        public const float OneTurnStunEquivalentHpReduction = OneTurnStunEquivalentDamage / MonsterMaxHealth;
        public const float OneTurnStunEquivalentDamage = 5;
        public const float EvaluateCoefficient = 0.05f;
        public const int ThrowDistance = 10;
        public const float SkillOnUseProbabilityOfSuccess = 0.95f;
        public const float SkillOnThrowProbabilityOfSuccess = 0.9f;

        public static float BlowAwayPrice(int distance)
        {
            return distance;
        }

        public static float BlowAwayEvaluate(int distance)
        {
            return 0.05f * distance;
        }

        public static float CircleAreaEvaluate(bool canIgnoreWalls, int radius)
        {
            if (canIgnoreWalls)
                return (radius + 1) * 3;
            return (radius + 1) * 2;
        }
    }
}