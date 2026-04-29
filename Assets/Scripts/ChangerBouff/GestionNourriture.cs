using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GestionNourriture : MonoBehaviour
{

    [SerializeField] private TMP_InputField quantiteSelect;
    [SerializeField] private Slider bouffSlider;

    public event Action<int> onChangementApport;
    
    public int quantite;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setApport(int quantite)
    {
        bouffSlider.value = quantite;
        string quantiteString = string.Format("{0:00 000}", quantite);
        quantiteSelect.text = quantiteString;
    }

    public void onTextChange()
    {
        try
        {
            quantite = Convert.ToInt32(quantiteSelect.text);
        }
        catch (Exception e)
        {
           quantite = 666;
        } 
       
     
       if (bouffSlider.maxValue <= quantite)
       {
           bouffSlider.maxValue = quantite + 200;
       }
       bouffSlider.value = quantite;
       onChangementApport?.Invoke(quantite);
    }

    public void OnSliderChange()
    {
        quantite = (int)bouffSlider.value;
        string quantiteString = string.Format("{0:00 000}", quantite);
        quantiteSelect.text = quantiteString;
        onChangementApport?.Invoke(quantite);
    }
    
}
