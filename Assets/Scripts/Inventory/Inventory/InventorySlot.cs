using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Componentes")]
    public Image icon;
    public TMP_Text amountText;
    public InventoryUI uiParent;

    [HideInInspector] public int myIndex;

    private CanvasGroup canvasGroup;
    private Vector3 originalIconLocalPosition;
    private Image ghostImage;
    public float ghostSize = 64f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void UpdateSlot(string itemName, int amount, Sprite itemSprite)
    {
        icon.sprite = itemSprite;
        icon.enabled = itemSprite != null;
        amountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public void ResetIconPosition()
    {
        icon.transform.localPosition = Vector3.zero;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        DestroyGhost();
    }

    void CreateGhost()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        GameObject ghost = new GameObject("DragGhost");
        ghost.transform.SetParent(rootCanvas.transform);
        ghost.transform.SetAsLastSibling();

        ghostImage = ghost.AddComponent<Image>();
        ghostImage.sprite = icon.sprite;
        ghostImage.raycastTarget = false;

        RectTransform ghostRect = ghostImage.GetComponent<RectTransform>();
        ghostRect.sizeDelta = new Vector2(ghostSize, ghostSize);
        ghostRect.position = icon.transform.position;
    }

    void DestroyGhost()
    {
        if (ghostImage != null)
        {
            Destroy(ghostImage.gameObject);
            ghostImage = null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (icon.sprite == null || !icon.enabled) return;
        originalIconLocalPosition = icon.transform.localPosition;
        CreateGhost();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostImage == null) return;
        ghostImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        icon.transform.localPosition = originalIconLocalPosition;
        DestroyGhost();

        if (icon.sprite == null || !icon.enabled) return;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        InventorySlot targetSlot = null;
        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent(out InventorySlot slot) && slot != this)
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot != null)
            uiParent.RequestSwap(myIndex, targetSlot.myIndex);
        else if (results.Count == 0)
            uiParent.DropItem(myIndex);
    }
}