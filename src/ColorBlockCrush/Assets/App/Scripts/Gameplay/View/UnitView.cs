using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.Gameplay.View
{
    public sealed class UnitView : MonoBehaviour
    {
        private const string CHARGES_LABEL_NAME = "unit-label__charges";

        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshRenderer _bodyRenderer;
        [SerializeField] private MeshRenderer _barrelRenderer;
        [SerializeField] private UIDocument _labelDocument;
        [SerializeField] private Transform _bodyTransform;
        [SerializeField] private Color _availableLabelColor = Color.white;
        [SerializeField] private Color _queuedLabelColor = new Color(1f, 1f, 1f, 0.4f);

        private MaterialPropertyBlock _propertyBlock;
        private Label _chargesLabel;
        private Vector3 _rootBaseScale;
        private Vector3 _bodyBaseScale;
        private int _charges;
        private bool _isAvailable;

        public int UnitId { get; private set; }
        public int LaneIndex { get; private set; }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _rootBaseScale = transform.localScale;
            _bodyBaseScale = _bodyTransform.localScale;
        }

        private void OnDisable()
        {
            _chargesLabel = null;
        }

        public void Bind(int unitId, int laneIndex, Color color, int charges)
        {
            UnitId = unitId;
            LaneIndex = laneIndex;

            ApplyColor(_bodyRenderer, color);
            ApplyColor(_barrelRenderer, color);
            SetCharges(charges);
        }

        public void SetCharges(int charges)
        {
            _charges = charges;
            ApplyLabel();
        }

        public void SetAvailable(bool available)
        {
            _isAvailable = available;
            ApplyLabel();
        }

        public void PlayFire()
        {
            Anim.PunchScale(_bodyTransform, 0.18f, 0.12f);
        }

        public void PlayLanding()
        {
            Anim.PunchScale(_bodyTransform, 0.25f, 0.22f);
        }

        public void ResetView()
        {
            Anim.Kill(transform);
            Anim.Kill(_bodyTransform);
            transform.localScale = _rootBaseScale;
            _bodyTransform.localScale = _bodyBaseScale;
        }

        private void ApplyLabel()
        {
            Label label = ResolveLabel();

            if (label == null)
                return;

            label.text = _charges.ToString();
            label.style.color = _isAvailable ? _availableLabelColor : _queuedLabelColor;
        }

        private Label ResolveLabel()
        {
            if (_chargesLabel != null)
                return _chargesLabel;

            if (_labelDocument == null)
                return null;

            VisualElement root = _labelDocument.rootVisualElement;

            if (root == null)
                return null;

            _chargesLabel = root.Q<Label>(CHARGES_LABEL_NAME);
            return _chargesLabel;
        }

        private void ApplyColor(MeshRenderer target, Color color)
        {
            target.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BASE_COLOR_ID, color);
            target.SetPropertyBlock(_propertyBlock);
        }
    }
}
