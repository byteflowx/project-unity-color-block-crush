using App.Scripts.Domain.Match;
using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public sealed class GameplayPalette : MonoBehaviour
    {
        [SerializeField] private Color _red = new Color(0.937f, 0.255f, 0.212f);
        [SerializeField] private Color _blue = new Color(0.231f, 0.557f, 0.918f);
        [SerializeField] private Color _yellow = new Color(1f, 0.788f, 0.239f);
        [SerializeField] private Color _green = new Color(0.298f, 0.733f, 0.373f);
        [SerializeField] private Color _purple = new Color(0.647f, 0.357f, 0.878f);
        [SerializeField] private float _coveredDarkening = 0.75f;
        [SerializeField] private float _coveredSaturation = 0.65f;

        public Color GetColor(BlockColor color)
        {
            switch (color)
            {
                case BlockColor.Red: return _red;
                case BlockColor.Blue: return _blue;
                case BlockColor.Yellow: return _yellow;
                case BlockColor.Green: return _green;
                default: return _purple;
            }
        }

        public Color GetCoveredColor(BlockColor color)
        {
            Color source = GetColor(color);
            Color.RGBToHSV(source, out float h, out float s, out float v);
            return Color.HSVToRGB(h, s * _coveredSaturation, v * _coveredDarkening);
        }
    }
}
