using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class apparition : MonoBehaviour
{
    
    public CanvasGroup canvasGroup;

    private float temps;
    private int load;

    public void Start()
    {
        StartCoroutine(Debut());
        load = 1;
    }
    
    
    IEnumerator Debut()
    {
        temps = 0f;

        while (temps <= 3f)
        {
            temps += Time.deltaTime;
            canvasGroup.alpha = (temps/3f);
            yield return null;
        }
    }
}
