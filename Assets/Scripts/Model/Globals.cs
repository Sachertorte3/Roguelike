#nullable enable
using System;
using Model.Map;

namespace Model
{
    public static class Globals
    {
        internal static IWorldViewer? World { get; set; }
        internal static ITilemapViewer? Map { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}