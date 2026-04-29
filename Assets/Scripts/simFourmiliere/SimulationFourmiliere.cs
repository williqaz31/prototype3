

//namespace SimulationFourmiliere
//{ 
using System;
using System.Collections.Generic;
using individu;
using UnityEngine;
using System.Linq;


namespace SimulationFourmiliere
{
    
    public class SimulationState
    {


        public event Action AucunApport; 
        public int StockNourriture
        {
            get => _stockNourriture;
            set  => _stockNourriture = value; 
        }

        public int DebutSaison;
        private Saison _saison;

        public Saison saison
        {
            get => _saison;
            set
            {
                _saison = value;
                OnSeasonChanged?.Invoke(_saison);
            }
        }
        public int apport
        {
            get => _apport;
            set => _apport = value;
        }
        

        private int _apport;
        private int _nbJourSansApport = 0;

        public int nbJourSansApport
        {
            get => _nbJourSansApport;
            set
            {
                _nbJourSansApport = value;
                if (nbJourSansApport == 10)
                {
                    AucunApport?.Invoke();
                }
            }
        }

        private int _stockNourriture;
        public Colonie Colonie;
        public List<Oeuf> Oeufs;
        public int Jour;
        public List<float> HistoriquePopulation;
        public List<float> HistoriqueNourriture;
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
        public static event Action<Saison> OnSeasonChanged; 
       
        

        public SimulationState(int stockInitial)
        {
            _stockNourriture = stockInitial;
            Colonie = new Colonie(4);
            Oeufs = new List<Oeuf>();
            Jour = 0;
            apport = 150;
            HistoriquePopulation = new List<float>();
            HistoriqueNourriture = new List<float>();
            HistoriqueNourriture.Add(_stockNourriture);
            HistoriquePopulation.Add(Colonie.Pop());
            ResetCounters();
        }

        public SimulationState(State etat)
        {
            _stockNourriture = (int)etat.graphBouff.Last();
            
            Oeufs = new List<Oeuf>();
            Jour = etat.gameTime;
            apport = etat.appartParJour;
            HistoriquePopulation = etat.graphPop;
            HistoriqueNourriture = etat.graphBouff;
            Colonie = new Colonie((int)etat.graphPop.Last(),new Reine(etat.ageReine,etat.dureeDeVieReine));
            
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
            NourritureConsomer = 0;
            NourritureTrouver = 0;


        }

        public void NewBorn()
        {
            Colonie.Naissance();
            Naissance++;
            
            //if (Naissance > 0 && Naissance % 3 == 0)
           // {
           //     if (mapLoader != null)
           //     {
            //        mapLoader.CreuserUnBloc();
           //         mapLoader.RefreshTilemap(); 
             //   }
           // }
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
        public Reine reine;


        public Colonie(int nbFourmisDebut)

        {
            Population = new List<Fourmi>();
            FourmisMortes = new List<Fourmi>();
            reine = new Reine();

            for (int i = 0; i < nbFourmisDebut; i++)
                Population.Add(new Fourmi());
        }

        public Colonie(int nbFourmisDebut, Reine Reine)
        {
            Population = new List<Fourmi>();
            FourmisMortes = new List<Fourmi>();
            reine = Reine;

            for (int i = 0; i < nbFourmisDebut; i++)
                Population.Add(new Fourmi());
        }

        public void Naissance()
        {
            Population.Add(new Fourmi());
            
            
        }
        
        

        public void ReineMorte()
        {
           
            reine = null;
            
        }

        public int Pop()
        {
            if (reine != null)
            {
                return Population.Count + 1;
            }
            return Population.Count;
        }
    }

    class Program
    {
        // Paramètres saisons
        const int DureeHiver = 92;
        const int DureePrintemps = 91;
        const int DureeEte = 91;
        const int DureeAutomne = 90;
        private const double ConsoHiver = 0.5;

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

        static  double DecisionConso(Saison saison)
        {
            if (saison == Saison.Hiver)
            {


                return ConsoHiver;
            }

            return 2;
        }

        static double PonteParSaison(Saison saison)
        {
            switch (saison)
            {
                case Saison.Hiver: return 0.0;
                case Saison.Printemps: return 0.7;
                case Saison.Ete: return 1.0;
                case Saison.Automne: return 0.3;
                default: return 0.0;
            }
        }

        static List<Oeuf> Ponte( double fEspace, SimulationState state)
        {
            double a = PonteParSaison(state.saison);
            
            
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
            try
            {
                if (state.saison != SaisonActuelle(state.Jour))
                {
                    state.DebutSaison = state.Jour;
                }
            }
            catch 
            {
            }
            state.saison = SaisonActuelle(state.Jour);



            double decision = DecisionConso(state.saison);

            int apport = state.apport;
            if (apport==0)
            {
                state.nbJourSansApport++;
                
            }



            state.NourritureTrouver += apport;

            double consoParFourmi = decision;
            int consommationHiver;
            int apportHiver;
           

            state.StockNourriture += apport;


            if (state.saison == Saison.Hiver)
            {
                 consommationHiver = (int)(Mathf.RoundToInt(DureeHiver - state.DebutSaison) * ConsoHiver * (state.Colonie.Pop() + state.Oeufs.Count));
                 apportHiver = (DureeHiver - state.DebutSaison) * 10;
            }
            else
            {
                consommationHiver = (int)((DureeHiver ) * ConsoHiver * (state.Colonie.Pop() + state.Oeufs.Count));
                apportHiver = (DureeHiver ) * 10;
            }


            double fEspace = Math.Max(0, 1.0 - (double)state.Colonie.Pop() / K);
            
            //# Vérifie si la reine est encore en vie pour pondre les oeufs et ne pond pas d'oeuf si elle est affamé
            
            if (state.Colonie.reine != null)
            {
                if (state.Colonie.reine.Vieillir() is null && state.Colonie.reine.joursSansManger == 0)
                {
                   
                    if( apport > ((state.Colonie.Pop() + state.Oeufs.Count) * consoParFourmi))
                    {
                        state.Oeufs = Ponte(fEspace,  state);
                    }
                    else if (state.saison == Saison.Automne || state.saison == Saison.Hiver)
                    {
                        if (state.StockNourriture + apportHiver >= consommationHiver)
                        {
                            state.Oeufs = Ponte(fEspace,  state);
                       
                        }
                        
                    } 
                    
                    
                    else if (state.Jour < 7)
                    {
                        state.Oeufs = Ponte(fEspace,  state);
                        
                    }
                }
                else
                {
             
                    state.Colonie.ReineMorte();
                }
            }
          

            //   # -----------------------------------------------GESTION DE NOURRITURE-----------------------------------------------------------------
            int consommationPossible = (int)(state.StockNourriture / consoParFourmi);

            if (consommationPossible >= state.Colonie.Pop())
            {
                //On peu nourrir toutes les fourmis
                state.JoursSansFamine++;
                state.Affamer = 0;
                
                state.FamineTerminer();
                

                foreach (Fourmi fourmi in state.Colonie.Population)
                {
                    fourmi.Manger();
                    state.StockNourriture -= (int)consoParFourmi;
                    state.NourritureConsomer += (int)consoParFourmi;
                   
                }
            }
            else
            {  //On ne peut pas nourrir tout les fourmis
                
               
                
                int fourmiNourries = consommationPossible;
                state.StockNourriture = 0;
                
                state.JoursSansFamine = 0;
                state.Score -= state.VitesseDegrade * state.Colonie.Pop();
                state.Score = Math.Clamp(state.Score, 0, 100);

               
                state.Colonie.Population.Sort((f1, f2) => f2.joursSansManger.CompareTo(f1.joursSansManger));
                // # Fait manger les fourmis affamées en commencent par la reine
                if (state.Colonie.reine != null)
                {
                    if (consommationPossible >= 1)
                    {
                        state.Colonie.reine.Manger();
                        state.NourritureConsomer +=(int) consoParFourmi;
                    }
                    else
                    {
                        if (state.Colonie.reine.Affamer() is null)
                        {
                            state.Colonie.ReineMorte();
                        }

                    }
                }

                for (int i = 0; i <= fourmiNourries-consoParFourmi; i++) 
                {
                        state.Colonie.Population[i].Manger();
                        state.NourritureConsomer += (int)consoParFourmi;
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

                             if (state.Affamer <= state.Colonie.Pop())
                             {
                                 state.Affamer++;
                             }
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
            state.HistoriqueNourriture.Add(state.StockNourriture);

            state.UpdateScore();
            state.Jour++;
        }
    }
}
