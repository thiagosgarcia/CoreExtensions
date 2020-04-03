using System.Diagnostics;
using System.Text.RegularExpressions;
using PenguinSoft.CoreExtensions.Helpers.Domain;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class DomainExtensions
    {
        [DebuggerStepThrough]
        public static string FormatCpfOrCnpj(this string source)
        {
            if (source.IsCNPJ())
                return FormatCNPJ(source);
            if (source.IsCPF())
                return FormatCPF(source);

            return source;
        }

        [DebuggerStepThrough]
        public static string FormatCPF(this string source)
        {
            if (source.IsEmpty()) return source;
            var cpf = long.Parse(source.ExtractNumbers());
            return $@"{cpf:000\.000\.000\-00}";
        }


        [DebuggerStepThrough]
        public static string FormatCNPJ(this string source)
        {
            if (source.IsEmpty()) return source;
            var cnpj = long.Parse(source.ExtractNumbers());
            return $@"{cnpj:00\.000\.000\/0000\-00}";
        }


        [DebuggerStepThrough]
        public static string FormatCep(this string source)
        {
            if (source.IsEmpty()) return source;
            var cep = long.Parse(source.ExtractNumbers());
            return $@"{cep:00000\-000}";
        }


        [DebuggerStepThrough]
        public static bool IsCep(this string value)
        {
            return !value.IsEmpty()
                   && Regex.IsMatch(value, @"(^\d{2}[.]?\d{3}[-]?\d{3}$)");
        }

        [DebuggerStepThrough]
        public static bool IsCPF(this string source)
        {
            return CpfHelper.Validar(source);
        }

        [DebuggerStepThrough]
        public static bool IsCNPJ(this string source)
        {
            return CnpjHelper.Validar(source);
        }

        [DebuggerStepThrough]
        public static bool IsCpfOrCnpj(this string source)
        {
            return source.IsCNPJ() || source.IsCPF();
        }

        public static bool IsPIS(this string pis)
        {
            return PisHelper.Validar(pis);
        }

    }
}
