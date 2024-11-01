using System.IO;
using System.Linq;

namespace Domain.Model.Item
{
    public record UpgradePath
    {
        private readonly string _path;

        public UpgradePath(params string[] path)
        {
            _path = Path.Combine(path);
        }

        public string GetUpgradeName()
        {
            return _path.Split(Path.DirectorySeparatorChar).Last();
        }

        public string Peek()
        {
            return _path.Split(Path.DirectorySeparatorChar).First();
        }

        public override string ToString()
        {
            return _path;
        }

        public UpgradePath Prepend(string prefix)
        {
            return new UpgradePath(Path.Combine(prefix, _path));
        }

        public UpgradePath Pop()
        {
            var segments = _path.Split(Path.DirectorySeparatorChar);
            return new UpgradePath(Path.Combine(segments[1..]));
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