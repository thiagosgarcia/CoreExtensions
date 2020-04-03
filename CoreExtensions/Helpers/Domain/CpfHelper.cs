using System;
using PenguinSoft.CoreExtensions.Extensions;

namespace PenguinSoft.CoreExtensions.Helpers.Domain
{
    public static class CpfHelper
    {
        private static string GerarDigito(string cpf)
        {
            var peso = 2;
            var soma = 0;

            for (var i = cpf.Length - 1; i >= 0; i--)
            {
                soma += peso * Convert.ToInt32(cpf[i].ToString());
                peso++;
            }

            var pNumero = 11 - (soma % 11);

            if (pNumero > 9)
                pNumero = 0;

            return pNumero.ToString();
        }
        public static bool Validar(string cpf)
        {
            // Se for vazio
            if (cpf == null || cpf.IsNullOrEmpty())
                return false;

            // Retirar todos os caracteres que não sejam numéricos
            var aux = cpf.ExtractNumbers().PadLeft(11, '0');

            // O tamanho do CPF tem que ser 11
            if (aux.Length != 11)
                return false;

            for (var number = 0; number < 10; number++)
            {
                if (aux == new string(number.ToString()[0], 11))
                    return false;
            }

            // Guardo o dígito para comparar no final
            var pDigito = aux.Substring(9, 2);
            aux = aux.Substring(0, 9);

            //Cálculo do 1o. digito do CPF
            aux += GerarDigito(aux);

            //Cálculo do 2o. digito do CPF
            aux += GerarDigito(aux);

            return pDigito == aux.Substring(9, 2);
        }
    }
}