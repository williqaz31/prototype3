using UnityEngine;
using SimulationFourmiliere;
using UnityEngine.UI;

public class backgroundManager : MonoBehaviour
{
    
    public Image background;
    public Sprite springImage, summerImage, automnImage, winterImage;

    void OnEnable() => SimulationState.OnSeasonChanged += UpdateBackground;
    void OnDisable() => SimulationState.OnSeasonChanged -= UpdateBackground;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateBackground(Saison saison)
    {
        //Change l'image d'arrière plan en fonction de la saison
        switch (saison)
        {
            case Saison.Automne:
                background.sprite = automnImage;
                break;
            case Saison.Ete:
                background.sprite = summerImage;
                break;
            case Saison.Hiver:
                background.sprite = winterImage;
                break;
            case Saison.Printemps:
                background.sprite = springImage;
                break;
            
        }
        
    }
}
