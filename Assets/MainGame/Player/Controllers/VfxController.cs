using UnityEngine;

public class VfxController : MonoBehaviour
{
    [SerializeField] UnityEngine.VFX.VisualEffect damageParticles;
    [SerializeField] UnityEngine.VFX.VisualEffect splashParticles;
    [SerializeField] UnityEngine.VFX.VisualEffect landParticles;


    public void playSplashParticles(bool param)
    {
        splashParticles.Play();
    }

}