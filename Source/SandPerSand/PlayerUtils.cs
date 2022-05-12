﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace SandPerSand
{
    public static class PlayerUtils
    {
        public static void ShieldPlayerControl(GameObject playerGo)
        {
            var controlComp = playerGo.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = false;
            var playerRB = playerGo.GetComponent<RigidBody>();
            playerRB.IgnoreGravity = false;

        }
        public static void ResumePlayerControl(GameObject playerGo)
        {
            var controlComp = playerGo.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = true;
            var playerRB = playerGo.GetComponent<RigidBody>();
            playerRB.IgnoreGravity = true;
        }

        public static void StopPlayer(GameObject playerGo)
        {
            var playerRB = playerGo.GetComponent<RigidBody>();
            playerRB.LinearVelocity = Vector2.Zero;
        }

        // FIXME the collider is broken after setting playerRB.IsActive = false then setting it back true
        public static void FreezePlayer(GameObject playerGo)
        {
            var controlComp = playerGo.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = false;
            var playerRB = playerGo.GetComponent<RigidBody>();
            playerRB.LinearVelocity = Vector2.Zero;
            playerRB.IsActive = false;
        }

        // FIXME the collider is broken after setting playerRB.IsActive = false then setting it back true
        public static void DefreezePlayer(GameObject playerGo)
        {
            var playerRB = playerGo.GetComponent<RigidBody>();
            playerRB.IsActive = true;
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            playerRB.IgnoreGravity = true;
            playerRB.LinearVelocity = Vector2.Zero;
            var controlComp = playerGo.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = true;
        }
    }
}
