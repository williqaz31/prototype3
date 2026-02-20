using Unity.VisualScripting;

//namespace SimulationFourmiliere
//{ 
using System;
using System.Collections.Generic;
using individu;
using UnityEngine;


namespace SimulationFourmiliere
{
    public class SimulationState
    {
        public int stockNourriture;
        public Colonie colonie;
        public List<Oeuf> oeufs;
        public int jour;
        public List<int> historiquePopulation;

        public SimulationState(int stockInitial)
        {
            stockNourriture = stockInitial;
            colonie = new Colonie(4);
            oeufs = new List<Oeuf>();
            jour = 0;
            historiquePopulation = new List<int>();
            historiquePopulation.Add(colonie.Pop());
        }
    }

    public enum Saison
    {
        HIVER,
        PRINTEMPS,
        ETE,
        AUTOMNE
    }


    public class Colonie
    {
        public List<Fourmi> population;
        public List<Fourmi> fourmisMortes;
        public Reine reine;


        public Colonie(int nbFourmisDebut)

        {
            population = new List<Fourmi>();
            fourmisMortes = new List<Fourmi>();
            reine = new Reine();

            for (int i = 0; i < nbFourmisDebut; i++)
                population.Add(new Fourmi());
        }

        public void Naissance()
        {
            population.Add(new Fourmi());
            
        }

       
       
  
  
  
  

       
       
       

        public void ReineMorte()
        {
           // Debug.Log("ReineMorte" + this.Pop());
            reine = null;
        }

        public int Pop()
        {
            return population.Count + (reine != null ? 1 : 0);
        }
    }

    class Program
    {
        // Paramètres saisons
        const int DUREE_HIVER = 92;
        const int DUREE_PRINTEMPS = 91;
        const int DUREE_ETE = 91;
        const int DUREE_AUTOMNE = 90;
        private const int CONSO_HIVER = 1;

        const int ANNEE = DUREE_HIVER + DUREE_PRINTEMPS + DUREE_ETE + DUREE_AUTOMNE;

        const int K = 10_000;
        const int PONTE_MAX = 10;


        static Saison SaisonActuelle(int jour)
        {
            int j = (jour ) % ANNEE;
            
            if (j < DUREE_ETE)
            {
               
                return Saison.ETE;
            }

            j -= DUREE_ETE;
            
            if (j < DUREE_AUTOMNE)
            {
                return Saison.AUTOMNE;
            }
            j-=  DUREE_AUTOMNE;


            if (j < DUREE_HIVER)
            {
      
                return Saison.HIVER;
                
            }
            return Saison.PRINTEMPS;
            
            

          
        }

        static (int apport, int conso) DecisionApport(Saison saison)
        {
            if (saison == Saison.HIVER)
            {


                return (10, CONSO_HIVER);
            }
            else
            {
                return (150, 2);
            }
        }

        static double PonteParSaison(Saison saison)
        {
            switch (saison)
            {
                case Saison.HIVER: return 0.0;
                case Saison.PRINTEMPS: return 0.065;
                case Saison.ETE: return 1.0;
                case Saison.AUTOMNE: return 0.3;
                default: return 0.0;
            }
        }

        static List<Oeuf> Ponte(List<Oeuf> oeufs, Colonie colonie, double f_espace, Saison saison,int jour)
        {
            double a = PonteParSaison(saison);
            
            
            int E_t = (int)Math.Round(PONTE_MAX * f_espace * a);
           // Debug.Log(E_t);
            List<Oeuf> nouveaux = new List<Oeuf>();

            for (int i = 0; i < E_t; i++)
                oeufs.Add(new Oeuf());

            foreach (var oeuf in oeufs)
            {
                if (oeuf.Vieillir() != null)
                {
                    colonie.Naissance();
                   // Debug.Log("Naissance jour: " + jour);
                }
                
                else
                    nouveaux.Add(oeuf);
            }

            return nouveaux;
        }

        public static void CalculSimulation(SimulationState state)
        {
            /*
              int stockNourriture = 50;
              Colonie colonie = new Colonie(4);
              List<Oeuf> oeufs = new List<Oeuf>();
                             */


            Saison saison = SaisonActuelle(state.jour);

            var decision = DecisionApport(saison);
            int apport = decision.apport;
            int consoParFourmi = decision.conso;

            int nourritureTotale = state.stockNourriture + apport;

            int consommationHiver = DUREE_HIVER * CONSO_HIVER * (state.colonie.Pop() + state.oeufs.Count);

            double f_espace = Math.Max(0, 1.0 - (double)state.colonie.Pop() / K);
            
            //# Vérifie si la reine est encore en vie pour pondre les oeufs et ne pond pas d'oeuf si elle est affamé
            if (state.colonie.reine != null)
            {
                
                if (state.colonie.reine.Vieillir() is null && state.colonie.reine.joursSansManger == 0)
                {
                   // Debug.Log("Reine en vie et non affamé");
                    // # Si la colonie à asser en réserve pour pouvoir survivre a l'hiver sans apport quotidien alors on peut pondre sionon non
                    if (state.stockNourriture >= consommationHiver)
                    {
                       // Debug.Log("asser de nourriture pour l'hiver");
                        state.oeufs = Ponte(state.oeufs, state.colonie, f_espace, saison,state.jour);
                    }
                    else
                    {
                       // Debug.Log("Conso hiver: " + consommationHiver + "stock "+ state.stockNourriture );
                    }
                }
            }
            else
            {
             //   Debug.Log("Reine morte");
                state.colonie.ReineMorte();
            }

            //   # -----------------------------------------------GESTION DE NOURRITURE-----------------------------------------------------------------
            int consommationPossible = (nourritureTotale / consoParFourmi);

            if (consommationPossible >= state.colonie.Pop())
            {
                //On peu nourrir toutes les fourmis
                state.stockNourriture = nourritureTotale - (state.colonie.Pop() * consoParFourmi);

                foreach (Fourmi fourmi in state.colonie.population)
                    fourmi.Manger();
            }
            else
            {  //On ne peut nourrir tout les fourmis
                int fourmiNourries = consommationPossible;

               
                state.colonie.population.Sort((f1, f2) => f2.joursSansManger.CompareTo(f1.joursSansManger));
                // # Fait manger les fourmis affamées en commencent par la reine
                if (state.colonie.reine != null)
                {
                    if (consommationPossible >= 1)
                    {
                        state.colonie.reine.Manger();
                    }
                    else
                    {
                        if (state.colonie.reine.Affamer() is null)
                        {
                            state.colonie.ReineMorte();
                        }

                    }
                }

                for (int i = 0; i <= fourmiNourries-consoParFourmi; i++) 
                {
                        state.colonie.population[i].Manger();
                }

                 // # Gestion des fourmies mortes
                List<Fourmi> mortes = new List<Fourmi>(); 
                int b;
                //# Boucle dans la population a partir de la derniere fourmi nourrie jusqu'à la dernière fourmi
                for (b = fourmiNourries;b < state.colonie.population.Count; b++)
                 { 
                     try
                     {
                         //#Vérifie si les fourmis qui ne mange pas aujourd'hui meurt
                         Fourmi morte = state.colonie.population[b].Affamer(); 
                         if (morte != null)
                         {
                             mortes.Add(morte);
                         }
                     }
                     catch (Exception e)
                     {
                        
                         Debug.Log(e.Message );
                        
                     }
                 }
                 
                 //  # Tue les fourmis affamées depuis 7 jours
                 foreach (Fourmi morte in mortes) 
                 {
                     state.colonie.population.Remove(morte);
                 }
                 state.stockNourriture = 0;   
                
            }

            // Mortalité naturelle
            state.colonie.population.RemoveAll(f => { return f.Vieillir() != null; });

            state.historiquePopulation.Add(state.colonie.Pop());


            state.jour++;
        }
    }
}
