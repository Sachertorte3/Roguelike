using System;

namespace Stats
{
    [Serializable]
    public class StatData
    {
        public float BaseValue;
        public float AdditiveValue;
        public float AdditiveMultiplier;
        public float MultiplicativeMultiplier;

        public StatData(float baseValue, float additiveValue = 0, float additiveMultiplier = 1,
            float multiplicativeMultiplier = 1)
        {
            BaseValue = baseValue;
            AdditiveValue = additiveValue;
            AdditiveMultiplier = additiveMultiplier;
            MultiplicativeMultiplier = multiplicativeMultiplier;
        }
    }

    [Serializable]
    public class ResourceData
    {
        public StatData Max;
        public float Value;

        public ResourceData(StatData max, float value)
        {
            Max = max;
            Value = value;
        }
    }
}