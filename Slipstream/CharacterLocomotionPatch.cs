using CG.Game;
using CG.Game.Player;
using HarmonyLib;
using Opsive.UltimateCharacterController.Character;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Slipstream
{
    [HarmonyPatch(typeof(CharacterLocomotion))]
    internal class CharacterLocomotionPatch
    {
        private static Vector3 lastShipVelocity = Vector3.zero;
        private static Quaternion lastShipRotation = Quaternion.identity;

        [HarmonyPrefix]
        [HarmonyPatch("UpdateExternalForces")]
        static void PrefixForces(CharacterLocomotion __instance, ref Vector3 ___m_ExternalForce, out Vector3 __state)
        {
            __state = Vector3.zero;
            if (!VoidManagerPlugin.Enabled) return;

            //Check if this is the local player
            if (LocalPlayer.Instance.Locomotion != __instance) return;

            //Check if the ship exists
            if (ClientGame.Current?.PlayerShip?.Platform?.Velocity == null || ClientGame.Current.PlayerShip.Transform == null) return;

            Vector3 shipVelocty = ClientGame.Current.PlayerShip.Platform.Velocity;
            Quaternion shipRotation = ClientGame.Current.PlayerShip.Platform.Rotation;

            //If the player is not standing on or in the ship
            if (!LocalPlayer.Instance.IsBeingSimulated)
            {
                Vector3 position = LocalPlayer.Instance.Position;
                IEnumerable<Collider> colliders = ClientGame.Current.PlayerShip.GetColliders();

                if (colliders != null && colliders.Any(collider => collider.bounds.Contains(position)))
                {
                    //Subtract ship velocity from player velocity before calculating resistance
                    __state = shipVelocty;
                    ___m_ExternalForce -= __state;

                    //Add the pilot's input since last frame to player velocity
                    __state += shipVelocty - lastShipVelocity;

                    //Rotate player around the ship as it rotates
                    Quaternion rotationDifference = Quaternion.Inverse(lastShipRotation) * shipRotation;
                    Vector3 playerOffest = position - ClientGame.Current.playerShip.Platform.Position;
                    Vector3 newOffest = rotationDifference * playerOffest;
                    LocalPlayer.Instance.Position += newOffest - playerOffest;
                }
            }

            lastShipVelocity = shipVelocty;
            lastShipRotation = shipRotation;
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateExternalForces")]
        static void PostfixForces(ref Vector3 ___m_ExternalForce, Vector3 __state)
        {
            if (!VoidManagerPlugin.Enabled) return;

            //Re-add ship velocity after resistance
            ___m_ExternalForce += __state;
        }
    }
}
