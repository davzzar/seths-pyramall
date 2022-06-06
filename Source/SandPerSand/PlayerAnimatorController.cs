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
    public class PlayerAnimatorController : Behaviour
    {
        private Animator animator;
        private InputHandler inputHandler;
        private PlayerControlComponent playerControler;
        private PlayerComponent playerComp;
        private GroundCheckComponent groundChecker;
        private RigidBody rigidBody;


        // transformation parameters
        private float xDir;
        private float yRBVelo;
        private float xRBVelo;
        private bool isGrounded;
        private bool jumpFlag;
        private bool sandReachedFlag;
        // TODO update these flags correctly
        private bool collectFlag;
        private bool tripFlag;
        public bool TripFlag
        {
            get => tripFlag;
            set
            {
                tripFlag = value;
            }
        }
        private bool dieFlag;

        protected override void OnEnable()
        {
            animator = Owner.GetComponent<Animator>();
            inputHandler = Owner.GetComponent<PlayerControlComponent>().InputHandler;
            playerControler = Owner.GetComponent<PlayerControlComponent>();
            playerComp = Owner.GetComponent<PlayerComponent>();
            groundChecker = this.Owner.GetOrAddComponent<GroundCheckComponent>();
            rigidBody = this.Owner.GetOrAddComponent<RigidBody>();
        }

        protected override void Update()
        {
            var anim = animator.CurrentAnime;
            xDir = MathF.Abs(this.playerControler.HorizontalSpeed) >= 0.1f ? this.playerControler.HorizontalSpeed : 0f; //inputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f);
            (xRBVelo,yRBVelo) = rigidBody.LinearVelocity;
            isGrounded = groundChecker.IsGrounded;
            //WillJump => isGrounded && JumpButtonPressed ||
            //              HasLanded && BufferedJump ||
            //              !isGrounded && JumpButtonPressed && CanUseCoyote
            jumpFlag = playerControler.WillJump;
            sandReachedFlag = playerControler.HasSandReached;
            dieFlag = !playerComp.IsPlayerAlive;
            // TODO set these flags correctly
            collectFlag = false;


            switch (anim.Name)
            {
                case "Spawn":
                    // switch to idle or fall once reach to the last frame
                    if (anim.IsAtEnd)
                    {
                        switchToAnim("Idle");
                        switchToAnim("Fall");
                    }
                    break;
                case "Idle":
                    switchToAnim("Drown");
                    switchToAnim("Move");
                    switchToAnim("Jump");
                    switchToAnim("FoundItem");
                    break;
                case "Move":
                    switchToAnim("Drown");
                    switchToAnim("Idle");
                    switchToAnim("Jump");
                    switchToAnim("Fall");
                    switchToAnim("FoundItem");
                    switchToAnim("Trip");
                    break;

                case "Jump":
                    switchToAnim("Drown");
                    if (yRBVelo < 0)
                    {
                        switchToAnim("Fall");
                    }else if (yRBVelo == 0)
                    {
                        switchToAnim("Idle");
                        // Move: gonna switch from idle anyway?
                        // can be removed if entering idle has no hDir constraint
                        switchToAnim("Move");
                    }
                    // FoundItem: only play foundItem anim after landing on ground?
                    //switchToAnim("FoundItem");
                    break;
                case "Fall":
                    switchToAnim("Drown");
                    switchToAnim("Land");
                    break;
                case "Land":
                    switchToAnim("Drown");
                    if (anim.IsAtEnd)
                    {
                        switchToAnim("Idle");
                    }
                    switchToAnim("Move");
                    switchToAnim("Jump");
                    break;
                case "Drown":
                    if (!sandReachedFlag)
                    {
                        animator.NextAnime("Jump");
                    }
                    switchToAnim("Die");
                    break;
                case "Trip":
                    if (anim.IsAtEnd)
                    {
                        switchToAnim("Idle");
                        switchToAnim("Move");
                    }
                    break;
                case "Die":
                    break;
                //case "FoundItem":
                //case "Trip":
                //    switchToAnim("Idle");
                //    switchToAnim("Move");
                //    break;
                //default:
                //    break;
            }
        }

        private void switchToAnim(string nextAnimName)
        {
            switch (nextAnimName)
            {
                case "Idle":
                    if (isGrounded && xDir == 0)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "Move":
                    if (isGrounded && xDir != 0 )
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "Jump":
                    if (jumpFlag)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "Fall":
                    if (!isGrounded)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "Land":
                    if (isGrounded)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "Drown":
                    if (sandReachedFlag)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
                case "FoundItem":
                    if (collectFlag)
                    {
                        animator.NextAnime(nextAnimName);
                        collectFlag = false;
                    }
                    break;
                case "Trip":
                    if (tripFlag)
                    {
                        animator.NextAnime(nextAnimName);
                        tripFlag = false;
                    }
                    break;
                case "Die":
                    if (dieFlag)
                    {
                        animator.NextAnime(nextAnimName);
                    }
                    break;
            }

        }
    }
}
