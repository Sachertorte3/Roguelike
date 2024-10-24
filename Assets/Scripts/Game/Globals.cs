#nullable enable
using System;

namespace Model.Game
{
    public static class Globals
    {
        internal static GameManager? GameManager { get; set; }
        internal static World? World { get; set; }
        internal static GameInput? Input { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}