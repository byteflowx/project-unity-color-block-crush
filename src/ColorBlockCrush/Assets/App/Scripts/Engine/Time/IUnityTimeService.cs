using System;

namespace App.Scripts.Engine.Time
{
    public interface IUnityTimeService
    {
        float DeltaTime { get; }
        float InGameTime { get; }
        DateTime UtcNow { get; }
        float RealtimeSinceStartup { get; }
    }
}