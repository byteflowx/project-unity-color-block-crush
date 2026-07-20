using System;
using UnityEngine;

namespace App.Scripts.Core.Common
{
    public static class Log
    {
        public static void Info(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
             Debug.Log(message);
#endif
        }

        public static void Warning(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(message);
#endif
        }

        public static void Error(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(message);
#endif
        }

        public static void Exception(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}