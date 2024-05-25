#nullable enable
using System;
using Model.Game;

namespace Model
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