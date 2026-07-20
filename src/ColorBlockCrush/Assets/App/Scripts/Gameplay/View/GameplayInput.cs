using UnityEngine;
using UnityEngine.InputSystem;

namespace App.Scripts.Gameplay.View
{
    public sealed class GameplayInput : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public bool TryGetTapWorldPosition(out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;

            if (!TryGetTapScreenPosition(out Vector2 screenPosition))
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (Mathf.Approximately(ray.direction.z, 0f))
                return false;

            float distance = -ray.origin.z / ray.direction.z;

            if (distance < 0f)
                return false;

            Vector3 point = ray.GetPoint(distance);
            worldPosition = new Vector3(point.x, point.y, 0f);
            return true;
        }

        private static bool TryGetTapScreenPosition(out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

            Touchscreen touchscreen = Touchscreen.current;

            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            Mouse mouse = Mouse.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            return false;
        }
    }
}
