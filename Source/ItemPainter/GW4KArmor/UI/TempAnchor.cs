using UnityEngine;
using Verse;

namespace GW4KArmor.UI;

public readonly ref struct TempAnchor
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