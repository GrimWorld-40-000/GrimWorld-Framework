using UnityEngine;
using Verse;

namespace GW_Frame.Debugging
{
    public class GWLog
    {
        public static Color ErrorMsgCol = new(0.4f, 0.54902f, 1.0f);
        public static Color WarningMsgCol = new(0.70196f, 0.4f, 1.0f);
        public static Color MessageMsgCol = new(0.4f, 1.0f, 0.54902f);

        public static void Error(string msg)
        {
            Log.Error("[GrimWorld Framework] ".Colorize(ErrorMsgCol) + msg);
        }

        public static void Warning(string msg)
        {
            Log.Warning("[GrimWorld Framework] ".Colorize(WarningMsgCol) + msg);
        }

        public static void Message(string msg)
        {
            Log.Message("[GrimWorld Framework] ".Colorize(MessageMsgCol) + msg);
        }
    }
}