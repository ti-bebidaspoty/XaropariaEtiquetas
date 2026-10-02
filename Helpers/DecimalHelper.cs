using System.Globalization;

namespace XaropariaEtiquetas.Helpers;

/// <summary>
/// Utilitários para conversão e formatação de pesos em decimal.
/// Pesos sempre são tratados como <see cref="decimal"/> (nunca double).
/// </summary>
public static class DecimalHelper
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// Tenta converter um texto (vírgula ou ponto) em decimal.
    /// </summary>
    public static bool TryParse(string? texto, out decimal valor)
    {
        valor = 0m;

        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        texto = texto.Trim().Replace(',', '.');

        return decimal.TryParse(
            texto,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out valor);
    }

    /// <summary>
    /// Converte um texto em decimal, retornando 0 quando inválido.
    /// </summary>
    public static decimal Parse(string? texto) =>
        TryParse(texto, out var valor) ? valor : 0m;

    /// <summary>
    /// Formata o peso com três casas (N3) no padrão pt-BR. Ex.: 25,430.
    /// </summary>
    public static string ParaN3(decimal valor) => valor.ToString("N3", PtBr);
}
