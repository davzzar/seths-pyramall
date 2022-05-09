using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace SandPerSand
{
    public sealed class ActionExecutor : Behaviour
    {
        private bool isArmed = true;

        private bool prevConditionState;

        public Action Action { get; set; }

        public Func<bool> Condition { get; set; }

        public ExecutionMode Mode { get; set; } = ExecutionMode.ConditionBecameTrue;

        public bool Repeat { get; set; } = false;

        public bool IsArmed => this.isArmed;

        public void Rearm()
        {
            this.isArmed = true;
        }

        /// <inheritdoc />
        protected override void Update()
        {
            if (!this.isArmed)
            {
                return;
            }

            var conditionState = this.Condition == null || this.Condition();
            var shouldExecute = false;

            switch (this.Mode)
            {
                case ExecutionMode.ConditionIsTrue:
                {
                    if (conditionState)
                    {
                        shouldExecute = true;
                    }

                    break;
                }
                case ExecutionMode.ConditionBecameTrue:
                {
                    if (!this.prevConditionState && conditionState)
                    {
                        shouldExecute = true;
                    }

                    break;
                }
                case ExecutionMode.ConditionBecameFalse:
                {
                    if (this.prevConditionState && !conditionState)
                    {
                        shouldExecute = true;
                    }

                    break;
                }
            }

            this.prevConditionState = conditionState;

            if (shouldExecute)
            {
                if (this.Action != null)
                {
                    this.Action();
                }

                if (!this.Repeat)
                {
                    this.isArmed = false;
                }
            }
        }
        
        public enum ExecutionMode
        {
            ConditionIsTrue,

            ConditionBecameTrue,

            ConditionBecameFalse
        }
    }
}
