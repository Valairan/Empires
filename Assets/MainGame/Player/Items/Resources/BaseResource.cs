using UnityEngine;

[CreateAssetMenu(fileName = "BaseResource", menuName = "Items/New Base Resource")]
public class BaseResource : Item
{
    public GameObject[] drops;
    public int[] dropsHowMany;
}
