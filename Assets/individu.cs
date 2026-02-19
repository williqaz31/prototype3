using System;

namespace individu
{
    public static class Utils
    {
        private static Random random = new Random();

        public static int DureeDeVie()
        {
            int low = 548;
            int high = 730;

            int lowOutside = 0;
            int highOutside = 900;

            double outsideProp = 0.3;

            if (random.NextDouble() < outsideProp)
                return random.Next(lowOutside, highOutside + 1);
            else
                return random.Next(low, high + 1);
        }

        public static int RandomRange(int min, int max)
        {
            return random.Next(min, max + 1);
        }
    }

    public class Fourmi
    {
        public int dureeDeVie;
        public int age;
        public int joursSansManger;

        public Fourmi()
        {
            dureeDeVie = Utils.DureeDeVie();
            age = 0;
            joursSansManger = 0;
        }

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

            if (age >= dureeDeVie)
                return this;
            else
                return null;
        }

   
    }

    public class Reine : Fourmi
    {
        public Reine() : base()
        {
            // Entre 10 et 20 ans (3650 à 7300 jours)
            dureeDeVie = Utils.RandomRange(3650, 7300);
        }
    }

    public class Oeuf
    {
        public int delai;
        public int age;

        public Oeuf()
        {
            delai = 21;
            age = 0;
        }

        public Oeuf Vieillir()
        {
            age++;

            if (age >= delai)
                return this;
            else
                return null;
        }
    }
}