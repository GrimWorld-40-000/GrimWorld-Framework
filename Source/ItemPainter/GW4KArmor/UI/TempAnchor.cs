using UnityEngine;
using Verse;
using System;

namespace GW4KArmor.UI
{
    public readonly struct TempAnchor : IDisposable
    {
        private readonly TextAnchor _old;

        public TempAnchor(TextAnchor anchor)
        {
            _old = Text.Anchor;
            Text.Anchor = anchor;
        }

        public void Dispose()
        {
            Text.Anchor = _old;
        }
    }
}