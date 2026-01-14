using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public static Loader instance;

    public Image progressImage;

    void Start()
    {
        if (instance == null)
            instance = this;
    }

    public void setProgress(float progress)
    {
        instance.progressImage.fillAmount = progress;
    }



}




