
using Unity.Netcode;
using UnityEngine;
public class TreeResourceBehaviour : BaseResourceBehaviour
{
    [SerializeField] ParticleSystem woodImpact;

    public override void takeDamage(DamageContext ctx)
    {
        Debug.Log("The tree is being hit");
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
        BaseResourceBehaviour currentTree = ResourcesManager.Singleton.placedTrees[x, y];
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
        ResourcesManager.Singleton.placedTrees[x, y].playEffect(hitpoint, hitnormal, type);
    }
    public override void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {
        woodImpact.transform.position = hitpoint;
        woodImpact.transform.rotation = Quaternion.LookRotation(hitnormal);
        woodImpact.Play();
    }

    [ClientRpc]
    public void killResource_ClientRpc(int x, int y)
    {
        Destroy(ResourcesManager.Singleton.placedTrees[x, y].gameObject);
        ResourcesManager.Singleton.placedTrees[x, y] = null;
    }

}
