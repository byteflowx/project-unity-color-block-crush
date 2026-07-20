using App.Scripts.Core.Enums;

namespace App.Scripts.Engine.Audio
{
    public sealed class AudioService : IAudioService
    {
        public void PlayUiSound(UiSoundType soundType)
        {
            string sfxName = GetSfxName(soundType);
            
            if (string.IsNullOrEmpty(sfxName))
                return;

            PlaySfx(sfxName);
        }

        public void PlaySfx(string soundKey)
        {
            if (SFXManager.Main == null)
                return;
            
            SFXManager.Main.PlayFromSFXObjectLibrary(soundKey);
        }

        public void PlayMusic(string trackKey)
        {
            if (MusicManager.Main == null) 
                return;
            
            MusicManager.Main.PlayFromLibrary(trackKey);
        }

        public void StopMusic()
        {
             if (MusicManager.Main == null) 
                 return;
             
             MusicManager.Main.Stop();
        }

        private string GetSfxName(UiSoundType soundType)
        {
            return soundType switch
            {
                UiSoundType.WindowOpen => "PanelOpen",
                UiSoundType.WindowClose => "PanelClose",
                UiSoundType.ButtonClick => "ButtonClick",
                UiSoundType.Purchase => "UpgradeClick",
                _ => null
            };
        }
    }
}