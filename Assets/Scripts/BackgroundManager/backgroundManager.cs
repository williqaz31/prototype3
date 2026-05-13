using SimulationFourmiliere;
using UnityEngine;
using UnityEngine.UI;

public class backgroundManager : MonoBehaviour
{
    public Image background;

    public Sprite springImage, summerImage, automnImage, winterImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnEnable()
    {
        SimulationState.OnSeasonChanged += UpdateBackground;
    }

    private void OnDisable()
    {
        SimulationState.OnSeasonChanged -= UpdateBackground;
    }

    private void UpdateBackground(Saison saison)
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