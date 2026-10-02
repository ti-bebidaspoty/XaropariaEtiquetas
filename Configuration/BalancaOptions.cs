using System.IO.Ports;

namespace XaropariaEtiquetas.Configuration;

public sealed class BalancaOptions
{
    public const string SectionName = "Balanca";

    public int BaudRate { get; set; } = 4800;

    public int DataBits { get; set; } = 7;

    public StopBits StopBits { get; set; } = StopBits.One;

    public Parity Parity { get; set; } = Parity.None;
}
