namespace XaropariaEtiquetas.Models;

public sealed record Produto(
    string ProdutoId,
    string Descricao,
    string? Ean)
{
    // Apenas apresentação: evita repetir o código quando a descrição já o contém
    // (ex.: "MP000001 - MP000001 - GAS CARBONICO"). Não altera o ProdutoId real.
    public override string ToString()
    {
        var descricao = Descricao?.Trim() ?? string.Empty;
        var id = ProdutoId?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(descricao))
        {
            return id;
        }

        if (descricao.StartsWith(id, StringComparison.OrdinalIgnoreCase))
        {
            return descricao;
        }

        return $"{id} - {descricao}";
    }
}
