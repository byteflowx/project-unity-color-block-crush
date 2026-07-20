using App.Scripts.Core.Enums;

namespace App.Scripts.Engine.Audio
{
    public interface IAudioService
    {
        void PlayUiSound(UiSoundType soundType);
        void PlaySfx(string soundKey);
        void PlayMusic(string trackKey);
        void StopMusic();
    }
}