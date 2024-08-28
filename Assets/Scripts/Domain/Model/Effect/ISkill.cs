using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public record UpgradePath
    {
        private string _path;
        public UpgradePath(params string[] path)
        {
            _path = Path.Combine(path);
        }
        public void Prepend(string prefix)
        {
            _path = Path.Combine(prefix, _path);
        }
        public string Pop()
        {
            var segments = _path.Split(Path.DirectorySeparatorChar);
            var firstSegment = segments.First();
            _path = Path.Combine(segments[1..]);
            return firstSegment;
        }
        public static UpgradePath Join(UpgradePath path1, UpgradePath path2)
        {
            return new UpgradePath(Path.Combine(path1._path, path2._path));
        }

        public static UpgradePath Join(string path1, UpgradePath path2)
        {
            return new UpgradePath(Path.Combine(path1, path2._path));
        }

        public static UpgradePath Join(UpgradePath path1, string path2)
        {
            return new UpgradePath(Path.Combine(path1._path, path2));
        }

        public bool Contains(string segment)
        {
            return _path.Split(Path.DirectorySeparatorChar).Contains(segment);
        }
    }
    public record UpgradeData(UpgradePath UpgradePath, System.Action apply);
    public interface ISkill : IHasInfo, IHasUpgrades
    {
    }
    public interface ICharacterSkill : ISerializable<CharacterSkillMemento>, ISkill
    {
        public void UpdateTurn();
        public bool IsUsable();
        public Color Color { get; }
        public int RushDistance { get; }
        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);
        public UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world);
    }
}