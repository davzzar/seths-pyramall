using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SandPerSand
{
    public class TripStone : Behaviour
    {
        public float thresholdX = 0;

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
                PlayerUtils.ShieldPlayerControl(playerGo);
                playerGo.AddComponent<GoTimer>().Init(0.1f, PlayerUtils.StopPlayer);
                playerGo.AddComponent<GoTimer>().Init(1f, PlayerUtils.ResumePlayerControl);
            }
        }
    }


}
