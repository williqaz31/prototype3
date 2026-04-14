using UnityEngine;

public class GestionGraphiques : MonoBehaviour
{
    [Header("Configuration du Tiroir")]
    public RectTransform panneauTiroir; 

    [Header("Positions (X, Y)")]
    public Vector2 positionOuverte = new Vector2(0, 0); 
    
    public Vector2 positionFermee = new Vector2(150, 0); 

    private bool estCache = false;

    public void ToggleGlissement()
    {
        if (panneauTiroir == null)
        {
            Debug.LogError("Erreur : Tu as oublié de glisser l'objet TiroirGraphiques dans la case du script sur _Gestionnaire !");
            return;
        }

        if (estCache)
        {
            panneauTiroir.anchoredPosition = positionOuverte;
            estCache = false;
            Debug.Log("Ouverture du tiroir");
        }
        else
        {
            panneauTiroir.anchoredPosition = positionFermee;
            estCache = true;
            Debug.Log("Fermeture du tiroir");
        }
    }
}