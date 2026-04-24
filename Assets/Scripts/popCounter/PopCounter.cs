using System;
using System.Collections.Generic;
using individu;
using JetBrains.Annotations;
using SimulationFourmiliere;
using TMPro;
using UnityEngine;
//using UnityEngine.UI;

public class PopCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text populationCounter;
    [SerializeField] private int stockNourriture = 50;
    [SerializeField] private graphPop _graphPop;
    [SerializeField] private graphBouff _graphBouff;
    public event Action GameOver;
    public SimulationState simState;
    private int popStartWeek;
    private int popStartMonth;
    
    

    public SimulationState GetState()
    {
        return simState;
    }

 





    //private int jourActuelle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
   
    }

    public void NoveauJour()
    

    {
        try
        {
            if (simState == null)
            {
                Time.timeScale = 0;
                Debug.Log("Aucune simulation popCounter ");
            }
            Program.CalculSimulation(simState);
            populationCounter.text = string.Format("{0:00000}", simState.Colonie.Pop());
            if (simState.Colonie.Pop() == 0)
            {
               GameOver?.Invoke();
            }
            string x = simState.Jour.ToString();


         
            
            
            _graphPop.chart.AddXAxisData(x);
            _graphPop.chart.AddData("population", simState.Colonie.Pop());
            _graphBouff.chart.AddXAxisData(x);
            _graphBouff.chart.AddData("nourriture", simState.StockNourriture);
           
           

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
      
        
    }
    

    public void NouvelleSemaine(bool isWeekly)
    {
        
        popStartWeek = simState.Colonie.Pop();
        if (isWeekly)
        { 
            simState.ResetCounters();
            
        }
    }

    public string EndWeek()
    {

        if (simState == null)
        {
            Time.timeScale = 0;
            Debug.Log("Aucune simulation popCounter ");
        }
        int popEndWeek = simState.Colonie.Pop();
        
        string details = getInfo(popEndWeek,popStartWeek);

        
        //Affichage Final----------------------------------------
        string info =
            $"-----------POPULATION--------------\nAu début de la semaine:  {popStartWeek}\nÀ la fin de la semaine:  {popEndWeek}{details}";
        
        return info;
        

    }
    
    public void NouveauMois(bool isWeekly)
    {
        popStartMonth = simState.Colonie.Pop();
        if (!isWeekly)
        {
            simState.ResetCounters();
        }
        
    }

    public string EndMonth()
    {
      
        int popEndMonth = simState.Colonie.Pop();
      
        string details =  getInfo(popEndMonth, popStartMonth);
    
        
          
        //Affichage Final----------------------------------------
        string info =
            $"-----------POPULATION--------------\nAu début du mois:  {popStartMonth}\nÀ la fin du mois :  {popEndMonth}{details}";

        return info;


    }
    private string getInfo(int popEnd,int popStart)
    {
        float croissance;
        string typeCroissance;
        int durer = 0;
        string famine;
        // Gestion de l'affichage de la croissance de la popualtion----------------------------------------
        if (popStart> 0)
        {
            
            croissance = ((float)(popEnd - popStart) / popStart) * 100;
          
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
        if (simState.Famine)
        {
            if (simState.FinFamine != null && simState.DebutFamine != null)
            {
                durer = (int)(simState.FinFamine - simState.DebutFamine);
            } else if (simState.DebutFamine != null)
            {
                durer = (int)(simState.Jour - simState.DebutFamine);
            }
            
       
            famine = $"<color=\"red\">\n-----------MANQUE DE NOURRITURE----------------\nDurée: {durer}\nNombre de fourmi affamées: {simState.Affamer}\nScore de famine: {simState.Score}</color >";
                          

        }
        else famine = $"<color=\"green\">Félicitation Il n'y à pas eu de famine\nScore de famine: {simState.Score}</color >";


        int morts_naturels = popStart- popEnd - simState.Naissance - simState.MortsAffame;
        int morts = popStart- popEnd;
        if (morts < 0)
        {
            morts = 0;
        }
        if (morts_naturels < 0)
        {
            morts_naturels = 0;
        }
        return $"\n{typeCroissance}\nNaissance: {simState.Naissance}\n-------------MORTS-----------------\nFamine: {simState.MortsAffame}\nNaturel: {morts_naturels }\nTotal: {morts}\n{famine}\n-----------NOURRITURE--------------\nStock: {simState.StockNourriture}\nNourriture trouvée: {simState.NourritureTrouver}\nNourriture consommée: {simState.NourritureConsomer}";
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
