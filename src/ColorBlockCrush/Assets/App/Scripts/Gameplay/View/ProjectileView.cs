using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trail;

        private Vector3 _origin;
        private Vector3 _destination;
        private float _duration;
        private float _elapsed;

        public bool IsFinished { get; private set; }

        public void Launch(Vector3 origin, Vector3 destination, float duration)
        {
            _origin = origin;
            _destination = destination;
            _duration = Mathf.Max(duration, 0.01f);
            _elapsed = 0f;
            IsFinished = false;

            transform.position = origin;
            transform.up = (destination - origin).normalized;

            if (_trail != null)
                _trail.Clear();
        }

        public void Advance(float deltaTime)
        {
            if (IsFinished)
                return;

            _elapsed += deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            transform.position = Vector3.Lerp(_origin, _destination, progress);

            if (progress >= 1f)
                IsFinished = true;
        }
    }
}
