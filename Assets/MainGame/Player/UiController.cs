using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
public class UiController : MonoBehaviour
{
    public static UiController Singleton;

    [Header("Properties Display")]
    [SerializeField] public Image Health;
    [SerializeField] public Image Armor;

    public void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public void setCurerntlyLookingAt(Item item)
    {

    }
    public void setHealth(float health)
    {
        Health.fillAmount = health / 100f;
    }
}