using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Utility class providing useful operations for <see cref="GameObject"/>s.
    /// </summary>
    public static class GameObjectUtil
    {
        /// <summary>
        /// Cached queue for game objects to prevent repeated memory allocation.
        /// </summary>
        [NotNull]
        private static readonly Queue<GameObject> goQueue = new Queue<GameObject>();
        
        /// <summary>
        /// Gets a <see cref="Component"/> of the specified type in the given <see cref="GameObject"/> or any of its children (recursive).
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.</exception>
        /// <seealso cref="GetComponentsInChildren{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInChildren{T}(GameObject, IList{T})"/>
        [CanBeNull]
        public static T GetComponentInChildren<T>([NotNull]this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            goQueue.Clear();
            goQueue.Enqueue(gameObject);

            while (goQueue.Count > 0)
            {
                var nextGo = goQueue.Dequeue();
                var comp = nextGo.GetComponent<T>();

                if (comp != null)
                {
                    goQueue.Clear();
                    return comp;
                }

                for (var i = 0; i < nextGo.Transform.ChildCount; i++)
                {
                    goQueue.Enqueue(nextGo.Transform.GetChild(i).Owner);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Gets all <see cref="Component"/>s of the specified type in the given <see cref="GameObject"/> and all of its children (recursive).<br/>
        /// Consider using <see cref="GetComponentsInChildren{T}(GameObject, IList{T})"/> instead to prevent internal memory allocation.
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.</exception>
        /// <seealso cref="GetComponentInChildren{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInChildren{T}(GameObject, IList{T})"/>
        [NotNull, ItemNotNull]
        public static T[] GetComponentsInChildren<T>([NotNull]this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            var result = new List<T>();

            goQueue.Clear();
            goQueue.Enqueue(gameObject);

            while (goQueue.Count > 0)
            {
                var nextGo = goQueue.Dequeue();
                nextGo.GetComponents(result);

                for (var i = 0; i < nextGo.Transform.ChildCount; i++)
                {
                    goQueue.Enqueue(nextGo.Transform.GetChild(i).Owner);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets all <see cref="Component"/>s of the specified type in the given <see cref="GameObject"/> and all of its children (recursive).
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.<br/>The items cannot be null.</exception>
        /// <seealso cref="GetComponentInChildren{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInChildren{T}(GameObject)"/>
        public static void GetComponentsInChildren<T>([NotNull]this GameObject gameObject, [NotNull]IList<T> items) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            goQueue.Clear();
            goQueue.Enqueue(gameObject);

            while (goQueue.Count > 0)
            {
                var nextGo = goQueue.Dequeue();
                nextGo.GetComponents(items);

                for (var i = 0; i < nextGo.Transform.ChildCount; i++)
                {
                    goQueue.Enqueue(nextGo.Transform.GetChild(i).Owner);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Component"/> of the specified type in the given <see cref="GameObject"/> or its parents (recursive).
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.</exception>
        /// <seealso cref="GetComponentsInParents{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInParents{T}(GameObject, IList{T})"/>
        [CanBeNull]
        public static T GetComponentInParents<T>([NotNull] this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            T component;
            var t = gameObject.Transform;

            do
            {
                component = t.Owner.GetComponent<T>();
                t = t.Parent;
            } while (component == null && t != null);

            return component;
        }

        /// <summary>
        /// Gets all <see cref="Component"/>s of the specified type in the given <see cref="GameObject"/> and its parents (recursive).<br/>
        /// Consider using <see cref="GetComponentsInParents{T}(GameObject, IList{T})"/> instead to prevent internal memory allocation.
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.</exception>
        /// <seealso cref="GetComponentInParents{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInParents{T}(GameObject, IList{T})"/>
        [NotNull, ItemNotNull]
        public static T[] GetComponentsInParents<T>([NotNull] this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            var result = new List<T>();
            var t = gameObject.Transform;

            do
            {
                t.Owner.GetComponents(result);
                t = t.Parent;
            } while (t != null);

            return result.ToArray();
        }
        
        /// <summary>
        /// Gets all <see cref="Component"/>s of the specified type in the given <see cref="GameObject"/> and its parents (recursive).
        /// </summary>
        /// <exception cref="ArgumentNullException">The game object cannot be null.<br/>The items cannot be null.</exception>
        /// <seealso cref="GetComponentInParents{T}(GameObject)"/>
        /// <seealso cref="GetComponentsInParents{T}(GameObject)"/>
        public static void GetComponentsInParents<T>([NotNull] this GameObject gameObject, [NotNull]IList<T> items) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            
            var t = gameObject.Transform;

            do
            {
                t.Owner.GetComponents(items);
                t = t.Parent;
            } while (t != null);
        }
    }
}
