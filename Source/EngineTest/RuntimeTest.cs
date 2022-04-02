using System;
using Engine;
using JetBrains.Annotations;
using NUnit.Framework;

namespace EngineTest
{
    public abstract class RuntimeTest
    {
        private GameEngine engine;

        /// <summary>
        /// Called once to setup the engine with game objects and components
        /// </summary>
        protected virtual void Setup([NotNull]GameEngine engine)
        { }

        /// <summary>
        /// Called once per frame in the update loop.
        /// </summary>
        protected abstract void Test();

        [SetUp]
        private void SetupInternal()
        {
            this.engine = new GameEngine();

            var helperGo = new GameObject();
            var helperComp = helperGo.AddComponent<RuntimeTestHelper>();
            helperComp.UpdateCallback += this.HelperCompOnUpdateCallback;

            this.Setup(this.engine);
        }

        [Test]
        private void DoTest()
        {
            this.engine.Run();
        }

        private void HelperCompOnUpdateCallback(object sender, EventArgs e)
        {
            this.Test();
        }

        private class RuntimeTestHelper : Behaviour
        {
            public event EventHandler<EventArgs> UpdateCallback;

            /// <inheritdoc />
            protected override void Update()
            {
                this.OnUpdateCallback();
            }

            protected virtual void OnUpdateCallback()
            {
                this.UpdateCallback?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
