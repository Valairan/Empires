using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;


public class VisualsManager : NetworkBehaviour
{
    public VisualEffect WaterSplash;
    public VisualEffect ShotSplash;

    public static VisualsManager Singleton;

    public override void OnNetworkSpawn()
    {
        if (Singleton == null) Singleton = this;
    }

    [ServerRpc]
    public void RequestLargeSplashVfx_ServerRpc(Vector3 position)
    {
        playLargeSplashOn_ClientRpc(position);
    }


    [ClientRpc]
    public void playLargeSplashOn_ClientRpc(Vector3 position)
    {
        WaterSplash.transform.position = position;
        WaterSplash.Play();
    }
    [ClientRpc]
    public void playShotSplashOn_ClientRpc(Vector3 position)
    {
        ShotSplash.transform.position = position;
        ShotSplash.Play();
    }

}
