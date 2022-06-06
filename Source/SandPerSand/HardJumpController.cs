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
        private PlayerControlComponent controlComp;
        public Vector2 StartPosition;
        public Vector2 JumpVelo;
        GameObject playerPSGo;
        public float JumpDistance = Conf.HardJumpDistance;


        // Sand Interaction
        private SandSimulation sandSimulation;
        public SandSimulation SandSimulation
        {
            get
            {
                if (this.sandSimulation == null || !this.sandSimulation.IsAlive)
                {
                    this.sandSimulation = GameObject.FindComponent<SandSimulation>();

                }
                return sandSimulation;
            }
        }

        protected override void OnEnable()
        {
            // disable PlayerControlComponent
            controlComp = Owner.GetComponent<PlayerControlComponent>();
            controlComp.IsActive = false;

            // record starting position
            Vector2 distance = Transform.Position - StartPosition;
            // TODO hard code
            // TODO redundent check
            if (distance.Length() >= 2f)
            {
                StartPosition = Owner.Transform.Position;
            }
            else
            {
                // TODO move towards instead of teleport
                Owner.Transform.Position = StartPosition;
            }

            // Give a Velocity
            rigidBody = Owner.GetComponent<RigidBody>();
            JumpVelo = Conf.HardJumpVelocity;
            if (rigidBody.LinearVelocity.X < 0)
            {
                JumpVelo.X = -JumpVelo.X;
            }
            // TODO if I set it here, the LinearVelocity
            // will not be equal to the given JumpVelo from the first Update
            // need better solution
            firstUpdate = true;
            Debug.Print($"HardJumpController Given JumpVelo:{JumpVelo}");

            // TODO breakthroughsand effect
            var inPlacePSGo = Template.MakeJumpThroughSandEffectInPlace(StartPosition,-JumpVelo*0.3f);
            playerPSGo = Template.MakeJumpThroughSandEffect(StartPosition,-JumpVelo*0.3f);
            playerPSGo.Transform.Parent = Owner.Transform;
            playerPSGo.Transform.LocalPosition = Vector2.Zero;
        }

        protected override void Update()
        {
            if (firstUpdate)
            {
                rigidBody.LinearVelocity = JumpVelo;
                firstUpdate = false;
            }
            // delete sands
            Circle circle = new Circle(Transform.Position, 1.5f);
            SandSimulation?.RemoveSand(circle);

            // if collision happen or reached the goal
            // stop hard jumping and hand over control to PlayerControlComponent
            if(Math.Abs(Owner.Transform.Position.X - StartPosition.X) > JumpDistance ||
                // collision happening TODO better way of handling collision
                rigidBody.LinearVelocity != JumpVelo ||
                SandSimulation == null)
            {
                IsActive = false;
            }
            Debug.Print($"HardJumpController:{rigidBody.LinearVelocity}");
        }

        protected override void OnDisable()
        {
            // TODO I smelt bug,
            // what if someone want to set controlComp.IsActive = false during the hard jump
            controlComp.IsActive = true;
            rigidBody.LinearVelocity = JumpVelo * (
                PlayerControlComponent.MaxHorizontalSpeed / Math.Abs(JumpVelo.X));

        }
    }
}
