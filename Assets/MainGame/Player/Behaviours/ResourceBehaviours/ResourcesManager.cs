
using Unity.Netcode;
using UnityEngine;

public class ResourcesManager : NetworkBehaviour
{
    public static ResourcesManager Singleton;

    public BaseResourceBehaviour[,] placedTrees;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        int grassDistance = PlayerPrefs.GetInt("GrassDistance", 4);
        //worldGenerator.numberOfChunksToRender = (int)grassDistance;
    }


    [ServerRpc(RequireOwnership = false)]
    public void DamageOre_ServerRpc(float damage, WeaponType type, Vector3 hitpoint, Vector3 hitnormal, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        playDamageEffect_ClientRpc(x, y, type, hitpoint, hitnormal);
        if (damage <= 5) return;
        BaseResourceBehaviour currentTree = placedTrees[x, y];
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        float health = currentTree.health.amount;
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
        if (currentTree.health.amount <= 0f)
        {
            killResource_ClientRpc(x, y);
            return;
        }
        currentTree.health.amount = health;
        updateResource_ClientRpc(x, y, health);

    }
    [ServerRpc(RequireOwnership = false)]
    public void DamageTree_ServerRpc(float damage, WeaponType type, Vector3 hitpoint, Vector3 hitnormal, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        Debug.Log("The tree is being hit on the server");
        playDamageEffect_ClientRpc(x, y, type, hitpoint, hitnormal);
        if (damage <= 5) return;
        BaseResourceBehaviour currentTree = placedTrees[x, y];
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        float health = currentTree.health.amount;
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
        updateResource_ClientRpc(x, y, health);

    }
    [ClientRpc]
    public void playDamageEffect_ClientRpc(int x, int y, WeaponType type, Vector3 hitpoint, Vector3 hitnormal)
    {
        placedTrees[x, y].playEffect(hitpoint, hitnormal, type);
    }
    [ClientRpc]
    public void updateResource_ClientRpc(int x, int y, float newHealth)
    {
        placedTrees[x, y].health.amount = newHealth;
    }
    [ClientRpc]
    public void killResource_ClientRpc(int x, int y)
    {
        Destroy(placedTrees[x, y].gameObject);
        placedTrees[x, y] = null;
    }


}