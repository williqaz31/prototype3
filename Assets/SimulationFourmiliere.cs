using Unity.VisualScripting;

//namespace SimulationFourmiliere
//{ 
using System;
using System.Collections.Generic;
using individu;


namespace SimulationFourmiliere
{
    public class SimulationState
    {
        public int stockNourriture;
        public Colonie colonie;
        public List<Oeuf> oeufs;
        public int jour;
        public  List<int> historiquePopulation;   

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
        public List<Fourmi> population = new List<Fourmi>();
        public List<Fourmi> fourmisMortes = new List<Fourmi>();
        public Reine reine = new Reine();

        public Colonie(int nbFourmisDebut)
        {
            for (int i = 0; i < nbFourmisDebut; i++)
                population.Add(new Fourmi());
        }

        public void Naissance()
        {
            population.Add(new Fourmi());
            Console.WriteLine("Naissance");
        }

        public void Mort()
        {
            if (population.Count > 0)
            {
                fourmisMortes.Add(population[0]);
                population.RemoveAt(0);
            }
        }

        public void ReineMorte()
        {
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

        const int ANNEE = DUREE_HIVER + DUREE_PRINTEMPS + DUREE_ETE + DUREE_AUTOMNE;
        const int DECALAGE_ANNEE = -183;

        static int k = 10000;
        static int E_max = 10;
        static int jours = 6000;

        static Saison SaisonActuelle(int jour)
        {
            int j = (jour + DECALAGE_ANNEE) % ANNEE;
            if (j < 0) j += ANNEE;

            if (j < DUREE_HIVER) return Saison.HIVER;
            j -= DUREE_HIVER;

            if (j < DUREE_PRINTEMPS) return Saison.PRINTEMPS;
            j -= DUREE_PRINTEMPS;

            if (j < DUREE_ETE) return Saison.ETE;

            return Saison.AUTOMNE;
        }

        static (int apport, int conso) DecisionApport(Saison saison)
        {
            if (saison == Saison.HIVER)
                return (10, 1);
            else
                return (150, 2);
        }

        static double PonteParSaison(Saison saison)
        {
            switch (saison)
            {
                case Saison.HIVER: return 0.0;
                case Saison.PRINTEMPS: return 0.065;
                case Saison.ETE: return 1.0;
                case Saison.AUTOMNE: return 0.3;
                default: return 0;
            }
        }

        static List<Oeuf> Ponte(List<Oeuf> oeufs, Colonie colonie, double f_espace, Saison saison)
        {
            int E_t = (int)Math.Round(E_max * f_espace * PonteParSaison(saison));
            List<Oeuf> nouveaux = new List<Oeuf>();

            for (int i = 0; i < E_t; i++)
                oeufs.Add(new Oeuf());

            foreach (var oeuf in oeufs)
            {
                if (oeuf.Vieillir() != null)
                    colonie.Naissance();
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
            List<Oeuf> oeufs = new List<Oeuf>();                  */
          
            Saison saison = SaisonActuelle(state.jour);
            
            var decision = DecisionApport(saison);
            int apport = decision.apport;
            int consoParFourmi = decision.conso;
            
            int nourritureTotale = state.stockNourriture + apport;
            
            double f_espace = Math.Max(0, 1.0 - (double)state.colonie.Pop() / k);
            
            if (state.colonie.reine != null)
            {
                state.oeufs = Ponte(state.oeufs, state.colonie, f_espace, saison);
            }
            
            int consommationPossible = nourritureTotale / consoParFourmi;
            
            if (consommationPossible >= state.colonie.Pop())
            {
                state.stockNourriture = nourritureTotale - (state.colonie.Pop() * consoParFourmi);
            
                foreach (var fourmi in state.colonie.population)
                    fourmi.Manger();
            }
            else
            {
                state.stockNourriture = 0;
            }
            
            // Mortalité naturelle
            state.colonie.population.RemoveAll(f =>
            {
                return f.Vieillir() != null;
            });
            
            state.historiquePopulation.Add(state.colonie.Pop());
            
           
           
            state.jour++; 
        }
    }
}
//}