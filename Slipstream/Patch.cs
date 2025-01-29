using CG.Client.UI;
using HarmonyLib;

namespace Slipstream
{
    [HarmonyPatch(typeof(FadeController), "Start")]
    internal class Patch
    {
        static void Postfix()
        {
            BepinPlugin.Log.LogInfo("Example Patch Executed");
        }
    }
}
