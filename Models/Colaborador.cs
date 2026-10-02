namespace XaropariaEtiquetas.Models;

public sealed record Colaborador(string Nome)
{
    public override string ToString() => Nome;
}
