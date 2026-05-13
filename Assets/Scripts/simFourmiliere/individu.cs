using Random = System.Random;

namespace individu
{
    public static class Utils
    {
        private static readonly Random random = new();

        public static int DureeDeVie()
        {
            var low = 548;
            var high = 730;

            var lowOutside = 0;
            var highOutside = 900;

            var outsideProp = 0.3;

            if (random.NextDouble() < outsideProp)
                return random.Next(lowOutside, highOutside);
            return random.Next(low, high);
        }

        public static int RandomRange(int min, int max)
        {
            return random.Next(min, max);
        }
    }

    public class Fourmi
    {
        public int age;
        public int dureeDeVie;
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

            if (joursSansManger >= 14)
                return this;

            return null;
        }

        public Fourmi Vieillir()
        {
            age++;

            if (age >= dureeDeVie)
                return this;
            return null;
        }
    }

    public class Reine : Fourmi
    {
        public Reine()
        {
            // Entre 10 et 20 ans (3650 à 7300 jours)
            dureeDeVie = Utils.RandomRange(3650, 7300);
        }

        public Reine(int Age, int DureeDeVie)
        {
            dureeDeVie = DureeDeVie;
            age = Age;
        }
    }

    public class Oeuf
    {
        public int age;
        public int delai;

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
            return null;
        }
    }
}