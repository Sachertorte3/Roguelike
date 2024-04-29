using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Unity.Logging;
using UnityEngine;

namespace Scripts.Model.Map
{
    public class Map
    {
        public IObservable<(Vector2Int position, TileData tile)> OnChangeTile => _onChangeTile;
        private readonly Subject<(Vector2Int, TileData)> _onChangeTile = new Subject<(Vector2Int, TileData)>();
        private readonly ReactiveCollection<TileData> _tiles;
        public readonly int Width;
        public readonly int Height;
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new ReactiveCollection<TileData>(Enumerable.Repeat(new TileData(TileType.Blank), width*height));
            _tiles.ObserveReplace().Subscribe(context => _onChangeTile.OnNext((IndexToVector(context.Index), context.NewValue)));
        }
        public void SetTest()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || x == Width-1 || y == 0 || y == Height-1)
                    {
                        _tiles[x + y * Width] = new TileData(TileType.Wall);
                    }
                    else
                    {
                        _tiles[x + y * Width] = new TileData(TileType.Floor);
                    }
                }
            }
        }
        public Vector2Int IndexToVector(int index)
        {
            if (index < 0)
            {
                Log.Fatal($"index {index} is out of range (< 0)");
                throw new ArgumentOutOfRangeException($"index {index} is out of range (< 0)");
            }
            else if (index >= Width * Height)
            {
                Log.Fatal($"index {index} is out of range (>= {Width * Height})");
                throw new ArgumentOutOfRangeException($"index {index} is out of range (>= {Width * Height})");
            }
            return new Vector2Int(index % Width, index / Width);
        }
    }
    public record TileData(TileType TileType);
    public enum TileType
    {
        Floor,
        Wall,
        Blank,
    }
}
