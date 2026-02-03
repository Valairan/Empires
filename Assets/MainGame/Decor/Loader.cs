using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public static Loader Singelton;

    public Image Background;
    public Image progressImage;

    void Awake()
    {
        if (Singelton == null) Singelton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void setProgress(float progress)
    {
        Singelton.progressImage.fillAmount = progress;
    }



}




