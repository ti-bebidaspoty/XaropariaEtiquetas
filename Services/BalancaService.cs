using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Options;
using XaropariaEtiquetas.Configuration;

namespace XaropariaEtiquetas.Services;

/// <summary>
/// Comunicação com a balança via porta serial. Encapsula totalmente o
/// <see cref="SerialPort"/>. O Form nunca manipula a serial diretamente.
/// </summary>
public sealed class BalancaService : IDisposable
{
    private const char Stx = '\x02'; // início de mensagem (descartado)
    private const char Cr = '\x0D';  // fim de mensagem

    private readonly BalancaOptions _options;
    private readonly StringBuilder _buffer = new();
    private readonly object _sync = new();

    private SerialPort? _porta;

    public BalancaService(IOptions<BalancaOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>Indica se a balança está conectada.</summary>
    public bool Conectada => _porta?.IsOpen == true;

    /// <summary>Porta COM atualmente conectada.</summary>
    public string? PortaAtual { get; private set; }

    /// <summary>Último peso lido da balança (kg).</summary>
    public decimal PesoAtual { get; private set; }

    /// <summary>Disparado sempre que um novo peso válido é lido.</summary>
    public event EventHandler<decimal>? PesoAlterado;

    /// <summary>
    /// Lista as portas disponíveis para seleção na interface (COM1 a COM8),
    /// preservando o comportamento do sistema antigo. A existência real da
    /// porta é validada apenas no momento da conexão.
    /// </summary>
    public IReadOnlyList<string> ListarPortas() =>
        Enumerable.Range(1, 8).Select(n => $"COM{n}").ToArray();

    /// <summary>Conecta à porta informada usando os parâmetros da configuração.</summary>
    public void Conectar(string porta)
    {
        if (string.IsNullOrWhiteSpace(porta))
        {
            throw new ArgumentException("Porta inválida.", nameof(porta));
        }

        Desconectar();

        var serial = new SerialPort(porta)
        {
            BaudRate = _options.BaudRate,
            DataBits = _options.DataBits,
            StopBits = _options.StopBits,
            Parity = _options.Parity,
            Encoding = Encoding.ASCII,
            ReadTimeout = 500,
            WriteTimeout = 500,
        };

        serial.DataReceived += OnDataReceived;
        serial.Open();

        _porta = serial;
        PortaAtual = porta;

        lock (_sync)
        {
            _buffer.Clear();
        }
    }

    /// <summary>Fecha a conexão com a balança, se aberta.</summary>
    public void Desconectar()
    {
        var serial = _porta;
        _porta = null;
        PortaAtual = null;

        if (serial is null)
        {
            return;
        }

        try
        {
            serial.DataReceived -= OnDataReceived;
            if (serial.IsOpen)
            {
                serial.Close();
            }
        }
        catch
        {
            // Falhas ao fechar a porta não devem derrubar a aplicação.
        }
        finally
        {
            serial.Dispose();
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (sender is not SerialPort serial || !serial.IsOpen)
            {
                return;
            }

            var recebido = serial.ReadExisting();

            lock (_sync)
            {
                _buffer.Append(recebido);
                ProcessarBuffer();
            }
        }
        catch
        {
            // Exceções da SerialPort nunca podem derrubar o sistema.
        }
    }

    /// <summary>
    /// Extrai mensagens completas (terminadas em CR) do buffer e atualiza o peso.
    /// </summary>
    private void ProcessarBuffer()
    {
        int fim;
        while ((fim = IndexOf(Cr)) >= 0)
        {
            var mensagem = _buffer.ToString(0, fim);
            _buffer.Remove(0, fim + 1);

            if (TentarConverterPeso(mensagem, out var peso))
            {
                AtualizarPeso(peso);
            }
        }
    }

    private int IndexOf(char caractere)
    {
        for (var i = 0; i < _buffer.Length; i++)
        {
            if (_buffer[i] == caractere)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Trata a mensagem bruta da balança: remove STX, normaliza "00." e converte.
    /// </summary>
    private static bool TentarConverterPeso(string mensagem, out decimal peso)
    {
        peso = 0m;

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            return false;
        }

        var texto = mensagem
            .Replace(Stx.ToString(), string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();

        // Ajuste equivalente ao sistema antigo: "00." vira "0.".
        if (texto.StartsWith("00.", StringComparison.Ordinal))
        {
            texto = texto[1..];
        }

        return Helpers.DecimalHelper.TryParse(texto, out peso);
    }

    private void AtualizarPeso(decimal peso)
    {
        PesoAtual = peso;
        PesoAlterado?.Invoke(this, peso);
    }

    public void Dispose() => Desconectar();
}
