using System.IO;
using System.Linq;

namespace Domain.Model.Effect
{
    public record UpgradePath
    {
        private string _path;
        public UpgradePath(params string[] path)
        {
            _path = Path.Combine(path);
        }
        public override string ToString()
        {
            return _path;
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

        public bool Contains(string path)
        {
            return _path.Contains(path);
        }
    }
}