using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SandPerSand
{
    public class TripStone : Behaviour
    {
        public float thresholdX = 0.5f;
        public float StopDelay = 0.05f;
        public float ResumeDelay = 0.5f;

        protected override void OnEnable()
        {
            base.OnEnable();
            var trigger = Owner.GetOrAddComponent<CircleTriggerComp>();
            trigger.Radius = 0.5f;
            trigger.CollisionEnter += StoneCollision;
        }

        private void StoneCollision(object sender, GameObject playerGo)
        {
            var xSpeed = Math.Abs(playerGo.GetComponent<RigidBody>().LinearVelocity.X);
            if (xSpeed > thresholdX)
            {
                playerGo.GetComponent<PlayerAnimatorController>().TripFlag = true;
                PlayerUtils.ShieldPlayerControl(playerGo);
                playerGo.AddComponent<GoTimer>().Init(StopDelay, PlayerUtils.StopPlayer);
                playerGo.AddComponent<GoTimer>().Init(ResumeDelay, PlayerUtils.ResumePlayerControl);
            }
        }
    }


}
