using System;
using App.Scripts.Engine.Time;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace App.Scripts.Engine.Haptics
{
    public sealed class HapticsService : IHapticsService, IDisposable
    {
        private const float MIN_INTERVAL_SECONDS = 0.125f;

        private readonly IUnityTimeService _timeService;

        private float _lastPlayTime = float.NegativeInfinity;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _vibrator;
        private bool _supportsAmplitude;
#endif

        public HapticsService(IUnityTimeService timeService)
        {
            _timeService = timeService;
            IsEnabled = true;

            Prepare();
        }

        public bool IsEnabled { get; set; }

        public void Play(HapticType type)
        {
            if (!IsEnabled)
                return;

            float now = _timeService.RealtimeSinceStartup;

            if (type == HapticType.Soft && now - _lastPlayTime < MIN_INTERVAL_SECONDS)
                return;

            _lastPlayTime = now;

            PlayNative(type);
        }

        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_vibrator != null)
            {
                _vibrator.Dispose();
                _vibrator = null;
            }
#endif
        }

        private void Prepare()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IOSHapticsBridge.Prepare();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }

            using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                _supportsAmplitude = version.GetStatic<int>("SDK_INT") >= 26;
            }
#endif
        }

        private void PlayNative(HapticType type)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IOSHapticsBridge.Play((int)type);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (_vibrator == null)
                return;

            GetAndroidPattern(type, out long milliseconds, out int amplitude);

            if (!_supportsAmplitude)
            {
                _vibrator.Call("vibrate", milliseconds);
                return;
            }

            using (AndroidJavaClass effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
            using (AndroidJavaObject effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude))
            {
                _vibrator.Call("vibrate", effect);
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void GetAndroidPattern(HapticType type, out long milliseconds, out int amplitude)
        {
            switch (type)
            {
                case HapticType.Heavy:
                    milliseconds = 55L;
                    amplitude = 255;
                    return;
                case HapticType.Soft:
                    milliseconds = 22L;
                    amplitude = 70;
                    return;
                default:
                    milliseconds = 35L;
                    amplitude = 140;
                    return;
            }
        }
#endif
    }
}
