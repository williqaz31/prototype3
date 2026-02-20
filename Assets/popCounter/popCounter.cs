using System.Collections.Generic;
using individu;
using JetBrains.Annotations;
using SimulationFourmiliere;
using TMPro;
using UnityEngine;
//using UnityEngine.UI;

public class popCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text populationCounter;
    [SerializeField] private int stockNourriture = 50;
    private SimulationState state;

    private int popStartWeek;
    private int popStartMonth;

    public TextMeshProUGUI info;
    

  

    private int jourActuelle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = new SimulationState(stockNourriture);

    }

    public void NoveauJour()

    {
        try
        {
            Program.CalculSimulation(state);
            populationCounter.text = string.Format("{0:00000}", state.colonie.Pop());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    

    public void NouvelleSemaine()
    {
        
        popStartWeek = state.colonie.Pop();
        state.ResetCounters();
    }

    public string EndWeek()
    {
        int durer = 0;
        
        string famine;
        int popEndWeek = state.colonie.Pop();
        // Gestion de l'affichage de la croissance de la popualtion----------------------------------------
        int croissance = ((popEndWeek - popStartWeek) / popStartWeek * 100);
        string typeCroissance = croissance switch
        {
            0 => "<color=\"black\"></color>",
            < 0 => $"<color=\"red\">Décroissance: {croissance}</color>",
            > 0 => $"<color=\"white\">Croissance: {croissance}</color>",

        };
        
       
        
        // Affichage Famine ----------------------------------------
        if (state.Famine)
        {
            if (state.FinFamine != null && state.DebutFamine != null)
            {
                durer = (int)(state.FinFamine - state.DebutFamine);
            } 
       
            famine = $"""
                      <color="red">
                      -----------MANQUE DE NOURRITURE----------------
                      Durée: {durer}
                      Score de famine: {state.score}


                      </color>
                      """;
            
        }
        else famine = "Félicitation Il n'y à pas eu de famine";
        
        
        
        
        //Affichage Final----------------------------------------
        string info = $"""
                       -----------POPULATION--------------
                       Au début de la semaine:  {popStartWeek}
                       À la fin de la semaine:  {popEndWeek}
                       {typeCroissance}
                       -------------MORTS-----------------
                       Famine: {state.MortsAffame}
                       Naturel: {popEndWeek - popStartWeek - state.naissance - state.MortsAffame}
                       Total: {popEndWeek - popStartWeek}
                       {famine}
                       -----------NOURRITURE--------------
                       Stock: {stockNourriture}
                       


                       """;
        return info;

    }

    public void NouveauMois()
    {
        popStartMonth = state.colonie.Pop();
        state.ResetCounters();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
