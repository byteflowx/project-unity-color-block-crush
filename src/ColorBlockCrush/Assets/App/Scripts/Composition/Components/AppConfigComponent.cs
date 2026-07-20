using UnityEngine;

namespace App.Scripts.Composition.Components
{
    public sealed class AppConfigComponent : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate;
        [SerializeField] private int _vSyncCount;
        [SerializeField] private bool _runInBackground;
        
        private void Start()
        {
            QualitySettings.vSyncCount = _vSyncCount;
            Application.targetFrameRate = _targetFrameRate;
            Application.runInBackground = _runInBackground;
        }
    }
}
