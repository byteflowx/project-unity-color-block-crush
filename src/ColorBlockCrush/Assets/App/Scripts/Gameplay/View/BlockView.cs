using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public sealed class BlockView : MonoBehaviour
    {
        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Transform _topTransform;
        [SerializeField] private MeshRenderer _topRenderer;
        [SerializeField] private GameObject _coveredObject;
        [SerializeField] private MeshRenderer _coveredRenderer;

        private MaterialPropertyBlock _propertyBlock;
        private Vector3 _rootBaseScale;
        private Vector3 _topBaseScale;

        public int CellId { get; private set; }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _rootBaseScale = transform.localScale;
            _topBaseScale = _topTransform.localScale;
        }

        public void Bind(int cellId)
        {
            CellId = cellId;
        }

        public void SetTopColor(Color color)
        {
            ApplyColor(_topRenderer, color);
        }

        public void SetCovered(bool visible, Color color)
        {
            _coveredObject.SetActive(visible);

            if (visible)
                ApplyColor(_coveredRenderer, color);
        }

        public void PlayHit()
        {
            Anim.PunchScale(_topTransform, 0.2f, 0.16f);
        }

        public void PlaySpawn()
        {
            Anim.PopIn(transform, 0.18f);
        }

        public void ResetView()
        {
            Anim.Kill(transform);
            Anim.Kill(_topTransform);
            transform.localScale = _rootBaseScale;
            _topTransform.localScale = _topBaseScale;
        }

        private void ApplyColor(MeshRenderer target, Color color)
        {
            target.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BASE_COLOR_ID, color);
            target.SetPropertyBlock(_propertyBlock);
        }
    }
}
