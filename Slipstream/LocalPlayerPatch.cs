using CG.Game.Player;
using Gameplay.SpacePlatforms;
using HarmonyLib;
using Opsive.UltimateCharacterController.Character;
using System.Reflection;
using UnityEngine;

namespace Slipstream
{
    [HarmonyPatch(typeof(LocalPlayer), "SetPlatform")]
    internal class LocalPlayerPatch
    {
        private static readonly FieldInfo externalForce = AccessTools.Field(typeof(CharacterLocomotion), "m_ExternalForce");

        static void Prefix(SpacePlatform ___spacePlatform, out SpacePlatform __state)
        {
            __state = ___spacePlatform;
        }

        static void Postfix(LocalPlayer __instance, SpacePlatform platform, SpacePlatform __state)
        {
            if (!VoidManagerPlugin.Enabled || platform == __state) return;

            if (platform != null)
            {
                //Remove all velocity when landing on the ship
                externalForce.SetValue(__instance.Locomotion, Vector3.zero);
            }
            else if (__state != null)
            {
                //Add ship velocity when stepping off the ship
                Vector3 playerVelocity = (Vector3)externalForce.GetValue(__instance.Locomotion);
                externalForce.SetValue(__instance.Locomotion, playerVelocity + __state.Velocity);
            }
        }
    }
}
