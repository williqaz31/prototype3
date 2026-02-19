namespace SimulationFourmiliere
{
   using System;
using System.Collections.Generic;

namespace SimulationFourmiliere
{
    public enum Saison
    {
        HIVER,
        PRINTEMPS,
        ETE,
        AUTOMNE
    }

    public class Fourmi
    {
        public int joursSansManger = 0;
        public int age = 0;
        public int ageMax = 365;

        public void Manger()
        {
            joursSansManger = 0;
        }

        public Fourmi Affamer()
        {
            joursSansManger++;
            if (joursSansManger >= 7)
                return this;
            return null;
        }

        public Fourmi Vieillir()
        {
            age++;
            if (age >= ageMax)
                return this;
            return null;
        }
    }

    public class Oeuf
    {
        int age = 0;
        int dureeEclosion = 21;

        public bool Vieillir()
        {
            age++;
            return age >= dureeEclosion;
        }
    }

    public class Reine : Fourmi
    {
        public new Fourmi Vieillir()
        {
            age++;
            if (age >= 3650)
                return this;
            return null;
        }
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
                if (oeuf.Vieillir())
                    colonie.Naissance();
                else
                    nouveaux.Add(oeuf);
            }

            return nouveaux;
        }

        static void Main(string[] args)
        {
            int stockNourriture = 50;
            Colonie colonie = new Colonie(4);
            List<Oeuf> oeufs = new List<Oeuf>();

            int[] P = new int[jours + 1];
            P[0] = colonie.Pop();

            for (int t = 0; t < jours; t++)
            {
                Saison saison = SaisonActuelle(t);

                var decision = DecisionApport(saison);
                int apport = decision.apport;
                int consoParFourmi = decision.conso;

                int nourritureTotale = stockNourriture + apport;

                double f_espace = Math.Max(0, 1.0 - (double)colonie.Pop() / k);

                if (colonie.reine != null)
                {
                    oeufs = Ponte(oeufs, colonie, f_espace, saison);
                }

                int consommationPossible = nourritureTotale / consoParFourmi;

                if (consommationPossible >= colonie.Pop())
                {
                    stockNourriture = nourritureTotale - (colonie.Pop() * consoParFourmi);

                    foreach (var fourmi in colonie.population)
                        fourmi.Manger();
                }
                else
                {
                    stockNourriture = 0;
                }

                // Mortalité naturelle
                colonie.population.RemoveAll(f =>
                {
                    return f.Vieillir() != null;
                });

                P[t + 1] = colonie.Pop();
            }

            Console.WriteLine("Simulation terminée.");
            Console.WriteLine("Population finale : " + colonie.Pop());
        }
    }
}
}