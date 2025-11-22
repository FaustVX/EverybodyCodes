using System.Collections.Frozen;

namespace EverybodyCodes.Core
{
    public static class Globals
    {
        public static bool IsTest { get; set; }
        public static FrozenDictionary<string, dynamic>? Args { get; set;}
    }
}
