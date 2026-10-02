namespace XaropariaEtiquetas.Models;

public sealed record Lote(
    string LoteNumero,
    string Deposito,
    DateTime Validade,
    DateTime DataEntrada)
{
    public override string ToString() =>
        $"{LoteNumero} - Dep. {Deposito} - Val. {Validade:dd/MM/yyyy}";
}
