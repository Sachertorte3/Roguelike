#nullable enable
using UnityEngine;

namespace Utilities
{
    public static class MathExtension
    {
        public static int RandomBinomialApproxValue(float trials, float probability)
        {
            // 二項分布の平均と標準偏差を計算
            float mean = trials * probability;
            float stdDev = Mathf.Sqrt(trials * probability * (1 - probability));

            // 正規分布からのサンプリング
            float normalValue = RandomNormal(mean, stdDev);

            // 結果を整数に四捨五入
            return Mathf.RoundToInt(normalValue);
        }

        // Box-Muller法を使用して正規分布からのサンプリングを行う
        private static float RandomNormal(float mean, float stdDev)
        {
            float u1 = Random.value; // 0から1までの適当な数字を1つ取る
            float u2 = Random.value; // もう一度、0から1までの適当な数字を1つ取る

            // Box-Muller法で乱数を取得する
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                Mathf.Sin(2.0f * Mathf.PI * u2);

            // 結果を計算する
            return mean + stdDev * randStdNormal;
        }
    }
}