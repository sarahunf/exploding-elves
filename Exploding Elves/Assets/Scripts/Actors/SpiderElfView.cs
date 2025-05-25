using System;
using UnityEngine;
using System.Collections;

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

        public event Action OnAttackFinished;

        public void SetColor(Color color)
        {
            if (spiderRenderer != null)
            {
                //0 is body
                //1 is legs
                spiderRenderer.materials[1].color = color; 
            }
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