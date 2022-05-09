using Engine;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    /// <summary>
    /// Contains templates for quick creation of prefab game objects
    /// </summary>
    public static partial class Template
    {
        /// <summary>
        /// Creates a new game object with a component of the given type.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the component.</returns>
        public static (GameObject go, T component) Make<T>(string name = null, Scene scene = null)
            where T : Component, new()
        {
            return Make<T>(Vector2.Zero, 0f, Vector2.One, name, scene);
        }

        /// <summary>
        /// Creates a new game object with a component of the given type and initialized the transform with the given position, rotation and scale.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the component.</returns>
        public static (GameObject go, T component) Make<T>(Vector2 position, float rotation, Vector2 scale, string name = null, Scene scene = null) 
            where T : Component, new()
        {
            if (scene == null)
            {
                scene = SceneManager.ScopedScene;
            }

            var go = new GameObject(name, scene);
            go.Transform.LocalPosition = position;
            go.Transform.LocalRotation = rotation;
            go.Transform.LossyScale = scale;

            var t0 = go.AddComponent<T>();

            return (go, t0);
        }

        /// <summary>
        /// Creates a new game object with components of the given type.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1) Make<T0, T1>(string name = null,
            Scene scene = null)
            where T0 : Component, new()
            where T1 : Component, new()
        {
            return Make<T0, T1>(Vector2.Zero, 0f, Vector2.One, name, scene);
        }

        /// <summary>
        /// Creates a new game object with components of the given type and initialized the transform with the given position, rotation and scale.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1) Make<T0, T1>(Vector2 position, float rotation, Vector2 scale, string name = null, Scene scene = null) 
            where T0 : Component, new() 
            where T1 : Component, new()
        {
            if (scene == null)
            {
                scene = SceneManager.ScopedScene;
            }

            var go = new GameObject(name, scene);
            go.Transform.LocalPosition = position;
            go.Transform.LocalRotation = rotation;
            go.Transform.LossyScale = scale;

            var t0 = go.AddComponent<T0>();
            var t1 = go.AddComponent<T1>();

            return (go, t0, t1);
        }

        /// <summary>
        /// Creates a new game object with components of the given type.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1, T2 component2) Make<T0, T1, T2>(
            string name = null, Scene scene = null)
            where T0 : Component, new()
            where T1 : Component, new()
            where T2 : Component, new()
        {
            return Make<T0, T1, T2>(Vector2.Zero, 0f, Vector2.One, name, scene);
        }

        /// <summary>
        /// Creates a new game object with components of the given type and initialized the transform with the given position, rotation and scale.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1, T2 component2) Make<T0, T1, T2>(Vector2 position, float rotation, Vector2 scale, string name = null, Scene scene = null) 
            where T0 : Component, new() 
            where T1 : Component, new()
            where T2 : Component, new()
        {
            if (scene == null)
            {
                scene = SceneManager.ScopedScene;
            }

            var go = new GameObject(name, scene);
            go.Transform.LocalPosition = position;
            go.Transform.LocalRotation = rotation;
            go.Transform.LossyScale = scale;

            var t0 = go.AddComponent<T0>();
            var t1 = go.AddComponent<T1>();
            var t2 = go.AddComponent<T2>();

            return (go, t0, t1, t2);
        }

        /// <summary>
        /// Creates a new game object with components of the given type.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1, T2 component2, T3 component3)
            Make<T0, T1, T2, T3>(string name = null, Scene scene = null)
            where T0 : Component, new()
            where T1 : Component, new()
            where T2 : Component, new()
            where T3 : Component, new()
        {
            return Make<T0, T1, T2, T3>(Vector2.Zero, 0f, Vector2.One, name, scene);
        }

        /// <summary>
        /// Creates a new game object with components of the given type and initialized the transform with the given position, rotation and scale.
        /// </summary>
        /// <returns>Returns a tuple containing the created game object and the components.</returns>
        public static (GameObject go, T0 component0, T1 component1, T2 component2, T3 component3) Make<T0, T1, T2, T3>(Vector2 position, float rotation, Vector2 scale, string name = null, Scene scene = null) 
            where T0 : Component, new() 
            where T1 : Component, new()
            where T2 : Component, new()
            where T3 : Component, new()
        {
            if (scene == null)
            {
                scene = SceneManager.ScopedScene;
            }

            var go = new GameObject(name, scene);
            go.Transform.LocalPosition = position;
            go.Transform.LocalRotation = rotation;
            go.Transform.LossyScale = scale;

            var t0 = go.AddComponent<T0>();
            var t1 = go.AddComponent<T1>();
            var t2 = go.AddComponent<T2>();
            var t3 = go.AddComponent<T3>();

            return (go, t0, t1, t2, t3);
        }
    }
}

