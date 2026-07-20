using App.Scripts.Domain.Match;
using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public sealed class GameplayLayout : MonoBehaviour
    {
        private const float MIN_FRACTION = 0.01f;

        [SerializeField] private Camera _camera;
        [SerializeField] private float _fieldOfView = 55f;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _padding = 0.06f;
        [SerializeField] private float _bandGap = 0.7f;
        [SerializeField] private float _slotBandHeight = 1.6f;
        [SerializeField] private float _queueBandHeight = 4.2f;
        [SerializeField] private float _laneSpacing = 1.4f;

        private int _columns;
        private int _rows;
        private int _laneCount;
        private int _lastWidth;
        private int _lastHeight;
        private bool _configured;

        public float CellSize => _cellSize;
        public float LaneSpacing => _laneSpacing;
        public float BoardBottomY { get; private set; }
        public float SlotRowY { get; private set; }
        public float QueueTopY { get; private set; }

        public void Configure(int columns, int rows, int laneCount)
        {
            _columns = columns;
            _rows = rows;
            _laneCount = laneCount;
            _configured = true;
            _lastWidth = 0;
            _lastHeight = 0;

            Apply();
        }

        private void LateUpdate()
        {
            if (!_configured)
                return;

            if (Screen.width == _lastWidth && Screen.height == _lastHeight)
                return;

            Apply();
        }

        public Vector3 GetCellPosition(int column, int row)
            => new Vector3(ColumnToX(column), BoardBottomY + (row + 0.5f) * _cellSize, 0f);

        public Vector3 GetSlotPosition(int slotIndex)
            => new Vector3(SlotToX(slotIndex), SlotRowY, 0f);

        public Vector3 GetQueuePosition(int laneIndex, int position)
            => new Vector3(LaneToX(laneIndex), QueueTopY - position * _laneSpacing, 0f);

        public float ColumnToX(int column)
            => (column - (_columns - 1) * 0.5f) * _cellSize;

        private float SlotToX(int slotIndex)
            => (slotIndex - (SlotColumnCount - 1) * 0.5f) * _laneSpacing;

        private float LaneToX(int laneIndex)
            => (laneIndex - (_laneCount - 1) * 0.5f) * _laneSpacing;

        private static int SlotColumnCount => SlotBoard.SLOT_COUNT;

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            float boardHeight = _rows * _cellSize;
            float contentHeight = boardHeight + _bandGap + _slotBandHeight + _bandGap + _queueBandHeight;
            float contentWidth = Mathf.Max(_columns * _cellSize, Mathf.Max(_laneCount, SlotColumnCount) * _laneSpacing);

            Rect safeArea = Screen.safeArea;
            float safeHeightFraction = Mathf.Clamp(safeArea.height / Screen.height, MIN_FRACTION, 1f);
            float safeWidthFraction = Mathf.Clamp(safeArea.width / Screen.width, MIN_FRACTION, 1f);
            float screenAspect = Mathf.Max((float)Screen.width / Screen.height, MIN_FRACTION);

            float halfHeightFromHeight = contentHeight * 0.5f / safeHeightFraction;
            float halfHeightFromWidth = contentWidth * 0.5f / safeWidthFraction / screenAspect;
            float halfHeight = Mathf.Max(halfHeightFromHeight, halfHeightFromWidth) * (1f + _padding);

            _camera.orthographic = false;
            _camera.fieldOfView = _fieldOfView;

            float distance = halfHeight / Mathf.Tan(_fieldOfView * 0.5f * Mathf.Deg2Rad);

            float contentTop = boardHeight;
            float contentBottom = -(_bandGap + _slotBandHeight + _bandGap + _queueBandHeight);
            float contentCenterY = (contentTop + contentBottom) * 0.5f;

            float safeCenterNormalized = (safeArea.y + safeArea.height * 0.5f) / Screen.height;
            float cameraY = contentCenterY - (safeCenterNormalized - 0.5f) * 2f * halfHeight;

            _camera.transform.position = new Vector3(0f, cameraY, -distance);
            _camera.transform.rotation = Quaternion.identity;
            _camera.nearClipPlane = Mathf.Max(0.1f, distance - 20f);
            _camera.farClipPlane = distance + 40f;

            BoardBottomY = 0f;
            SlotRowY = -(_bandGap + _slotBandHeight * 0.5f);
            QueueTopY = -(_bandGap + _slotBandHeight + _bandGap);
        }
    }
}
