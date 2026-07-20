namespace App.Scripts.Engine.Haptics
{
    public interface IHapticsService
    {
        bool IsEnabled { get; set; }

        void Play(HapticType type);
    }
}
