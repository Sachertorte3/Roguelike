using UnityEngine;

namespace Utilities.WorldCreater
{
    public class WorldGenerator
    {
        private readonly float _seed;
        private readonly float[] AMP_PARAM = { 60, 30, 10 };
        private readonly float[] FREQ_PARAM = { 0.02f, 0.1f, 1 };
        private const float OCEAN_HEIGHT = 40;
        private const float MOUNTAIN_HEIGHT = 60;
        private const float DESERT_PRECIPITATION = -30;
        private const float FOREST_PRECIPITATION = 8;
        public WorldGenerator(float seed)
        {
            _seed = seed / Mathf.PI;
        }

        private WorldTileCategory ChooseTile(float height, float precipitation)
        {
            if (height < OCEAN_HEIGHT)
                return WorldTileCategory.Ocean;
            else if (height >= MOUNTAIN_HEIGHT)
                return WorldTileCategory.Mountain;
            else if (precipitation < DESERT_PRECIPITATION)
                return WorldTileCategory.Desert;
            else if (precipitation < FOREST_PRECIPITATION)
                return WorldTileCategory.Grass;
            else
                return WorldTileCategory.Forest;
        }

        public float GetNoiseAtPosition(Vector2 position)
        {
            float nx = position.x;
            float ny = position.y;
            float noise = 0;
            for (int i = 0; i < AMP_PARAM.Length; i++)
            {
                noise += Mathf.PerlinNoise(nx * FREQ_PARAM[i] + _seed, ny * FREQ_PARAM[i] + _seed) * AMP_PARAM[i];
            }
            return noise;
        }

        public WorldTileCategory GetTile(Vector2 position)
        {
            float height = GetNoiseAtPosition(position);
            float precipitation = GetNoiseAtPosition(position);
            return ChooseTile(height, precipitation);
        }
    }
}