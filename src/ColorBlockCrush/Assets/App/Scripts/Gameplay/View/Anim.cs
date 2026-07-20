using DG.Tweening;
using UnityEngine;

namespace App.Scripts.Gameplay.View
{
    public static class Anim
    {
        public static void Init()
        {
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            DOTween.SetTweensCapacity(500, 100);
        }

        public static void Kill(Transform target)
        {
            DOTween.Kill(target, true);
        }

        public static void MoveTo(Transform target, Vector3 position, float duration)
        {
            DOTween.Kill(target, true);
            target.DOMove(position, duration).SetEase(Ease.OutQuad).SetLink(target.gameObject);
        }

        public static void JumpTo(Transform target, Vector3 position, float duration)
        {
            DOTween.Kill(target, true);
            target.DOJump(position, 0.9f, 1, duration).SetEase(Ease.OutQuad).SetLink(target.gameObject);
        }

        public static void FallTo(Transform target, Vector3 position, float duration)
        {
            DOTween.Kill(target, true);
            target.DOMove(position, duration).SetEase(Ease.OutBounce).SetLink(target.gameObject);
        }

        public static void PunchScale(Transform target, float strength, float duration)
        {
            target.DOPunchScale(Vector3.one * strength, duration, 6, 0.8f).SetLink(target.gameObject);
        }

        public static void SlideOut(Transform target, float distance, float duration)
        {
            DOTween.Kill(target, true);
            target.DOMoveX(target.position.x + distance, duration).SetEase(Ease.InBack).SetLink(target.gameObject);
        }

        public static void PopIn(Transform target, float duration)
        {
            target.localScale = Vector3.zero;
            target.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetLink(target.gameObject);
        }
    }
}
