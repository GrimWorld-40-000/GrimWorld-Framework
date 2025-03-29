using System;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsTabRecord : TabRecord
    {
        public readonly SettingsTabDef def;
        
        public SettingsTabRecord(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected)
            : base(label, clickedAction, selected)
        {
            this.def = def;
        }
        
        public virtual void OnGUI(Rect rect)
        {

        }
    }
}