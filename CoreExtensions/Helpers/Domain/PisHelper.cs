using PenguinSoft.CoreExtensions.Extensions;

namespace PenguinSoft.CoreExtensions.Helpers.Domain
{
    public static class PisHelper
    {
        public static bool Validar(string pis)
        {
            var multiplicador = new int[10] { 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;

            pis = pis.Trim();
            pis = pis.ExtractNumbers().PadLeft(11, '0');

            soma = 0;

            for (var i = 0; i < 10; i++)
                soma += int.Parse(pis[i].ToString()) * multiplicador[i];
            resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            return pis.EndsWith(resto.ToString());
        }
    }
}
