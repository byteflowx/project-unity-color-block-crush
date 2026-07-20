using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public sealed class SlotMarkerView : MonoBehaviour
    {
        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Color _emptyColor = new Color(0.06f, 0.15f, 0.31f);
        [SerializeField] private Color _occupiedColor = new Color(0.11f, 0.24f, 0.45f);

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            SetOccupied(false);
        }

        public void SetOccupied(bool occupied)
        {
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BASE_COLOR_ID, occupied ? _occupiedColor : _emptyColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
