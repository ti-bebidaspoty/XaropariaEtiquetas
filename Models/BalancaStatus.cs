namespace XaropariaEtiquetas.Models;

public sealed record BalancaStatus(
    bool Conectada,
    string? Porta,
    decimal Peso)
{
    public static BalancaStatus Desconectada { get; } =
        new(false, null, 0m);
}
