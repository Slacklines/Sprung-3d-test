using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public RectTransform seperator;

    private Vector3 targetScale = new Vector3 (.2f, .03f, 1f);

    // Update is called once per frame
    void Update()
    {
        seperator.localScale = Vector3.Lerp (seperator.localScale, targetScale, Time.deltaTime * 4f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = new Vector3 (.4f, .03f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = new Vector3 (.2f, .03f, 1f);
    }
}
