namespace XaropariaEtiquetas.Models;

public sealed class ItemEtiqueta
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Produto Produto { get; init; }

    public required Lote Lote { get; init; }

    public required string Colaborador { get; init; }

    public decimal PesoBruto { get; init; }

    public decimal Tara { get; init; }

    public decimal PesoLiquido { get; init; }

    public int QuantidadeEtiquetas { get; init; }

    // Colunas de apoio para o DataGridView (exibição formatada).
    public string ProdutoDescricao => Produto.Descricao;

    public string LoteNumero => Lote.LoteNumero;

    public string Deposito => Lote.Deposito;

    public string PesoLiquidoTexto => PesoLiquido.ToString("N3");
}
