using UnityEngine;

[CreateAssetMenu(fileName = "BaseResource", menuName = "Empires/Resource/New Base Resource")]
public class BaseResource : Item
{
    public GameObject[] drops;
    public int[] dropsHowMany;
}
