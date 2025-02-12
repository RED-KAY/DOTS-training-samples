using System;
using System.Collections.Generic;

namespace Utilities.GenericObjectPool
{
    public static class GenericObjectPool
    {
        private static Dictionary<Type, object> s_GenericPool = new Dictionary<Type, object>();

        public static T Get<T>()
        {
            if (s_GenericPool.TryGetValue(typeof(T), out var value))
            {
                Stack<T> stack = value as Stack<T>;
                if (stack.Count > 0)
                {
                    return stack.Pop();
                }
            }
            if (typeof(T).IsArray)
            {
                return (T)Activator.CreateInstance(typeof(T), 0);
            }
            return Activator.CreateInstance<T>();
        }

        public static void Return<T>(T obj)
        {
            if (s_GenericPool.TryGetValue(typeof(T), out var value))
            {
                Stack<T> stack = value as Stack<T>;
                stack.Push(obj);
            }
            else
            {
                Stack<T> stack2 = new Stack<T>();
                stack2.Push(obj);
                s_GenericPool.Add(typeof(T), stack2);
            }
        }
    }
}
