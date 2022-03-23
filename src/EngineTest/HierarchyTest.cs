using System.Collections.Generic;
using System.Linq;
using Engine;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace EngineTest
{
    public class HierarchyTest
    {
        [SetUp]
        public void Setup()
        {
            var emptyScene = new Scene();
            SceneManager.LoadScene(emptyScene);
        }

        [Test]
        public void FlatHierarchyTest()
        {
            var gos = new List<GameObject>();

            for (var i = 0; i < 10; i++)
            {
                var go = new GameObject();
                go.Name = "GameObject " + i;
                go.Transform.LocalPosition = Vector2.One;
                gos.Add(go);
            }

            Assert.AreEqual(gos.Count, SceneManager.ActiveScene.Objects.Count, "Objects count missmatch.");
            Assert.AreEqual(gos.Count, SceneManager.ActiveScene.RootObjects.Count(), "Root count missmatch");

            for (var i = 0; i < gos.Count; i++)
            {
                var go = gos[i];
                Assert.IsTrue(SceneManager.ActiveScene.Objects.Contains(go),
                    "SceneManager.ActiveScene.Objects.Contains(go)");
                Assert.IsTrue(SceneManager.ActiveScene.RootObjects.Contains(go),
                    "SceneManager.ActiveScene.RootObjects.Contains(go)");
                Assert.IsTrue(go.Transform.Parent == null, "go.Transform.Parent == null");
                Assert.AreEqual(Vector2.One, go.Transform.LocalPosition);
                Assert.AreEqual(Vector2.One, go.Transform.Position);
            }
        }

        [Test]
        public void DeepHierarchyTest()
        {
            var gos = new List<GameObject>();
            Transform prev = null;

            for (var i = 0; i < 10; i++)
            {
                var go = new GameObject();
                go.Name = "GameObject " + i;
                go.Transform.LocalPosition = Vector2.One;
                go.Transform.Parent = prev;
                prev = go.Transform;
                gos.Add(go);
            }

            Assert.AreEqual(gos.Count, SceneManager.ActiveScene.Objects.Count, "Objects count missmatch.");
            Assert.AreEqual(1, SceneManager.ActiveScene.RootObjects.Count(), "Root count missmatch");

            for (var i = 0; i < gos.Count; i++)
            {
                var go = gos[i];
                Assert.IsTrue(SceneManager.ActiveScene.Objects.Contains(go),
                    "SceneManager.ActiveScene.Objects.Contains(go)");

                if (i == 0)
                {
                    Assert.IsTrue(SceneManager.ActiveScene.RootObjects.Contains(go),
                        "SceneManager.ActiveScene.RootObjects.Contains(go)");
                    Assert.IsTrue(go.Transform.Parent == null, "go.Transform.Parent == null");
                }
                else
                {
                    Assert.IsFalse(SceneManager.ActiveScene.RootObjects.Contains(go),
                        "SceneManager.ActiveScene.RootObjects.Contains(go)");
                    Assert.IsFalse(go.Transform.Parent == null, "go.Transform.Parent == null");
                }

                Assert.AreEqual(Vector2.One, go.Transform.LocalPosition);
                Assert.AreEqual(Vector2.One * (i+1), go.Transform.Position);
            }
        }
    }
}