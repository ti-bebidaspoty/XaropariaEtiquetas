using Microsoft.Data.SqlClient;
using XaropariaEtiquetas.Models;

namespace XaropariaEtiquetas.Services;

/// <summary>
/// Consulta o banco do SAP Business One. Responsável somente por acesso a dados.
/// Toda a connection string vem de ConnectionStrings:Sap.
/// SQL parametrizado, sem Entity Framework.
/// </summary>
public sealed class SapService
{
    private readonly string _connectionString;

    public SapService(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Lista os produtos da Xaroparia. Permite um filtro opcional por
    /// ProdutoID ou Descricao sem alterar o resultado quando não há busca.
    /// </summary>
    public async Task<IReadOnlyList<Produto>> BuscarProdutosAsync(
        string? busca = null,
        CancellationToken cancellationToken = default)
    {
        var temBusca = !string.IsNullOrWhiteSpace(busca);

        var sql = temBusca
            ? """
              SELECT ProdutoID, Descricao, EAN
              FROM PTY_XaropariaProdutos
              WHERE Descricao IS NOT NULL
                AND LTRIM(RTRIM(Descricao)) <> ''
                AND (ProdutoID LIKE @Busca OR Descricao LIKE @Busca)
              ORDER BY Descricao
              """
            : """
              SELECT ProdutoID, Descricao, EAN
              FROM PTY_XaropariaProdutos
              WHERE Descricao IS NOT NULL
                AND LTRIM(RTRIM(Descricao)) <> ''
              ORDER BY Descricao
              """;

        var produtos = new List<Produto>();

        await using var conexao = new SqlConnection(_connectionString);
        await conexao.OpenAsync(cancellationToken);

        await using var comando = new SqlCommand(sql, conexao);
        if (temBusca)
        {
            comando.Parameters.Add("@Busca", System.Data.SqlDbType.NVarChar).Value =
                $"%{busca!.Trim()}%";
        }

        await using var leitor = await comando.ExecuteReaderAsync(cancellationToken);

        while (await leitor.ReadAsync(cancellationToken))
        {
            var produtoId = leitor.IsDBNull(0) ? null : leitor.GetString(0);
            var descricao = leitor.IsDBNull(1) ? null : leitor.GetString(1);

            // Ignora produtos sem ProdutoID ou sem Descricao.
            if (string.IsNullOrWhiteSpace(produtoId) || string.IsNullOrWhiteSpace(descricao))
            {
                continue;
            }

            produtos.Add(new Produto(
                ProdutoId: produtoId,
                Descricao: descricao,
                Ean: leitor.IsDBNull(2) ? null : leitor.GetString(2)));
        }

        return produtos;
    }

    /// <summary>
    /// Busca os lotes de um produto (número, depósito, validade e data de entrada).
    /// </summary>
    public async Task<IReadOnlyList<Lote>> BuscarLotesAsync(
        string produtoId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(produtoId))
        {
            return [];
        }

        const string sql = """
            SELECT
                'DataEntrada' = CONVERT(DATE, A.InDate),
                'LoteNumero'  = A.BatchNum,
                'Deposito'    = B.WhsCode,
                'Validade'    = CONVERT(DATE, A.ExpDate)
            FROM OIBT A
            INNER JOIN OITW B
                ON A.ItemCode = B.ItemCode
                AND A.WhsCode = B.WhsCode
            INNER JOIN OITM C
                ON B.ItemCode = C.ItemCode
            INNER JOIN OWHS D
                ON D.WhsCode = B.WhsCode
            WHERE
                BPLid = 3
                AND D.DropShip = 'N'
                AND D.Inactive = 'N'
                AND A.ItemCode = @ProdutoId
                AND A.Quantity <> 0
            GROUP BY
                A.InDate,
                A.BatchNum,
                B.WhsCode,
                A.ExpDate
            ORDER BY
                A.BatchNum
            """;

        var lotes = new List<Lote>();

        await using var conexao = new SqlConnection(_connectionString);
        await conexao.OpenAsync(cancellationToken);

        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.Add("@ProdutoId", System.Data.SqlDbType.NVarChar).Value = produtoId;

        await using var leitor = await comando.ExecuteReaderAsync(cancellationToken);

        while (await leitor.ReadAsync(cancellationToken))
        {
            lotes.Add(new Lote(
                LoteNumero: leitor.IsDBNull(1) ? string.Empty : leitor.GetString(1),
                Deposito: leitor.IsDBNull(2) ? string.Empty : leitor.GetString(2),
                Validade: leitor.IsDBNull(3) ? DateTime.MinValue : leitor.GetDateTime(3),
                DataEntrada: leitor.IsDBNull(0) ? DateTime.MinValue : leitor.GetDateTime(0)));
        }

        return lotes;
    }

    /// <summary>
    /// Lista os colaboradores da Xaroparia (departamento 16 ativos) somados aos
    /// colaboradores específicos definidos pela regra do sistema antigo.
    /// </summary>
    public async Task<IReadOnlyList<Colaborador>> BuscarColaboradoresAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CONCAT(A.firstName, ' ', A.lastName) AS Nome
            FROM OHEM A
            WHERE
                (
                    A.dept = 16
                    AND A.Active = 'Y'
                    AND A.status = 1
                )
                OR
                (
                    (A.firstName LIKE 'Mailson%' AND A.lastName LIKE 'Da Silva Rocha%')
                    OR (A.firstName LIKE 'Marcelo%' AND A.lastName LIKE 'Balsarini Dalecio%')
                    OR (A.firstName LIKE 'Charliston%' AND A.lastName LIKE 'Nunes Da Silva Oscar%')
                    OR (A.firstName LIKE 'Ryan%' AND A.lastName LIKE 'Santos Da Silva%')
                    OR (A.firstName LIKE 'Edmar%' AND A.lastName LIKE 'Da Silva Oscar%')
                    OR (A.firstName LIKE 'Clebyson%' AND A.lastName LIKE 'Carlos Amancio Pinto%')
                    OR (A.firstName LIKE 'Rauan%' AND A.lastName LIKE 'Gomes Passos%')
                    OR (A.firstName LIKE 'Lucas%' AND A.lastName LIKE 'Mendonça%')
                    OR (A.firstName LIKE 'Alex%' AND A.lastName LIKE 'Nunes Da Silva%')
                    OR (A.firstName LIKE 'Maicon%' AND A.lastName LIKE 'Michachi%')
                    OR (A.firstName LIKE 'Kayky%' AND A.lastName LIKE 'Mathias De Araujo%')
                    OR (A.firstName LIKE 'Luciano%' AND A.lastName LIKE '%Antonio%Silva%')
                    OR (A.firstName LIKE 'Mateus%' AND A.lastName LIKE '%Vinicius%Silva%')
                    OR (A.firstName LIKE 'Nevio%' AND A.lastName LIKE '%Antunes%Silva%')
                )
            ORDER BY Nome
            """;

        var colaboradores = new List<Colaborador>();

        await using var conexao = new SqlConnection(_connectionString);
        await conexao.OpenAsync(cancellationToken);

        await using var comando = new SqlCommand(sql, conexao);
        await using var leitor = await comando.ExecuteReaderAsync(cancellationToken);

        while (await leitor.ReadAsync(cancellationToken))
        {
            var nome = leitor.GetString(0).Trim();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                colaboradores.Add(new Colaborador(nome));
            }
        }

        return colaboradores;
    }
}
