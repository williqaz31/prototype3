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
        
        int croissance;
        int durer = 0;
        
        string famine;
        int popEndWeek = state.colonie.Pop();
        // Gestion de l'affichage de la croissance de la popualtion----------------------------------------
        if (popStartWeek > 0)
        {
            croissance = ((popEndWeek - popStartWeek) / popStartWeek * 100);
        }
        else croissance = 0;

        string typeCroissance = croissance switch
        {
            0 => "<color=\"white\">Population stable</color>",
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
       
            famine = $"<color=\"red\">\n-----------MANQUE DE NOURRITURE----------------\nDurée: {durer} \nScore de famine: {state.score}</color >";
                          

        }
        else famine = "<color=\"green\">Félicitation Il n'y à pas eu de famine</color >";


        int morts_naturels = popStartWeek - popEndWeek - state.naissance - state.MortsAffame;
        int morts = popStartWeek - popEndWeek;
        if (morts < 0)
        {
            morts = 0;
        }
        if (morts_naturels < 0)
        {
            morts_naturels = 0;
        }
        
        //Affichage Final----------------------------------------
        string info =
            $"-----------POPULATION--------------\nAu début de la semaine:  {popStartWeek}\nÀ la fin de la semaine:  {popEndWeek}\n{typeCroissance} \n-------------MORTS-----------------\nFamine: {state.MortsAffame}\nNaturel: {morts_naturels }\nTotal: {morts}\n{famine}\n-----------NOURRITURE--------------\nStock: {stockNourriture}";


                       
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
