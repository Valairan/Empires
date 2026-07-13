
using Unity.Netcode;
using UnityEngine;
public class TreeResourceBehaviour : BaseResourceBehaviour, IScatteredDecoration
{
    [SerializeField] private string hitEffectName = "TreeHit";

    public GameObject decorationObject => throw new System.NotImplementedException();

    public override void takeDamage(DamageContext ctx)
    {
        float damage = ctx.damage;
        DamageTree_ServerRpc(damage, ctx.type, ctx.hitpoint, ctx.hitnormal, xCoordinate, yCoordinate);
    }


    [ServerRpc(RequireOwnership = false)]
    public void DamageTree_ServerRpc(float damage, WeaponType type, Vector3 hitpoint, Vector3 hitnormal, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        Debug.Log("The tree is being hit on the server");
        playDamageEffect_ClientRpc(x, y, type, hitpoint, hitnormal);
        if (damage <= 5) return;
        if (!WorldDataStore.Lookup.GetTreeAt(x, y).decorationObject.TryGetComponent(out TreeResourceBehaviour currentTree)) return;
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        float health = currentTree.health.currentAmount.Value;
        if (Vector3.Distance(sender.transform.position, currentTree.transform.position) > 2)
            return;
        health -= damage;
        if (health <= 0f)
        {
            Vector3 position = currentTree.transform.position;
            if (((BaseResource)currentTree.baseitem).drops.Length > 0)
            {
                BaseResource res = (BaseResource)currentTree.baseitem;
                for (int i = 0; i < res.drops.Length; i++)
                {
                    for (int j = 0; j < res.dropsHowMany[i]; j++)
                    {
                        position.y += 1;
                        GameObject temp = Instantiate(res.drops[i], position, Quaternion.identity);
                        temp.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
            killResource_ClientRpc(x, y);
            return;
        }

    }

    [ClientRpc]
    public void playDamageEffect_ClientRpc(int x, int y, WeaponType type, Vector3 hitpoint, Vector3 hitnormal)
    {
        //ResourcesManager.Singleton.placedTrees[x, y].treeObject.GetComponent<TreeResourceBehaviour>().playEffect(hitpoint, hitnormal, type);
    }
    public override void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {
        Quaternion rotation = Quaternion.LookRotation(hitnormal, Vector3.up);
        //VisualEffectEvents.RaiseEffectRequested(new VisualEffectRequest(hitEffectName, hitpoint, rotation, transform, 1f));
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
