using UnityEngine;

namespace Utilities.WorldCreater
{
    public class WorldCreater
    {
        private readonly float _seed;
        private float _heightSeed => _seed * 1.5f;
        private float _precipitationSeed => _seed * 2.5f;
        private readonly float[] AMP_PARAM = { 60, 30, 10 };
        private readonly float[] FREQ_PARAM = { 0.02f, 0.1f, 1 };
        private const float OCEAN_HEIGHT = 40;
        private const float MOUNTAIN_HEIGHT = 60;
        private const float DESERT_PRECIPITATION = 40;
        private const float FOREST_PRECIPITATION = 60;
        public WorldCreater(float seed)
        {
            _seed = seed / Mathf.PI;
        }

        private WorldTileType ChooseTile(float height, float precipitation)
        {
            if (height < OCEAN_HEIGHT)
                return WorldTileType.Ocean;
            else if (height >= MOUNTAIN_HEIGHT)
                return WorldTileType.Mountain;
            else if (precipitation < DESERT_PRECIPITATION)
                return WorldTileType.Desert;
            else if (precipitation < FOREST_PRECIPITATION)
                return WorldTileType.Grass;
            else
                return WorldTileType.Forest;
        }

        public float GetNoiseAtPosition(Vector2 position, float seed)
        {
            float nx = position.x;
            float ny = position.y;
            float noise = 0;
            for (int i = 0; i < AMP_PARAM.Length; i++)
            {
                noise += Mathf.PerlinNoise(nx * FREQ_PARAM[i] + seed, ny * FREQ_PARAM[i] + seed) * AMP_PARAM[i];
            }
            return noise;
        }

        public WorldTileType GetTile(Vector2 position)
        {
            float height = GetNoiseAtPosition(position, _heightSeed);
            float precipitation = GetNoiseAtPosition(position, _precipitationSeed);
            return ChooseTile(height, precipitation);
        }
    }
}