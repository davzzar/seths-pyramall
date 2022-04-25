using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;
using Engine;
using System.Diagnostics;
namespace SandPerSand
{
    public class MyAnimatorController : Behaviour
    {
        private Animator animator;
        private InputHandler inputHandler;

        protected override void OnEnable()
        {
            animator = Owner.GetComponent<Animator>();
            inputHandler = Owner.GetComponent<PlayerControlComponent>().InputHandler;
        }

        protected override void Update()
        {
            var anim = animator.CurrentAnime;
            var horizontalDir = inputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f);
            //Debug.Print("horizontalDir:" + horizontalDir);
            switch (anim.Name)
            {
                case "Spawn":
                    // switch to idle once reach to the last frame
                    if (anim.IsAtEnd)
                    {
                        animator.NextAnime("Idle");
                    }
                    break;
                case "Idle":
                    // switch to move if player has horizontal velocity
                    if (horizontalDir != 0)
                    {
                        animator.NextAnime("Move");
                    }
                    break;
                case "Move":
                    // switch to idle if player doesn't have horizontal velocity
                    if (horizontalDir == 0)
                    {
                        animator.NextAnime("Idle");
                    }
                    break;
            }
        }
    }
}
