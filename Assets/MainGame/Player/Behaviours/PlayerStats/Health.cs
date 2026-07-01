using System;
using Unity.Netcode;
using UnityEngine;

public class Health : RangeStat
{

    // Make health a NetworkVariable to sync it

    public override void init()
    {
    }

    public override void decreaseAmount(float amount)
    {
        currentAmount.Value -= amount;
    }
    public override void increaseAmount(float amount)
    {
        currentAmount.Value -= amount;
    }

}

public class RangeStat : NetworkBehaviour
{
    public NetworkVariable<float> currentAmount = new NetworkVariable<float>(100f);
    public float maximumAmount;
    public Action atZero;
    public Action atFull;

    public virtual void init()
    {

    }

    public virtual void setAmount()
    {

    }
    public virtual void decreaseAmount(float amount)
    {

    }
    public virtual void increaseAmount(float amount)
    {

    }
}