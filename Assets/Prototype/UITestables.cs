using UnityEngine;

public class UITestables : MonoBehaviour
{
    public PlayerController controller;
    public Weapon weapon;
    public void damageTheCurrentPlayer()
    {
        controller.takeDamage(new DamageContext() { damager = weapon });
        Debug.Log("Potato");
    }
}
