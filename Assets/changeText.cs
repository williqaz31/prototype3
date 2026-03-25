using TMPro;
using UnityEditor.UI;
using UnityEngine;

public class changeText : MonoBehaviour
{
    public TMP_Text text;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleText()
    {
        if (text.text == "Afficher formule")
        {
           text.text = "Cacher formule";
        } else if (text.text != "Afficher formule")
        {
            text.text = "Afficher formule";
        }
    }
}
