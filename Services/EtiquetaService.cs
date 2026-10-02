using System.Globalization;
using System.Text;
using System.Text.Json;
using XaropariaEtiquetas.Helpers;
using XaropariaEtiquetas.Models;

namespace XaropariaEtiquetas.Services;

/// <summary>
/// Gera o ZPL das etiquetas, preservando o layout do sistema antigo.
/// Nenhum ZPL deve existir fora deste serviço.
/// A mídia possui duas posições físicas por linha (esquerda | direita),
/// ambas dentro do mesmo ^XA ... ^XZ.
/// </summary>
public sealed class EtiquetaService
{
    private const string CabecalhoLinha = "^XA^MD7^PR2^MMT^PW807^LL240^LS0";
    private const string RodapeLinha = "^XZ";

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly JsonSerializerOptions QrJsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Gera o ZPL para todos os itens da fila, expandindo cada item na sua
    /// quantidade de etiquetas e emparelhando esquerda/direita por linha física.
    /// Se a quantidade total for ímpar, a última linha tem apenas a esquerda.
    /// </summary>
    public string GerarZpl(IEnumerable<ItemEtiqueta> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var etiquetas = ExpandirEtiquetas(itens).ToList();
        var data = DateTime.Today.ToString("dd/MM/yyyy", PtBr);

        var zpl = new StringBuilder();

        for (var i = 0; i < etiquetas.Count; i += 2)
        {
            var esquerda = etiquetas[i];
            var direita = i + 1 < etiquetas.Count ? etiquetas[i + 1] : null;

            zpl.Append(CabecalhoLinha);
            zpl.Append(GerarLado(esquerda, data, ladoEsquerdo: true));

            if (direita is not null)
            {
                zpl.Append(GerarLado(direita, data, ladoEsquerdo: false));
            }

            zpl.Append(RodapeLinha);
        }

        return zpl.ToString();
    }

    private static IEnumerable<EtiquetaUnica> ExpandirEtiquetas(
        IEnumerable<ItemEtiqueta> itens)
    {
        foreach (var item in itens)
        {
            var quantidade = Math.Max(1, item.QuantidadeEtiquetas);
            for (var i = 0; i < quantidade; i++)
            {
                yield return new EtiquetaUnica(Guid.NewGuid(), item);
            }
        }
    }

    private static string GerarLado(EtiquetaUnica etiqueta, string data, bool ladoEsquerdo)
    {
        var item = etiqueta.Item;

        var produto = item.Produto.Descricao ?? string.Empty;
        var produtoLinha1 = Fatiar(produto, 0);
        var produtoLinha2 = Fatiar(produto, 30);
        var produtoLinha3 = Fatiar(produto, 60);

        var pesoLiquido = DecimalHelper.ParaN3(item.PesoLiquido);
        var tara = DecimalHelper.ParaN3(item.Tara);
        var validade = item.Lote.Validade == DateTime.MinValue
            ? string.Empty
            : item.Lote.Validade.ToString("dd/MM/yyyy", PtBr);
        var lote = item.Lote.LoteNumero;
        var colaborador = item.Colaborador;
        var jsonQr = MontarQrCode(etiqueta, pesoLiquido, validade);

        return ladoEsquerdo
            ? $"^FT26,35 ^A0N,20,20 ^FH^CI28^FD{produtoLinha1}^FS^CI27" +
              $"^FT26,55 ^A0N,20,20 ^FH^CI28^FD{produtoLinha2}^FS^CI27" +
              $"^FT26,80 ^A0N,20,20 ^FH^CI28^FD{produtoLinha3}^FS^CI27" +
              "^FT16,103 ^A0N,20,20 ^FH^CI28^FDPeso Liq.:^FS^CI27" +
              "^FT58,126 ^A0N,20,20 ^FH^CI28^FDTara:^FS^CI27" +
              "^FT56,149 ^A0N,20,20 ^FH^CI28^FDData:^FS^CI27" +
              $"^FT104,103 ^A0N,20,20 ^FH^CI28^FD{pesoLiquido} ^FS^CI27" +
              $"^FT104,126 ^A0N,20,20 ^FH^CI28^FD{tara} ^FS^CI27" +
              $"^FT104,149 ^A0N,20,20 ^FH^CI28^FD{data} ^FS^CI27" +
              "^FT22,172 ^A0N,20,20 ^FH^CI28^FDValidade:^FS^CI27" +
              $"^FT104,172 ^A0N,20,20 ^FH^CI28^FD{validade}^FS^CI27" +
              "^FT54,195 ^A0N,20,20 ^FH^CI28^FDLote:^FS^CI27" +
              $"^FT104,195 ^A0N,20,20 ^FH^CI28^FD{lote} ^FS^CI27" +
              $"^FT25,238^A0N,20,20^FD{colaborador}^FS^CI27" +
              "^FO250,55" +
              "^BQN,2,3" +
              $"^FDLA,{jsonQr}^FS"
            : $"^FT458,35 ^A0N,20,20 ^FH^CI28^FD{produtoLinha1}^FS^CI27" +
              $"^FT458,50 ^A0N,20,20 ^FH^CI28^FD{produtoLinha2}^FS^CI27" +
              $"^FT458,75 ^A0N,20,20 ^FH^CI28^FD{produtoLinha3}^FS^CI27" +
              "^FT448,103 ^A0N,20,20 ^FH^CI28^FDPeso Liq.:^FS^CI27" +
              "^FT490,126 ^A0N,20,20 ^FH^CI28^FDTara:^FS^CI27" +
              "^FT488,149 ^A0N,20,20 ^FH^CI28^FDData:^FS^CI27" +
              $"^FT536,103 ^A0N,20,20 ^FH^CI28^FD{pesoLiquido} ^FS^CI27" +
              $"^FT536,126 ^A0N,20,20 ^FH^CI28^FD{tara} ^FS^CI27" +
              $"^FT536,149 ^A0N,20,20 ^FH^CI28^FD{data} ^FS^CI27" +
              "^FT454,172 ^A0N,20,20 ^FH^CI28^FDValidade:^FS^CI27" +
              $"^FT536,172 ^A0N,20,20 ^FH^CI28^FD{validade}^FS^CI27" +
              "^FT486,195 ^A0N,20,20 ^FH^CI28^FDLote:^FS^CI27" +
              $"^FT536,195 ^A0N,20,20 ^FH^CI28^FD{lote} ^FS^CI27" +
              $"^FT457,238^A0N,20,20^FD{colaborador}^FS^CI27" +
              "^FO650,55" +
              "^BQN,2,3" +
              $"^FDLA,{jsonQr}^FS";
    }

    private static string MontarQrCode(
        EtiquetaUnica etiqueta,
        string pesoLiquido,
        string validade)
    {
        var item = etiqueta.Item;

        var conteudo = new
        {
            id = etiqueta.Id.ToString(),
            pro = item.Produto.ProdutoId,
            lo = item.Lote.LoteNumero,
            peLiq = pesoLiquido,
            val = validade,
            dpt = item.Lote.Deposito,
        };

        return JsonSerializer.Serialize(conteudo, QrJsonOptions);
    }

    /// <summary>Retorna o trecho de até 30 caracteres a partir de <paramref name="inicio"/>.</summary>
    private static string Fatiar(string texto, int inicio)
    {
        if (string.IsNullOrEmpty(texto) || inicio >= texto.Length)
        {
            return string.Empty;
        }

        var comprimento = Math.Min(30, texto.Length - inicio);
        return texto.Substring(inicio, comprimento);
    }

    private sealed record EtiquetaUnica(Guid Id, ItemEtiqueta Item);
}
