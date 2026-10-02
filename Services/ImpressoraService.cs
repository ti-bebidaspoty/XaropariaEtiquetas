using System.Text;
using Microsoft.Extensions.Options;
using XaropariaEtiquetas.Configuration;

namespace XaropariaEtiquetas.Services;

/// <summary>
/// Envia o ZPL para a impressora Zebra compartilhada no Windows.
/// Estratégia: grava o ZPL em arquivo temporário e copia para o share.
/// </summary>
public sealed class ImpressoraService
{
    private readonly ImpressoraOptions _options;

    public ImpressoraService(IOptions<ImpressoraOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Imprime o conteúdo ZPL na impressora configurada em Impressora:Caminho.
    /// </summary>
    public async Task ImprimirAsync(
        string zpl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zpl))
        {
            throw new ArgumentException("ZPL vazio.", nameof(zpl));
        }

        if (string.IsNullOrWhiteSpace(_options.Caminho))
        {
            throw new InvalidOperationException(
                "Caminho da impressora não configurado (Impressora:Caminho).");
        }

        var arquivoTemporario = Path.Combine(
            Path.GetTempPath(),
            $"etiqueta_{Guid.NewGuid():N}.zpl");

        try
        {
            await File.WriteAllTextAsync(
                arquivoTemporario, zpl, Encoding.ASCII, cancellationToken);

            File.Copy(arquivoTemporario, _options.Caminho, overwrite: true);
        }
        finally
        {
            TentarExcluir(arquivoTemporario);
        }
    }

    private static void TentarExcluir(string caminho)
    {
        try
        {
            if (File.Exists(caminho))
            {
                File.Delete(caminho);
            }
        }
        catch
        {
            // A falha ao excluir o arquivo temporário não deve interromper o fluxo.
        }
    }
}
