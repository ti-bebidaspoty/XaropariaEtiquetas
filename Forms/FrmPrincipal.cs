using System.ComponentModel;
using XaropariaEtiquetas.Helpers;
using XaropariaEtiquetas.Models;
using XaropariaEtiquetas.Services;

namespace XaropariaEtiquetas.Forms;

public partial class FrmPrincipal : Form
{
    private readonly SapService _sapService;
    private readonly BalancaService _balancaService;
    private readonly EtiquetaService _etiquetaService;
    private readonly ImpressoraService _impressoraService;

    private readonly BindingList<ItemEtiqueta> _fila = [];

    // Evita que a atribuição de DataSource nos combos dispare o carregamento
    // de lotes por uma seleção transitória (índice 0) durante o preenchimento.
    private bool _preenchendoCombos;

    public FrmPrincipal(
        SapService sapService,
        BalancaService balancaService,
        EtiquetaService etiquetaService,
        ImpressoraService impressoraService)
    {
        _sapService = sapService;
        _balancaService = balancaService;
        _etiquetaService = etiquetaService;
        _impressoraService = impressoraService;

        InitializeComponent();
        ConfigurarEventos();
        ConfigurarFila();
    }

    private void ConfigurarEventos()
    {
        Load += async (_, _) => await CarregarDadosIniciaisAsync();

        cmbProduto.SelectedIndexChanged += async (_, _) => await CarregarLotesAsync();

        rbBalanca.CheckedChanged += (_, _) => AtualizarModoPesagem();
        rbManual.CheckedChanged += (_, _) => AtualizarModoPesagem();

        btnConectar.Click += (_, _) => AlternarConexaoBalanca();
        txtTara.TextChanged += (_, _) => AtualizarCalculoPeso();
        txtPesoBruto.TextChanged += (_, _) => AtualizarCalculoPeso();
        txtPesoEmbalagem.TextChanged += (_, _) => AtualizarCalculoPeso();
        nudQuantidadeEmbalagens.ValueChanged += (_, _) => AtualizarCalculoPeso();

        _balancaService.PesoAlterado += OnPesoBalancaAlterado;

        btnAdicionar.Click += (_, _) => AdicionarItemFila();
        btnLimpar.Click += (_, _) => LimparFila();
        btnRemover.Click += (_, _) => RemoverSelecionado();
        btnImprimir.Click += async (_, _) => await ImprimirFilaAsync();

        FormClosed += (_, _) => _balancaService.Desconectar();
    }

    private void ConfigurarFila()
    {
        dgvFila.AutoGenerateColumns = false;
        dgvFila.DataSource = _fila;

        AdicionarColuna(nameof(ItemEtiqueta.ProdutoDescricao), "Produto");
        AdicionarColuna(nameof(ItemEtiqueta.LoteNumero), "Lote");
        AdicionarColuna(nameof(ItemEtiqueta.Deposito), "Depósito");
        AdicionarColuna(nameof(ItemEtiqueta.PesoLiquidoTexto), "Peso líquido");
        AdicionarColuna(nameof(ItemEtiqueta.QuantidadeEtiquetas), "Quantidade");
        AdicionarColuna(nameof(ItemEtiqueta.Colaborador), "Colaborador");

        _fila.ListChanged += (_, _) => AtualizarContadorFila();
        AtualizarContadorFila();
    }

    private void AdicionarColuna(string propriedade, string titulo)
    {
        dgvFila.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propriedade,
            HeaderText = titulo,
            Name = propriedade,
            ReadOnly = true,
        });
    }

    private async Task CarregarDadosIniciaisAsync()
    {
        AtualizarModoPesagem();
        AtualizarStatusBalanca();
        CarregarPortas();

        try
        {
            var produtos = await _sapService.BuscarProdutosAsync();
            PreencherComboProtegido(cmbProduto, produtos);

            var colaboradores = await _sapService.BuscarColaboradoresAsync();
            PreencherCombo(cmbColaborador, colaboradores);

            lblStatusSap.Text = "●  SAP  disponível";
            lblStatusSap.ForeColor = Color.FromArgb(0x2E, 0x7D, 0x32);
        }
        catch (Exception ex)
        {
            lblStatusSap.Text = "●  SAP  indisponível";
            lblStatusSap.ForeColor = Color.FromArgb(0xC6, 0x28, 0x28);
            MostrarErro("Não foi possível carregar os dados do SAP.", ex);
        }
    }

    private static void PreencherCombo(ComboBox combo, System.Collections.IEnumerable itens)
    {
        combo.DataSource = itens.Cast<object>().ToList();
        combo.SelectedIndex = -1;
        combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        combo.AutoCompleteSource = AutoCompleteSource.ListItems;
    }

    private void PreencherComboProtegido(ComboBox combo, System.Collections.IEnumerable itens)
    {
        _preenchendoCombos = true;
        try
        {
            PreencherCombo(combo, itens);
        }
        finally
        {
            _preenchendoCombos = false;
        }
    }

    private async Task CarregarLotesAsync()
    {
        if (_preenchendoCombos)
        {
            return;
        }

        if (cmbProduto.SelectedItem is not Produto produto)
        {
            cmbLote.DataSource = null;
            return;
        }

        try
        {
            var lotes = await _sapService.BuscarLotesAsync(produto.ProdutoId);
            PreencherCombo(cmbLote, lotes);
        }
        catch (Exception ex)
        {
            MostrarErro("Não foi possível carregar os lotes.", ex);
        }
    }

    // ---- Pesagem ----

    private void CarregarPortas()
    {
        var portaSelecionada = cmbPorta.SelectedItem as string;
        cmbPorta.DataSource = _balancaService.ListarPortas().ToList();

        if (portaSelecionada is not null)
        {
            cmbPorta.SelectedItem = portaSelecionada;
        }
        else
        {
            cmbPorta.SelectedIndex = cmbPorta.Items.Count > 0 ? 0 : -1;
        }
    }

    private void AtualizarModoPesagem()
    {
        var balanca = rbBalanca.Checked;

        lblRotuloPorta.Visible = balanca;
        pnlPorta.Visible = balanca;
        pnlManual.Visible = !balanca;

        // O peso bruto nunca é digitado: vem da balança ou é calculado no manual.
        txtPesoBruto.ReadOnly = true;
        txtPesoBruto.BackColor = Color.FromArgb(0xF5, 0xF7, 0xF9);

        if (!balanca && _balancaService.Conectada)
        {
            _balancaService.Desconectar();
            AtualizarStatusBalanca();
        }

        AtualizarCalculoPeso();
    }

    private void AlternarConexaoBalanca()
    {
        if (_balancaService.Conectada)
        {
            _balancaService.Desconectar();
            AtualizarStatusBalanca();
            return;
        }

        if (cmbPorta.SelectedItem is not string porta)
        {
            MostrarAviso("Selecione uma porta COM.");
            return;
        }

        try
        {
            _balancaService.Conectar(porta);
        }
        catch (Exception)
        {
            MostrarAviso(
                $"Não foi possível conectar à {porta}.\n" +
                "Verifique se a porta está disponível e tente novamente.");
        }

        AtualizarStatusBalanca();
    }

    private void OnPesoBalancaAlterado(object? sender, decimal peso)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnPesoBalancaAlterado(sender, peso));
            return;
        }

        if (rbBalanca.Checked)
        {
            txtPesoBruto.Text = DecimalHelper.ParaN3(peso);
        }
    }

    private void AtualizarStatusBalanca()
    {
        var conectada = _balancaService.Conectada;

        lblStatusBalanca.Text = conectada
            ? $"Balança conectada - {_balancaService.PortaAtual}"
            : "Balança desconectada";

        lblStatusBalanca.ForeColor = Color.FromArgb(0x24, 0x27, 0x2C);

        indicadorBalanca.BackColor = conectada
            ? Color.FromArgb(0x2E, 0x7D, 0x32)
            : Color.FromArgb(0xC6, 0x28, 0x28);
        indicadorBalanca.Invalidate();

        lblStatusBalancaGeral.Text = conectada
            ? $"●  Balança  conectada - {_balancaService.PortaAtual}"
            : "●  Balança  desconectada";
        lblStatusBalancaGeral.ForeColor = conectada
            ? Color.FromArgb(0x2E, 0x7D, 0x32)
            : Color.FromArgb(0xC6, 0x28, 0x28);

        btnConectar.Text = conectada ? "Desconectar" : "Conectar";
    }

    /// <summary>
    /// Cálculo central do peso. No modo balança, o peso bruto vem da balança;
    /// no modo manual, é calculado como pesoPorEmbalagem * quantidade.
    /// Em ambos: pesoLiquido = pesoBruto - tara.
    /// </summary>
    private void AtualizarCalculoPeso()
    {
        var bruto = CalcularPesoBruto();

        if (rbManual.Checked)
        {
            txtPesoBruto.Text = DecimalHelper.ParaN3(bruto);
        }

        var tara = DecimalHelper.Parse(txtTara.Text);
        var liquido = bruto - tara;

        lblPesoLiquido.Text = DecimalHelper.ParaN3(liquido < 0 ? 0 : liquido);
    }

    private decimal CalcularPesoBruto()
    {
        if (rbManual.Checked)
        {
            var pesoEmbalagem = DecimalHelper.Parse(txtPesoEmbalagem.Text);
            var quantidade = (int)nudQuantidadeEmbalagens.Value;
            return pesoEmbalagem * quantidade;
        }

        return DecimalHelper.Parse(txtPesoBruto.Text);
    }

    // ---- Fila ----

    private void AdicionarItemFila()
    {
        if (cmbProduto.SelectedItem is not Produto produto)
        {
            MostrarAviso("Selecione um produto.");
            return;
        }

        if (cmbLote.SelectedItem is not Lote lote)
        {
            MostrarAviso("Selecione um lote.");
            return;
        }

        if (cmbColaborador.SelectedItem is not Colaborador colaborador)
        {
            MostrarAviso("Selecione um colaborador.");
            return;
        }

        var bruto = CalcularPesoBruto();
        var tara = DecimalHelper.Parse(txtTara.Text);
        var liquido = bruto - tara;

        if (liquido <= 0)
        {
            MostrarAviso("O peso líquido deve ser maior que zero.");
            return;
        }

        _fila.Add(new ItemEtiqueta
        {
            Produto = produto,
            Lote = lote,
            Colaborador = colaborador.Nome,
            PesoBruto = bruto,
            Tara = tara,
            PesoLiquido = liquido,
            QuantidadeEtiquetas = (int)numQuantidade.Value,
        });
    }

    private void RemoverSelecionado()
    {
        if (dgvFila.CurrentRow?.DataBoundItem is ItemEtiqueta item)
        {
            _fila.Remove(item);
        }
    }

    private void LimparFila()
    {
        if (_fila.Count == 0)
        {
            return;
        }

        var confirmar = MessageBox.Show(
            "Deseja limpar toda a fila de impressão?",
            "Confirmar",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmar == DialogResult.Yes)
        {
            _fila.Clear();
        }
    }

    private async Task ImprimirFilaAsync()
    {
        if (_fila.Count == 0)
        {
            MostrarAviso("A fila está vazia.");
            return;
        }

        try
        {
            btnImprimir.Enabled = false;

            var zpl = _etiquetaService.GerarZpl(_fila);
            await _impressoraService.ImprimirAsync(zpl);

            _fila.Clear();
            MessageBox.Show(
                "Etiquetas enviadas para impressão.",
                "Sucesso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MostrarErro("Falha ao imprimir as etiquetas.", ex);
        }
        finally
        {
            btnImprimir.Enabled = true;
        }
    }

    private void AtualizarContadorFila()
    {
        var total = _fila.Sum(i => i.QuantidadeEtiquetas);
        lblContadorFila.Text = total == 1 ? "1 etiqueta" : $"{total} etiquetas";
    }

    // ---- Mensagens ----

    private static void MostrarAviso(string mensagem) =>
        MessageBox.Show(mensagem, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void MostrarErro(string mensagem, Exception ex) =>
        MessageBox.Show(
            $"{mensagem}\n\n{ex.Message}",
            "Erro",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
}
