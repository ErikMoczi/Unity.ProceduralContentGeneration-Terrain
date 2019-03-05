using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PCG.Terrain.Common.Helpers
{
    public static class NoAllocHelpers
    {
        private const string UnityNoAllocHelpersTypeName = "UnityEngine.NoAllocHelpers";
        private const string ExtractArrayFromListTName = "ExtractArrayFromListT";
        private const string ResizeListName = "ResizeList";

        private static readonly Dictionary<Type, Delegate> ExtractArrayFromListTDelegates =
            new Dictionary<Type, Delegate>();

        private static readonly Dictionary<Type, Delegate> ResizeListDelegates = new Dictionary<Type, Delegate>();

        public static T[] ExtractArrayFromListT<T>(List<T> list)
        {
            if (!ExtractArrayFromListTDelegates.TryGetValue(typeof(T), out var obj))
            {
                obj = ExtractArrayFromListTDelegates[typeof(T)] = Delegate.CreateDelegate(
                    typeof(Func<List<T>, T[]>),
                    ExtractArrayFromListTInfo<T>()
                );
            }

            var func = (Func<List<T>, T[]>) obj;
            return func.Invoke(list);
        }

        public static void ResizeList<T>(List<T> list, int size)
        {
            if (!ResizeListDelegates.TryGetValue(typeof(T), out var obj))
            {
                obj = ResizeListDelegates[typeof(T)] = Delegate.CreateDelegate(
                    typeof(Action<List<T>, int>),
                    ResizeListInfo<T>()
                );
            }

            var action = (Action<List<T>, int>) obj;
            action.Invoke(list, size);
        }

        private static MethodInfo ExtractArrayFromListTInfo<T>()
        {
            return MakeMethod<T>(UnityNoAllocHelpers(),
#if DEBUG
                UnityNoAllocHelpersTypeName,
#endif
                ExtractArrayFromListTName,
                BindingFlags.Static | BindingFlags.Public);
        }

        private static MethodInfo ResizeListInfo<T>()
        {
            return MakeMethod<T>(UnityNoAllocHelpers(),
#if DEBUG
                UnityNoAllocHelpersTypeName,
#endif
                ResizeListName,
                BindingFlags.Static | BindingFlags.Public);
        }

        private static Type UnityNoAllocHelpers()
        {
            var assembly = Assembly.GetAssembly(typeof(Mesh));
            return assembly.GetType(UnityNoAllocHelpersTypeName);
        }

        private static MethodInfo MakeMethod<T>(Type type,
#if DEBUG
            string className,
#endif
            string methodName,
            BindingFlags bindingFlags)
        {
            var methodInfo = type.GetMethod(methodName, bindingFlags);
#if DEBUG
            if (methodInfo == null)
            {
                throw new MissingMethodException(className, methodName);
            }
#endif
            // ReSharper disable once PossibleNullReferenceException
            return methodInfo.MakeGenericMethod(typeof(T));
        }
    }
}