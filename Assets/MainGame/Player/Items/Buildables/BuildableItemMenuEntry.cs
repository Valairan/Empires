using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildableItemMenuEntry : MonoBehaviour
{
    public Item item;
    public Image buildableIcon;
    public TMP_Text buildableIconName;
    public GameObject itemCostParent;
    public ManufacturingCost cost;

    public void Start()
    {
        buildableIcon.sprite = item.ItemIcon;
        buildableIconName.text = item.ItemName;
    }

    public void toggleEntry()
    {

    }

    public void setPrices()
    {

    }
}
