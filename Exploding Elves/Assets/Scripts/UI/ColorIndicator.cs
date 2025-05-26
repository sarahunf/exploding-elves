using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UI
{
    public class ColorIndicator : MonoBehaviour
    {
        [SerializeField] private Image indicatorImage;
        [SerializeField] private bool shouldAnimate;
        [SerializeField] private float animationDuration = 0.5f;
        
        private Sequence animationSequence;
        private bool isAnimating;

        private void Awake()
        {
            if (indicatorImage == null)
            {
                indicatorImage = GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            StartAnimation();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public void SetColor(Color color)
        {
            if (indicatorImage != null)
            {
                indicatorImage.color = color;
            }
        }
        

        private void StartAnimation()
        {
            if (!shouldAnimate) return;
            
            if (!isAnimating)
            {
                isAnimating = true;
                CreateAnimationSequence();
            }
            else
            {
                StopAnimation();
            }
        }

        private void CreateAnimationSequence()
        {
            StopAnimation();

            animationSequence = DOTween.Sequence();
            animationSequence.Append(transform.DOLocalRotate(new Vector3(0, 360, 0), animationDuration, RotateMode.FastBeyond360))
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        private void StopAnimation()
        {
            if (animationSequence != null)
            {
                animationSequence.Kill();
                animationSequence = null;
            }
            isAnimating = false;
            transform.localPosition = Vector3.zero;
        }
    }
} 