using UnityEngine;
using Verse;

namespace GW4KArmor.Debugging
{
    public class GW4KLog
    {
        public static Color ErrorMsgCol = new(0.4f, 0.54902f, 1.0f);
        public static Color WarningMsgCol = new(0.70196f, 0.4f, 1.0f);
        public static Color MessageMsgCol = new(0.4f, 1.0f, 0.54902f);

        public static void Error(string msg)
        {
            Log.Error("[GrimWorld 40K] ".Colorize(ErrorMsgCol) + msg);
        }

        public static void Warning(string msg)
        {
            Log.Warning("[GrimWorld 40K] ".Colorize(WarningMsgCol) + msg);
        }

        public static void Message(string msg)
        {
            Log.Message("[GrimWorld 40K] ".Colorize(MessageMsgCol) + msg);
        }
    }
}