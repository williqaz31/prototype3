using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class apparition : MonoBehaviour
{
    
    public CanvasGroup canvasGroup;

    private float temps;
   

    public void Start()
    {
        StartCoroutine(Debut());
       
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
