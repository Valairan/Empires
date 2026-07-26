using System;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class OreResourceBehaviour : BaseResourceBehaviour, IScatteredDecoration
{
    [SerializeField] private string hitEffectName = "OreHit";

    public GameObject decorationObject => throw new NotImplementedException();

    public override void takeDamage(DamageContext ctx)
    {
        float damage = ctx.damage;
        DamageOre_ServerRpc(ctx.damage, ctx.type, ctx.hitpoint, ctx.hitnormal, xCoordinate, yCoordinate);
    }
    public override void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {
        Quaternion rotation = Quaternion.LookRotation(hitnormal, Vector3.up);
        //VisualEffectEvents.RaiseEffectRequested(new VisualEffectRequest(hitEffectName, hitpoint, rotation, transform, 1f));
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageOre_ServerRpc(float damage, WeaponType type, Vector3 hitpoint, Vector3 hitnormal, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        playDamageEffect_ClientRpc(x, y, type, hitpoint, hitnormal);
        if (damage <= 5) return;
        if (!WorldDataStore.Lookup.GetTreeAt(x, y).decorationObject.TryGetComponent(out BaseResourceBehaviour currentTree)) return;
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        float health = currentTree.health.currentAmount.Value;
        if (Vector3.Distance(sender.transform.position, currentTree.transform.position) > 2)
            return;
        health -= damage;
        Vector3 position = currentTree.transform.position;
        if (((BaseResource)currentTree.baseitem).drops.Length > 0)
        {
            BaseResource res = (BaseResource)currentTree.baseitem;
            for (int i = 0; i < res.drops.Length; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (res.dropsHowMany[i] <= 0) break;
                    position.x += UnityEngine.Random.Range(-.1f, .1f);
                    position.z += UnityEngine.Random.Range(-.1f, .1f);
                    position.y += 1;
                    GameObject temp = Instantiate(res.drops[i], position, Quaternion.identity);
                    temp.GetComponent<NetworkObject>().Spawn();
                    res.dropsHowMany[i] -= 1;
                }
            }
        }
        if (currentTree.health.currentAmount.Value <= 0f)
        {
            killResource_ClientRpc(x, y);
            return;
        }
        currentTree.health.currentAmount.Value = health;

    }

    [ClientRpc]
    public void playDamageEffect_ClientRpc(int x, int y, WeaponType type, Vector3 hitpoint, Vector3 hitnormal)
    {
        //ResourcesManager.Singleton.placedTrees[x, y].treeObject.GetComponent<OreResourceBehaviour>().playEffect(hitpoint, hitnormal, type);
    }
    [ClientRpc]
    public void killResource_ClientRpc(int x, int y)
    {
        if (!WorldDataStore.Lookup.GetTreeAt(x, y).decorationObject.TryGetComponent(out TreeResourceBehaviour currentTree)) return;
        if (currentTree == null) return;
        Destroy(currentTree);
        WorldDataStore.Lookup.RemoveTreeAt(x, y);
    }

    public void InitializeDecoration(int x, int y)
    {
        xCoordinate = x;
        yCoordinate = y;
    }

}
