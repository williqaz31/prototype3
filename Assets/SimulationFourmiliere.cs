

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
        public int StockNourriture
        {
            get => _stockNourriture;
            set  => _stockNourriture = value; 
        }

        private int _stockNourriture;
        public Colonie Colonie;
        public List<Oeuf> Oeufs;
        public int Jour;
        public List<int> HistoriquePopulation;
        public int MortsAffame;
        public int Naissance;
        public int Affamer;
        public int? DebutFamine = null;
        public int? FinFamine = null;
        public bool Famine = false;
        private double _vitesseRecup = 0.18;
        private double _vitesseDegrade= 0.25;
        public double Score = 0;
        public int JoursSansFamine = 0;
        public int NourritureTrouver = 0;
        public int NourritureConsomer = 0;
        

        public SimulationState(int stockInitial)
        {
            _stockNourriture = stockInitial;
            Colonie = new Colonie(4);
            Oeufs = new List<Oeuf>();
            Jour = 0;
            HistoriquePopulation = new List<int>();
            HistoriquePopulation.Add(Colonie.Pop());
            ResetCounters();
        }

        public void UpdateScore()
        {
            if (JoursSansFamine >= 10)
            {
                if (Score > 30)
                {
                    Score = 30;
                } else Score -= VitesseRecup * Colonie.Pop();
            }else if (JoursSansFamine >= 3)
            {
                if (Score > 60)
                {
                    Score = 60;
                } else Score -= VitesseRecup * Colonie.Pop();
                
            }
            Score = Math.Clamp(Score, 0, 100);
            
        }
      
        public double VitesseDegrade
        {
           

            get
            {
                int etatCritique = (int)(Colonie.Pop() * 0.2);
                _vitesseDegrade = (_stockNourriture - (Colonie.Pop() + Oeufs.Count) * 2) switch
                {
                    <= 0 => 0.8,
                    var x when x <= etatCritique => 0.4,
                    var x when x <= Colonie.Pop() * 0.5 => 0.2,
                    var x when x <= Colonie.Pop() => 0.0,
                    _ => 0

                };
                return _vitesseDegrade;
            }
        }


        public double VitesseRecup
        {
            get
            {
                int etatCritique = (int)(Colonie.Pop() * 0.2);
                _vitesseRecup = (_stockNourriture - (Colonie.Pop() + Oeufs.Count) * 2) switch
                {
                    <= 0 => 0.0,
                    var x when x <= etatCritique => 0.1,
                    var x when x <= Colonie.Pop() * 0.5 => 0.3,
                    var x when x <= Colonie.Pop() => 0.8,
                    _ => 0

                };
                return _vitesseRecup;
            } 
           
        }

        public void DeclencheFamine()
        {
            
            DebutFamine = Jour;
            Famine = true;
             
        }

        public void FamineTerminer()
      
        {
            FinFamine = Jour;
      
        }

        public void ResetCounters()
        {
            MortsAffame = 0;
            Naissance = 0;
            Affamer = 0;
            Famine = false;
            FinFamine = null;
            DebutFamine = null;


        }

        public void NewBorn()
        {
            Colonie.Naissance();
            Naissance++;
        }

        public void NouvMorts(Fourmi morte)
        {
            Colonie.Population.Remove(morte);
            MortsAffame++;
            Famine = true;
            // score monte a 70 si il y a une mort de famine
            if (Score > 70)
            {
                Score = 70;
            }
            else Score += VitesseDegrade * Colonie.Pop();

            Score = Math.Clamp(Score, 0, 100);
        }
    }

    public enum Saison
    {
        Hiver,
        Printemps,
        Ete,
        Automne
    }


    public class Colonie
    {
        public List<Fourmi> Population;
        public List<Fourmi> FourmisMortes;
        public Reine Reine;


        public Colonie(int nbFourmisDebut)

        {
            Population = new List<Fourmi>();
            FourmisMortes = new List<Fourmi>();
            Reine = new Reine();

            for (int i = 0; i < nbFourmisDebut; i++)
                Population.Add(new Fourmi());
        }

        public void Naissance()
        {
            Population.Add(new Fourmi());
            
            
        }
        
        

        public void ReineMorte()
        {
           // Debug.Log("ReineMorte" + this.Pop());
            Reine = null;
        }

        public int Pop()
        {
            return Population.Count + (Reine != null ? 1 : 0);
        }
    }

    class Program
    {
        // Paramètres saisons
        const int DureeHiver = 92;
        const int DureePrintemps = 91;
        const int DureeEte = 91;
        const int DureeAutomne = 90;
        private const int ConsoHiver = 1;

        const int Annee = DureeHiver + DureePrintemps + DureeEte + DureeAutomne;

        const int K = 10000;
        const int PonteMax = 10;


        static Saison SaisonActuelle(int jour)
        {
            int j = (jour ) % Annee;
            
            if (j < DureeEte)
            {
               
                return Saison.Ete;
            }

            j -= DureeEte;
            
            if (j < DureeAutomne)
            {
                return Saison.Automne;
            }
            j-=  DureeAutomne;


            if (j < DureeHiver)
            {
      
                return Saison.Hiver;
                
            }
            return Saison.Printemps;
            
            

          
        }

        static (int apport, int conso) DecisionApport(Saison saison)
        {
            if (saison == Saison.Hiver)
            {


                return (10, ConsoHiver);
            }

            return (150, 2);
        }

        static double PonteParSaison(Saison saison)
        {
            switch (saison)
            {
                case Saison.Hiver: return 0.0;
                case Saison.Printemps: return 0.065;
                case Saison.Ete: return 1.0;
                case Saison.Automne: return 0.3;
                default: return 0.0;
            }
        }

        static List<Oeuf> Ponte( double fEspace, Saison saison,SimulationState state)
        {
            double a = PonteParSaison(saison);
            
            
            int eT = (int)Math.Round(PonteMax * fEspace * a);
           // Debug.Log(E_t);
            List<Oeuf> nouveaux = new List<Oeuf>();

            for (int i = 0; i < eT; i++)
                state.Oeufs.Add(new Oeuf());

            foreach (var oeuf in state.Oeufs)
            {
                if (oeuf.Vieillir() != null)
                {
                    state.NewBorn();
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


            Saison saison = SaisonActuelle(state.Jour);

            var decision = DecisionApport(saison);
                
            int apport = decision.apport;
            
           
            
            state.NourritureTrouver += apport;
            
            int consoParFourmi = decision.conso;

            state.StockNourriture += apport;
            
        

            int consommationHiver = DureeHiver * ConsoHiver * (state.Colonie.Pop() + state.Oeufs.Count);
           

            double fEspace = Math.Max(0, 1.0 - (double)state.Colonie.Pop() / K);
            
            //# Vérifie si la reine est encore en vie pour pondre les oeufs et ne pond pas d'oeuf si elle est affamé
            
            if (state.Colonie.Reine != null)
            {
                if (state.Colonie.Reine.Vieillir() is null && state.Colonie.Reine.joursSansManger == 0)
                {
                   
                    // # Si la colonie à asser en réserve pour pouvoir survivre a l'hiver sans apport quotidien alors on peut pondre sionon non
                    if (state.StockNourriture >= consommationHiver)
                    {
                        state.Oeufs = Ponte(fEspace, saison, state);
                    }
                }
            }
            else
            {
             
                state.Colonie.ReineMorte();
            }

            //   # -----------------------------------------------GESTION DE NOURRITURE-----------------------------------------------------------------
            int consommationPossible = (state.StockNourriture / consoParFourmi);

            if (consommationPossible >= state.Colonie.Pop())
            {
                //On peu nourrir toutes les fourmis
                state.JoursSansFamine++;
                state.Affamer = 0;
                
                state.FamineTerminer();
                

                foreach (Fourmi fourmi in state.Colonie.Population)
                {
                    fourmi.Manger();
                    state.StockNourriture -= consoParFourmi;
                   
                }
            }
            else
            {  //On ne peut pas nourrir tout les fourmis
                
                Debug.Log("Famine");
                
                int fourmiNourries = consommationPossible;
                state.StockNourriture = 0;
                
                state.JoursSansFamine = 0;
                state.Score -= state.VitesseDegrade * state.Colonie.Pop();
                state.Score = Math.Clamp(state.Score, 0, 100);

               
                state.Colonie.Population.Sort((f1, f2) => f2.joursSansManger.CompareTo(f1.joursSansManger));
                // # Fait manger les fourmis affamées en commencent par la reine
                if (state.Colonie.Reine != null)
                {
                    if (consommationPossible >= 1)
                    {
                        state.Colonie.Reine.Manger();
                    }
                    else
                    {
                        if (state.Colonie.Reine.Affamer() is null)
                        {
                            state.Colonie.ReineMorte();
                        }

                    }
                }

                for (int i = 0; i <= fourmiNourries-consoParFourmi; i++) 
                {
                        state.Colonie.Population[i].Manger();
                }

                 // # Gestion des fourmies mortes
                List<Fourmi> mortes = new List<Fourmi>(); 
                int b;
                //# Boucle dans la population a partir de la derniere fourmi nourrie jusqu'à la dernière fourmi
                for (b = fourmiNourries;b < state.Colonie.Population.Count; b++)
                 { 
                     try
                     {
                         //#Vérifie si les fourmis qui ne mange pas aujourd'hui meurt
                         
                         Fourmi morte = state.Colonie.Population[b].Affamer(); 
                         if (morte != null)
                         {
                             mortes.Add(morte);
                             state.Affamer--;
                         }
                         else
                         {
                             if (state.FinFamine is null)
                             {
                                 state.DeclencheFamine();
                             }
                             
                             state.Affamer++;
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
                     state.NouvMorts(morte);
                 }
                 
                
            }

            // Mortalité naturelle
            state.Colonie.Population.RemoveAll(f => { return f.Vieillir() != null; });

            state.HistoriquePopulation.Add(state.Colonie.Pop());

            state.UpdateScore();
            state.Jour++;
        }
    }
}
