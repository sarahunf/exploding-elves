using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Actors
{
    public class SpiderElfView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign the Renderer (MeshRenderer or SkinnedMeshRenderer) of the spider model.")]
        public Renderer spiderRenderer;
        [Tooltip("Assign the Animator component controlling spider animations.")]
        public Animator spiderAnimator;
        
        // Animator parameter names
        [Header("Animation Parameters")]
        public string attackTrigger = "Attack";
        public string walkingBool = "Walking";

        private Vector3 normalScale;
        private const float SPAWN_SCALE = 0.5f;
        private const float SCALE_DURATION = 0.3f;

        private void Awake()
        {
            normalScale = transform.localScale;
        }

        public void SetBodyColor(Color color)
        {
            if (spiderRenderer != null)
            {
                //0 is body
                spiderRenderer.materials[0].color = color; 
            }
        }
        
        public void SetHighlightColor(Color color)
        {
            if (spiderRenderer != null)
            {
                //1 is legs
                spiderRenderer.materials[1].color = color; 
            }
        }

        public void SetEmission(bool isSpawning, Color emissionColor)
        {
            if (spiderRenderer != null)
            {
                Material bodyMaterial = spiderRenderer.materials[0];
                if (isSpawning)
                {
                    bodyMaterial.EnableKeyword("_EMISSION");
                    bodyMaterial.SetColor("_EmissionColor", emissionColor);
                }
                else
                {
                    bodyMaterial.DisableKeyword("_EMISSION");
                }
            }
        }

        public void SetScale(bool canReplicate)
        {
            Vector3 targetScale = canReplicate ? normalScale : normalScale * SPAWN_SCALE;
            transform.DOScale(targetScale, SCALE_DURATION).SetEase(Ease.OutBack);
        }
        
        public void SetWalking(bool isWalking)
        {
            if (spiderAnimator != null)
            {
                spiderAnimator.SetBool(walkingBool, isWalking);
            }
        }
        
        public void AttackAndDestroy(Action OnFinished = null)
        {
            if (spiderAnimator != null)
            {
                spiderAnimator.SetTrigger(attackTrigger);
                StartCoroutine(DestroyAfterAttack(OnFinished));
            }
        }

        private IEnumerator DestroyAfterAttack(Action OnFinished = null)
        {
            if (spiderAnimator != null)
            {
                float attackLength = 0.5f;
                foreach (var clip in spiderAnimator.runtimeAnimatorController.animationClips)
                {
                    if (clip.name == attackTrigger)
                    {
                        attackLength = clip.length;
                        break;
                    }
                }
                yield return new WaitForSeconds(attackLength);
            }
            OnFinished?.Invoke();
        }
    }
} 