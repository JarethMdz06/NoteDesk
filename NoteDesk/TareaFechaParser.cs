using System;
using System.Globalization;

namespace NoteDesk
{
    public static class TareaFechaParser
    {
        public static bool TryParse(string fechaTexto, out DateTime fecha)
        {
            fecha = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(fechaTexto))
            {
                return false;
            }

            string fechaLimpia = fechaTexto.Trim()
                .Replace(" de ", " ")
                .Replace(",", "");

            CultureInfo cultura = new CultureInfo("es-MX");

            if (DateTime.TryParse(
                    fechaLimpia,
                    cultura,
                    DateTimeStyles.AllowWhiteSpaces,
                    out fecha))
            {
                return true;
            }

            string[] formatos =
            {
                "d MMMM yyyy",
                "dd MMMM yyyy",
                "d/M/yyyy",
                "dd/MM/yyyy",
                "d-MM-yyyy",
                "dd-MM-yyyy"
            };

            return DateTime.TryParseExact(
                fechaLimpia,
                formatos,
                cultura,
                DateTimeStyles.AllowWhiteSpaces,
                out fecha);
        }
    }
}
