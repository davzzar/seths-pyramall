using System;
using Engine;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
namespace SandPerSand
{
    public class EffectAnimatorController : Behaviour
    {

        private Animator animator;
        private InputHandler inputHandler;
        protected override void OnAwake()
        {
            animator = Owner.GetComponent<Animator>();
            inputHandler = Owner.GetComponentInParents<PlayerControlComponent>().InputHandler;
        }
        protected override void OnEnable()
        {
            animator.Entry();
        }
        protected override void Update()
        {
            var anim = animator.CurrentAnime;
            switch (anim.Name)
            {
                case "CanHardJump":
                    if (inputHandler.getButtonState(Buttons.A)== ButtonState.Pressed)
                    {
                        animator.NextAnime("HardJumpPressed");
                    }
                    break;
                case "HardJumpPressed":
                    if (anim.IsAtEnd)
                    {
                        animator.NextAnime("Idle");
                    }
                    break;
            }
        }
    }
}
