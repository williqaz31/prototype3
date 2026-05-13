using UnityEngine;

public class gestionnairePanelBouff : MonoBehaviour
{
    public RectTransform panneauTiroir;
    public Vector2 positionOuverte = new(0, 0);
    public Vector2 positionFermee = new(450, 0);

    private bool estCache;

    public void ToggleGlissement()
    {
        if (panneauTiroir == null) return;

        if (estCache)
        {
            panneauTiroir.anchoredPosition = positionOuverte;
            estCache = false;
        }
        else
        {
            panneauTiroir.anchoredPosition = positionFermee;
            estCache = true;
        }
    }
}