using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
public class UiController : MonoBehaviour
{
    public static UiController Singleton;

    [Header("Properties Display")]
    [SerializeField] public Image Health;
    [SerializeField] public Image Armor;
    [SerializeField] public Image InteractionProgress;

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
    public void setInteractionProgress(float health)
    {
        InteractionProgress.fillAmount = health / 100f;
    }
    public void displayInteractIcon(bool display)
    {
        InteractionProgress.gameObject.SetActive(display);
    }
}