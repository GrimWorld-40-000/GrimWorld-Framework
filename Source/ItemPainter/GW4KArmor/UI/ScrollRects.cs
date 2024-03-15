using System.Collections.Generic;
using UnityEngine;

namespace GW4KArmor.UI
{
    internal static class ScrollRects
    {
        internal const int MAX_COUNT = 128;
        private static readonly Vector2[] _scrollPoints = new Vector2[128];
        private static readonly Dictionary<string, int> _indicesByName = new Dictionary<string, int>();

        private static int _maxID;

        internal static int NameToIndex(string name)
        {
            int num;
            var flag = _indicesByName.TryGetValue(name, out num);
            int result;
            if (flag)
            {
                result = num;
            }
            else
            {
                _indicesByName[name] = _maxID;
                result = _maxID++;
            }

            return result;
        }

        internal static ref Vector2 Get(string name)
        {
            return ref _scrollPoints[NameToIndex(name)];
        }
    }
}