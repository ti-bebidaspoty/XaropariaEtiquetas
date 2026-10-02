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

    // Listas originais mantidas em memória; o filtro dos combos acontece sobre elas (sem novas consultas ao SAP).
    private List<Produto> _todosProdutos = [];
    private List<Lote> _todosLotes = [];
    private List<Colaborador> _todosColaboradores = [];
    private bool _filtrandoCombo;
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

        cmbProduto.TextUpdate += (_, _) => FiltrarProdutos(cmbProduto.Text);
        cmbLote.TextUpdate += (_, _) => FiltrarLotes(cmbLote.Text);
        cmbColaborador.TextUpdate += (_, _) => FiltrarColaboradores(cmbColaborador.Text);
        cmbProduto.KeyDown += ConfirmarComEnter;
        cmbLote.KeyDown += ConfirmarComEnter;
        cmbColaborador.KeyDown += ConfirmarComEnter;
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
        btnLimparFormulario.Click += (_, _) => LimparFormulario();
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
            _todosProdutos = produtos.ToList();
            PreencherComboPesquisavel(cmbProduto, _todosProdutos);

            var colaboradores = await _sapService.BuscarColaboradoresAsync();
            _todosColaboradores = colaboradores.ToList();
            PreencherComboPesquisavel(cmbColaborador, _todosColaboradores);

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

    private void PreencherComboPesquisavel<T>(ComboBox combo, List<T> itens) where T : class
    {
        _preenchendoCombos = true;
        try
        {
            combo.AutoCompleteMode = AutoCompleteMode.None;
            combo.AutoCompleteSource = AutoCompleteSource.None;
            combo.DataSource = null;
            combo.Items.Clear();
            combo.Items.AddRange([.. itens]);
            combo.SelectedIndex = -1;
        }
        finally
        {
            _preenchendoCombos = false;
        }
    }

    private void FiltrarProdutos(string texto) =>
        FiltrarCombo(cmbProduto, _todosProdutos, texto, p =>
            p.ProdutoId.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || p.Descricao.Contains(texto, StringComparison.OrdinalIgnoreCase));

    private void FiltrarLotes(string texto) =>
        FiltrarCombo(cmbLote, _todosLotes, texto, l =>
            l.LoteNumero.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || l.Deposito.Contains(texto, StringComparison.OrdinalIgnoreCase));

    private void FiltrarColaboradores(string texto) =>
        FiltrarCombo(cmbColaborador, _todosColaboradores, texto, c =>
            c.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase));

    private void FiltrarCombo<T>(ComboBox combo, List<T> origem, string texto, Func<T, bool> corresponde) where T : class
    {
        if (_filtrandoCombo || _preenchendoCombos)
        {
            return;
        }

        // Texto atualizado por seleção na lista: não filtra de novo.
        if (combo.SelectedItem is not null && combo.SelectedItem.ToString() == texto)
        {
            return;
        }

        var cursor = combo.SelectionStart;
        _filtrandoCombo = true;
        try
        {
            var filtrados = string.IsNullOrWhiteSpace(texto)
                ? origem
                : origem.Where(corresponde).ToList();

            combo.BeginUpdate();
            combo.Items.Clear();
            combo.Items.AddRange([.. filtrados]);
            combo.EndUpdate();

            combo.Text = texto;
            combo.SelectionStart = Math.Min(cursor, texto.Length);
            combo.SelectionLength = 0;

            Cursor.Current = Cursors.Default;
            combo.DroppedDown = filtrados.Count > 0;
            combo.SelectionStart = Math.Min(cursor, texto.Length);
            combo.SelectionLength = 0;
        }
        finally
        {
            _filtrandoCombo = false;
        }
    }

    private void ConfirmarComEnter(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter || sender is not ComboBox combo)
        {
            return;
        }

        if (combo.SelectedIndex < 0 && combo.Items.Count == 1)
        {
            combo.SelectedIndex = 0;
        }

        if (combo.DroppedDown)
        {
            combo.DroppedDown = false;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
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
        if (_preenchendoCombos || _filtrandoCombo)
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
            _todosLotes = lotes.ToList();
            PreencherComboPesquisavel(cmbLote, _todosLotes);
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

    private void LimparFormulario()
    {
        // Produto, Lote e Colaborador (restaura as listas completas, sem consultar o SAP)
        PreencherComboPesquisavel(cmbProduto, _todosProdutos);
        PreencherComboPesquisavel(cmbColaborador, _todosColaboradores);
        _todosLotes = [];
        PreencherComboPesquisavel(cmbLote, _todosLotes);
        cmbProduto.Text = string.Empty;
        cmbLote.Text = string.Empty;
        cmbColaborador.Text = string.Empty;

        numQuantidade.Value = 1;
        txtTara.Text = DecimalHelper.ParaN3(0);

        if (rbManual.Checked)
        {
            txtPesoEmbalagem.Text = DecimalHelper.ParaN3(0);
            nudQuantidadeEmbalagens.Value = 1;
        }

        // Modo balança: o peso bruto continua vindo da balança; apenas recalcula o líquido.
        AtualizarCalculoPeso();
        cmbProduto.Focus();
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

            MessageBox.Show(
                "Etiquetas enviadas para impressão com sucesso.",
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
