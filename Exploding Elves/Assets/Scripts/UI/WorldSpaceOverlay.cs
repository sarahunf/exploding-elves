using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceOverlay : MonoBehaviour
{
    [SerializeField] private Transform worldObjectToTrack;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Vector3 offset = Vector3.zero;
    
    private RectTransform rectTransform;
    private Camera mainCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (worldObjectToTrack == null || targetCanvas == null || overlayImage == null)
            return;

        var screenPos = mainCamera.WorldToScreenPoint(worldObjectToTrack.position + offset);
        if (screenPos.z < 0)
        {
            overlayImage.enabled = false;
            return;
        }

        overlayImage.enabled = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPos,
            targetCanvas.worldCamera,
            out var canvasPos
        );
        
        rectTransform.anchoredPosition = canvasPos;
    }
} 