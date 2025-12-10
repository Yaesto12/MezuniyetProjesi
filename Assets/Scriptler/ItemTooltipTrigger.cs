using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string itemName;
    public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // PauseMenuInfo scriptinde TooltipPanel referansý olmalý
        PauseMenuInfo.Instance.ShowTooltip(itemName, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PauseMenuInfo.Instance.HideTooltip();
    }
}