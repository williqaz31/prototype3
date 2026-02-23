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
            populationCounter.text = string.Format("{0:00000}", state.Colonie.Pop());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    

    public void NouvelleSemaine(bool isWeekly)
    {
        
        popStartWeek = state.Colonie.Pop();
        if (isWeekly)
        { 
            state.ResetCounters();
            
        }
    }

    public string EndWeek()
    {
        
       
        int popEndWeek = state.Colonie.Pop();
        
        string details = getInfo(popEndWeek,popStartWeek);

        
        //Affichage Final----------------------------------------
        string info =
            $"-----------POPULATION--------------\nAu début de la semaine:  {popStartWeek}\nÀ la fin de la semaine:  {popEndWeek}{details}";
        
        return info;
        

    }
    
    public void NouveauMois(bool isWeekly)
    {
        popStartMonth = state.Colonie.Pop();
        if (!isWeekly)
        {
            state.ResetCounters();
        }
        
    }

    public string EndMonth()
    {
      
        int popEndMonth = state.Colonie.Pop();
      
        string details =  getInfo(popEndMonth, popStartMonth);
    
        
          
        //Affichage Final----------------------------------------
        string info =
            $"-----------POPULATION--------------\nAu début du mois:  {popStartMonth}\nÀ la fin du mois :  {popEndMonth}{details}";

        return info;


    }
    private string getInfo(int popEnd,int popStart)
    {
        int croissance;
        string typeCroissance;
        int durer = 0;
        string famine;
        // Gestion de l'affichage de la croissance de la popualtion----------------------------------------
        if (popStart> 0)
        {
            croissance = Mathf.RoundToInt((popEnd - popStart) / popStart * 100);
        }
        else croissance = 0;

        switch(croissance)
        {
            case < 0:
                
                typeCroissance = $"<color=\"red\">Décroissance: {croissance}%</color>";
                break;
                
            case 0: 
                typeCroissance =  "<color=\"white\">Population stable</color>";
                break;
            case > 0:
                typeCroissance = $"<color=\"white\">Croissance: {croissance}%</color>";
                break;
            default: 
                typeCroissance =  "<color=\"white\">Population stable</color>";
                break;

        }

        
        // Affichage Famine ----------------------------------------
        if (state.Famine)
        {
            if (state.FinFamine != null && state.DebutFamine != null)
            {
                durer = (int)(state.FinFamine - state.DebutFamine);
            } 
       
            famine = $"<color=\"red\">\n-----------MANQUE DE NOURRITURE----------------\nDurée: {durer}\nNombre de fourmi affamées: {state.Affamer}\nScore de famine: {state.Score}</color >";
                          

        }
        else famine = $"<color=\"green\">Félicitation Il n'y à pas eu de famine\nScore de famine: {state.Score}</color >";


        int morts_naturels = popStart- popEnd - state.Naissance - state.MortsAffame;
        int morts = popStart- popEnd;
        if (morts < 0)
        {
            morts = 0;
        }
        if (morts_naturels < 0)
        {
            morts_naturels = 0;
        }
        return $"\n{typeCroissance}\nNaissance: {state.Naissance}\n-------------MORTS-----------------\nFamine: {state.MortsAffame}\nNaturel: {morts_naturels }\nTotal: {morts}\n{famine}\n-----------NOURRITURE--------------\nStock: {state.StockNourriture}\nNourriture trouvé: {state.NourritureTrouver}\nNourriture consommer: {state.NourritureConsomer}";
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
