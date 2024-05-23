#nullable enable
using System;

namespace Model
{
    public static class Globals
    {
        internal static GameManager? GameManager { get; set; }
        internal static World? World { get; set; }
        public static Func<bool>? IsDash { get; set; }
        public static Func<bool>? IsNoMove { get; set; }
    }
}