using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SandPerSand
{
    public class HardJumpController : Behaviour
    {
        private bool firstUpdate = false;
        private RigidBody rigidBody;
        public Vector2 StartPosition;
        public Vector2 JumpVelo = Conf.Dist.HardJumpVelocity;
        public float JumpDistance = Conf.Dist.HardJumpDistance;
        protected override void OnEnable()
        {
            // record starting position
            StartPosition = Owner.Transform.Position;

            // disable PlayerControlComponent
            var controlComp = Owner.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = false;
            // Give a Velocity
            rigidBody = Owner.GetComponent<RigidBody>();
            // TODO if I set it here, the LinearVelocity
            // will not be equal to the given JumpVelo from the first Update
            // need better solution
            // rigidBody.LinearVelocity = JumpVelo;
            firstUpdate = true;
            Debug.Print($"HardJumpController Given JumpVelo:{JumpVelo}");
        }

        protected override void Update()
        {
            if (firstUpdate)
            {
                if (rigidBody.LinearVelocity.X >= 0)
                {
                    JumpVelo.X = Math.Abs(JumpVelo.X);
                }
                else
                {
                    JumpVelo.X = -Math.Abs(JumpVelo.X);
                }
                rigidBody.LinearVelocity = JumpVelo;
                firstUpdate = false;
            }
            // if collision happen or reached the goal
            // stop hard jumping and hand over control to PlayerControlComponent
            // note: if 
            if(Math.Abs(Owner.Transform.Position.X - StartPosition.X) > JumpDistance ||
                // collision happening TODO better way of handling collision
                rigidBody.LinearVelocity != JumpVelo)
            {
                IsActive = false;
            }
            Debug.Print($"HardJumpController:{rigidBody.LinearVelocity}");
        }

        protected override void OnDisable()
        {
            // TODO I smelt bug,
            // what if someone want to set controlComp.IsActive = false during the hard jump
            var controlComp = Owner.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = true;
            rigidBody = Owner.GetComponent<RigidBody>();
            rigidBody.LinearVelocity = Vector2.Zero;
        }
    }
}
