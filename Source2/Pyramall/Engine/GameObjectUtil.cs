using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace Engine
{
    public static class GameObjectUtil
    {
        [NotNull]
        private static readonly Queue<GameObject> goQueue = new Queue<GameObject>();
        
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
