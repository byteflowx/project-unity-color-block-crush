using System.Runtime.InteropServices;

namespace App.Scripts.Engine.Haptics
{
    internal static class IOSHapticsBridge
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _CBCHapticsPrepare();

        [DllImport("__Internal")]
        private static extern void _CBCHapticsPlay(int type);

        public static void Prepare() => _CBCHapticsPrepare();

        public static void Play(int type) => _CBCHapticsPlay(type);
#else
        public static void Prepare()
        {
        }

        public static void Play(int type)
        {
        }
#endif
    }
}
