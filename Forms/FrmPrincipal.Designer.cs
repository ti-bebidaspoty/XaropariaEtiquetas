using System.ComponentModel;

namespace XaropariaEtiquetas.Forms;

partial class FrmPrincipal
{
    private System.ComponentModel.IContainer components = null;

    // Paleta
    private static readonly Color CorFundo = Color.FromArgb(0xF6, 0xF7, 0xF9);
    private static readonly Color CorCard = Color.White;
    private static readonly Color CorTextoPrincipal = Color.FromArgb(0x24, 0x27, 0x2C);
    private static readonly Color CorTextoSecundario = Color.FromArgb(0x74, 0x7A, 0x82);
    private static readonly Color CorVerde = Color.FromArgb(0x2E, 0x7D, 0x32);
    private static readonly Color CorVerdeClaro = Color.FromArgb(0xEA, 0xF4, 0xEC);
    private static readonly Color CorVermelho = Color.FromArgb(0xC6, 0x28, 0x28);
    private static readonly Color CorVermelhoClaro = Color.FromArgb(0xFD, 0xEC, 0xEC);
    private static readonly Color CorBorda = Color.FromArgb(0xE5, 0xE7, 0xEB);
    private static readonly Color CorDisplay = Color.FromArgb(0xEE, 0xF2, 0xF5);

    private TableLayoutPanel layoutRaiz;

    private Panel cardCabecalho;
    private PictureBox picLogo;
    private Label lblTitulo;
    private Label lblSubtitulo;
    private Label lblStatusSap;
    private Label lblStatusBalancaGeral;
    private Label lblStatusImpressora;

    private Panel cardProduto;
    private ComboBox cmbProduto;
    private ComboBox cmbLote;
    private ComboBox cmbColaborador;

    private Panel cardPesagem;
    private RadioButton rbBalanca;
    private RadioButton rbManual;
    private Label lblRotuloPorta;
    private FlowLayoutPanel pnlPorta;
    private ComboBox cmbPorta;
    private Button btnConectar;
    private FlowLayoutPanel pnlManual;
    private TextBox txtPesoEmbalagem;
    private NumericUpDown nudQuantidadeEmbalagens;
    private TextBox txtPesoBruto;
    private TextBox txtTara;
    private Label lblPesoLiquido;
    private Panel pnlStatusBalanca;
    private Panel indicadorBalanca;
    private Label lblStatusBalanca;

    private Panel cardImpressao;
    private NumericUpDown numQuantidade;
    private Button btnAdicionar;

    private Panel cardFila;
    private Label lblContadorFila;
    private DataGridView dgvFila;
    private Button btnLimpar;
    private Button btnLimparFormulario;
    private Button btnImprimir;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();

        SuspendLayout();

        // ---- Form ----
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Xaroparia Etiquetas - Controle de pesagem e impressão";
        ClientSize = new Size(1260, 700);
        MinimumSize = new Size(1280, 700);
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = CorFundo;
        Font = new Font("Segoe UI", 10F);
        ForeColor = CorTextoPrincipal;
        Padding = new Padding(10);
        CarregarIconeAplicacao();

        // ---- Base 1366x768: container central fixo 1240 x 640, centralizado horizontalmente ----
        layoutRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = CorFundo,
        };
        layoutRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layoutRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1240F));
        layoutRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layoutRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 676F));
        layoutRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = CorFundo,
            Margin = new Padding(0),
        };
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 252F));  // produto | pesagem
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));   // impressão
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 298F));  // fila

        ConstruirCabecalho();
        ConstruirCardProduto();
        ConstruirCardPesagem();
        ConstruirCardImpressao();
        ConstruirCardFila();

        // ---- Conteúdo superior: 2 colunas (esquerda = Produto + Impressão, direita = Pesagem) ----
        var painelSuperior = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = CorFundo,
            Margin = new Padding(0),
        };
        painelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        painelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        painelSuperior.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        painelSuperior.Controls.Add(cardProduto, 0, 0);
        painelSuperior.Controls.Add(cardPesagem, 1, 0);

        conteudo.Controls.Add(cardCabecalho, 0, 0);
        conteudo.Controls.Add(painelSuperior, 0, 1);
        conteudo.Controls.Add(cardImpressao, 0, 2);
        conteudo.Controls.Add(cardFila, 0, 3);
        layoutRaiz.Controls.Add(conteudo, 1, 0);

        Controls.Add(layoutRaiz);

        ResumeLayout(false);
    }

    private static Panel CriarCard()
    {
        var card = new Panel
        {
            BackColor = CorCard,
            Margin = new Padding(4),
            Padding = new Padding(14, 10, 14, 10),
            Dock = DockStyle.Fill,
            AutoSize = false,
        };
        AplicarCantosArredondados(card, 12);
        return card;
    }

    private static Panel CriarCardAutoAltura()
    {
        var card = new Panel
        {
            BackColor = CorCard,
            Margin = new Padding(4),
            Padding = new Padding(14, 10, 14, 10),
            Dock = DockStyle.Fill,
            AutoSize = false,
        };
        AplicarCantosArredondados(card, 12);
        return card;
    }

    /// <summary>
    /// Aplica cantos arredondados e uma borda quase imperceptível a um painel,
    /// reagindo a mudanças de tamanho. Simples e reutilizável, sem biblioteca externa.
    /// </summary>
    private static void AplicarCantosArredondados(Panel painel, int raio)
    {
        void Atualizar()
        {
            if (painel.Width <= 0 || painel.Height <= 0)
            {
                return;
            }
            painel.Region = new Region(CriarCaminhoArredondado(painel.ClientRectangle, raio));
        }

        painel.Resize += (_, _) => Atualizar();
        painel.HandleCreated += (_, _) => Atualizar();

        painel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = painel.ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            using var caminho = CriarCaminhoArredondado(r, raio);
            using var pen = new Pen(CorBorda);
            e.Graphics.DrawPath(pen, caminho);
        };
    }

    private static System.Drawing.Drawing2D.GraphicsPath CriarCaminhoArredondado(Rectangle bounds, int raio)
    {
        var diametro = raio * 2;
        var caminho = new System.Drawing.Drawing2D.GraphicsPath();

        if (diametro <= 0)
        {
            caminho.AddRectangle(bounds);
            caminho.CloseFigure();
            return caminho;
        }

        var arco = new Rectangle(bounds.Location, new Size(diametro, diametro));
        caminho.AddArc(arco, 180, 90);
        arco.X = bounds.Right - diametro;
        caminho.AddArc(arco, 270, 90);
        arco.Y = bounds.Bottom - diametro;
        caminho.AddArc(arco, 0, 90);
        arco.X = bounds.Left;
        caminho.AddArc(arco, 90, 90);
        caminho.CloseFigure();
        return caminho;
    }

    private static Label CriarTituloSecao(string texto)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13F),
            ForeColor = CorTextoPrincipal,
            Margin = new Padding(0, 0, 0, 4),
        };
    }

    private static Label CriarRotulo(string texto)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = CorTextoSecundario,
            Margin = new Padding(0, 2, 0, 2),
        };
    }

    private static ComboBox CriarCombo()
    {
        return new ComboBox
        {
            Font = new Font("Segoe UI", 11F),
            FlatStyle = FlatStyle.Flat,
            Height = 38,
            Width = 420,
            Margin = new Padding(0, 0, 0, 6),
        };
    }

    // Cores de borda dos campos de seleção
    private static readonly Color CorBordaCampo = Color.FromArgb(0xD8, 0xDE, 0xE5);
    private static readonly Color CorBordaCampoFoco = Color.FromArgb(0x9D, 0xB8, 0xA1);

    /// <summary>
    /// Cria um ComboBox já envolvido em um Panel com borda leve e visível.
    /// A borda fica levemente destacada quando o campo recebe foco.
    /// Reutilizável para Produto, Lote, Colaborador e Porta.
    /// </summary>
    private Panel CriarCampoCombo(out ComboBox combo, bool dropDownList = false)
    {
        combo = new ComboBox
        {
            Font = new Font("Segoe UI", 10F),
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = CorTextoPrincipal,
            MaxDropDownItems = 12,
            DropDownStyle = dropDownList ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown,
            Margin = new Padding(0),
        };

        var container = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(8, 5, 6, 5),
            Height = 34,
            Margin = new Padding(0, 0, 0, 2),
        };
        ConfigurarBordaCampo(container, combo);
        container.Controls.Add(combo);
        return container;
    }

    /// <summary>
    /// Desenha a borda do container e alterna a cor no foco do campo interno.
    /// Comportamento reutilizável, sem duplicar código por campo.
    /// </summary>
    private static void ConfigurarBordaCampo(Panel container, Control campo)
    {
        var comFoco = false;

        container.Paint += (s, e) =>
        {
            var r = container.ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            using var pen = new Pen(comFoco ? CorBordaCampoFoco : CorBordaCampo);
            e.Graphics.DrawRectangle(pen, r);
        };

        campo.GotFocus += (_, _) => { comFoco = true; container.Invalidate(); };
        campo.LostFocus += (_, _) => { comFoco = false; container.Invalidate(); };
        campo.Enter += (_, _) => { comFoco = true; container.Invalidate(); };
        campo.Leave += (_, _) => { comFoco = false; container.Invalidate(); };
    }

    private void EstilizarBotaoPrincipal(Button botao)
    {
        botao.FlatStyle = FlatStyle.Flat;
        botao.FlatAppearance.BorderSize = 0;
        botao.BackColor = CorVerde;
        botao.ForeColor = Color.White;
        botao.Font = new Font("Segoe UI Semibold", 10F);
        botao.Height = 34;
        botao.Cursor = Cursors.Hand;
        botao.Padding = new Padding(18, 0, 18, 0);
        botao.AutoSize = true;
    }

    private void EstilizarBotaoSecundario(Button botao)
    {
        botao.FlatStyle = FlatStyle.Flat;
        botao.FlatAppearance.BorderSize = 1;
        botao.FlatAppearance.BorderColor = CorBorda;
        botao.BackColor = Color.White;
        botao.ForeColor = CorTextoPrincipal;
        botao.Font = new Font("Segoe UI", 10F);
        botao.Height = 34;
        botao.Cursor = Cursors.Hand;
        botao.Padding = new Padding(16, 0, 16, 0);
        botao.AutoSize = true;
    }

    private void ConstruirCabecalho()
    {
        cardCabecalho = CriarCard();
        cardCabecalho.Padding = new Padding(18, 6, 18, 6);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = true,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        picLogo = new PictureBox
        {
            Size = new Size(46, 46),
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(0, 0, 18, 0),
            Anchor = AnchorStyles.Left,
            BackColor = Color.Transparent,
        };
        CarregarLogo();
        layout.Controls.Add(picLogo, 0, 0);

        var pilhaTitulo = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0),
        };

        lblTitulo = new Label
        {
            Text = "Xaroparia Etiquetas",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F),
            ForeColor = CorTextoPrincipal,
            Margin = new Padding(0),
        };

        lblSubtitulo = new Label
        {
            Text = "Controle de pesagem e impressão",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = CorTextoSecundario,
            Margin = new Padding(0, 2, 0, 0),
        };

        pilhaTitulo.Controls.Add(lblTitulo);
        pilhaTitulo.Controls.Add(lblSubtitulo);
        layout.Controls.Add(pilhaTitulo, 1, 0);

        var pilhaStatus = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
        };

        lblStatusSap = CriarBadgeStatus("SAP  verificando...");
        lblStatusBalancaGeral = CriarBadgeStatus("Balança  desconectada");
        lblStatusImpressora = CriarBadgeStatus("Impressora  disponível");

        pilhaStatus.Controls.Add(lblStatusSap);
        pilhaStatus.Controls.Add(lblStatusBalancaGeral);
        pilhaStatus.Controls.Add(lblStatusImpressora);
        layout.Controls.Add(pilhaStatus, 2, 0);

        cardCabecalho.Controls.Add(layout);
    }

    private static Label CriarBadgeStatus(string texto)
    {
        return new Label
        {
            Text = "●  " + texto,
            AutoSize = true,
            Font = new Font("Segoe UI", 9F),
            ForeColor = CorTextoSecundario,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0, 0, 0, 0),
        };
    }

    private void CarregarLogo()
    {
        try
        {
            var caminho = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
            if (System.IO.File.Exists(caminho))
            {
                picLogo.Image = Image.FromFile(caminho);
            }
            else
            {
                picLogo.BackColor = CorVerdeClaro;
            }
        }
        catch
        {
            picLogo.BackColor = CorVerdeClaro;
        }
    }

    private void CarregarIconeAplicacao()
    {
        try
        {
            var caminho = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
            if (System.IO.File.Exists(caminho))
            {
                Icon = new Icon(caminho);
            }
        }
        catch
        {
            // Ícone opcional; mantém o padrão caso não exista.
        }
    }

    private void ConstruirCardProduto()
    {
        cardProduto = CriarCard();

        var pilha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            AutoSize = false,
        };
        pilha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // título
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // rótulo produto
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // combo produto
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // rótulo lote
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // combo lote
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // rótulo colaborador
        pilha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // combo colaborador
        pilha.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // respiro

        var campoProduto = CriarCampoCombo(out cmbProduto);
        campoProduto.Dock = DockStyle.Fill;
        campoProduto.Margin = new Padding(0, 0, 0, 10);

        var campoLote = CriarCampoCombo(out cmbLote);
        campoLote.Dock = DockStyle.Fill;
        campoLote.Margin = new Padding(0, 0, 0, 10);

        var campoColaborador = CriarCampoCombo(out cmbColaborador);
        campoColaborador.Dock = DockStyle.Fill;
        campoColaborador.Margin = new Padding(0, 0, 0, 2);

        // cmbProduto abre uma lista mais larga para descrições longas,
        // sem ultrapassar o conforto em telas menores.
        cmbProduto.DropDownWidth = 650;
        cmbLote.DropDownWidth = 480;
        cmbColaborador.DropDownWidth = 420;

        var rotuloProduto = CriarRotulo("Produto");
        rotuloProduto.Margin = new Padding(0, 2, 0, 6);
        var rotuloLote = CriarRotulo("Lote");
        rotuloLote.Margin = new Padding(0, 0, 0, 6);
        var rotuloColaborador = CriarRotulo("Colaborador");
        rotuloColaborador.Margin = new Padding(0, 0, 0, 6);

        pilha.Controls.Add(CriarTituloSecao("Produto"));
        pilha.Controls.Add(rotuloProduto);
        pilha.Controls.Add(campoProduto);
        pilha.Controls.Add(rotuloLote);
        pilha.Controls.Add(campoLote);
        pilha.Controls.Add(rotuloColaborador);
        pilha.Controls.Add(campoColaborador);

        cardProduto.Controls.Add(pilha);
    }

    private void ConstruirCardPesagem()
    {
        cardPesagem = CriarCard();

        var pilha = new TableLayoutPanel
        {
            ColumnCount = 1,
            AutoSize = false,
            Dock = DockStyle.Fill,
        };
        pilha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        pilha.Controls.Add(CriarTituloSecao("Pesagem"));

        var pnlModo = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 2),
        };
        rbBalanca = new RadioButton
        {
            Text = "Balança",
            Checked = true,
            AutoSize = true,
            Font = new Font("Segoe UI", 11F),
            Margin = new Padding(0, 0, 24, 0),
        };
        rbManual = new RadioButton
        {
            Text = "Manual",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F),
        };
        pnlModo.Controls.Add(rbBalanca);
        pnlModo.Controls.Add(rbManual);
        pilha.Controls.Add(pnlModo);

        lblRotuloPorta = CriarRotulo("Porta");
        pilha.Controls.Add(lblRotuloPorta);
        pnlPorta = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 4),
        };
        cmbPorta = new ComboBox
        {
            Font = new Font("Segoe UI", 10F),
            FlatStyle = FlatStyle.Flat,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.White,
            ForeColor = CorTextoPrincipal,
            Dock = DockStyle.Fill,
            MaxDropDownItems = 12,
            Margin = new Padding(0),
        };
        var containerPorta = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(8, 5, 6, 5),
            Height = 34,
            Width = 150,
            Margin = new Padding(0, 0, 8, 0),
        };
        ConfigurarBordaCampo(containerPorta, cmbPorta);
        containerPorta.Controls.Add(cmbPorta);
        btnConectar = new Button { Text = "Conectar" };
        EstilizarBotaoSecundario(btnConectar);
        pnlPorta.Controls.Add(containerPorta);
        pnlPorta.Controls.Add(btnConectar);
        pilha.Controls.Add(pnlPorta);

        // Campos exclusivos do modo manual: peso por embalagem e quantidade.
        pnlManual = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 8),
            Visible = false,
        };
        var pnlPesoEmbalagem = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 24, 0),
        };
        pnlPesoEmbalagem.Controls.Add(CriarRotulo("Peso por embalagem"));
        txtPesoEmbalagem = new TextBox
        {
            Text = "0,000",
            Font = new Font("Segoe UI", 10F),
            Width = 150,
            Margin = new Padding(0, 0, 0, 0),
        };
        pnlPesoEmbalagem.Controls.Add(txtPesoEmbalagem);
        pnlManual.Controls.Add(pnlPesoEmbalagem);

        var pnlQtdEmbalagens = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
        };
        pnlQtdEmbalagens.Controls.Add(CriarRotulo("Quantidade de embalagens"));
        nudQuantidadeEmbalagens = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 9999,
            Value = 1,
            Font = new Font("Segoe UI", 11F),
            Width = 120,
        };
        pnlQtdEmbalagens.Controls.Add(nudQuantidadeEmbalagens);
        pnlManual.Controls.Add(pnlQtdEmbalagens);

        pilha.Controls.Add(pnlManual);

        // Displays de peso - estilo moderno, sem caixas pesadas
        var grade = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4),
        };
        grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4F));

        grade.Controls.Add(CriarRotuloDisplay("Peso bruto"), 0, 0);
        grade.Controls.Add(CriarRotuloDisplay("Tara"), 1, 0);
        grade.Controls.Add(CriarRotuloDisplay("Peso líquido"), 2, 0);

        txtPesoBruto = CriarDisplayPeso(somenteLeitura: true);
        txtTara = CriarDisplayPeso(somenteLeitura: false);
        txtTara.Text = "0,000";
        lblPesoLiquido = new Label
        {
            Text = "0,000",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 44,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 24F),
            ForeColor = CorVerde,
            BackColor = Color.Transparent,
            Margin = new Padding(4, 2, 4, 2),
        };

        grade.Controls.Add(txtPesoBruto, 0, 1);
        grade.Controls.Add(txtTara, 1, 1);
        grade.Controls.Add(lblPesoLiquido, 2, 1);

        grade.Controls.Add(CriarUnidadeKg(), 0, 2);
        grade.Controls.Add(CriarUnidadeKg(), 1, 2);
        grade.Controls.Add(CriarUnidadeKg(), 2, 2);

        pilha.Controls.Add(grade);

        // Status da balança - indicador discreto, sem caixa pesada
        pnlStatusBalanca = new Panel
        {
            AutoSize = true,
            Padding = new Padding(0, 2, 0, 0),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 2, 0, 0),
        };
        var pilhaStatusBal = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0),
        };
        indicadorBalanca = new Panel
        {
            Width = 10,
            Height = 10,
            BackColor = CorVermelho,
            Margin = new Padding(0, 5, 8, 0),
        };
        indicadorBalanca.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(indicadorBalanca.BackColor);
            e.Graphics.FillEllipse(brush, 0, 0, indicadorBalanca.Width - 1, indicadorBalanca.Height - 1);
        };
        lblStatusBalanca = new Label
        {
            Text = "Balança desconectada",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = CorTextoPrincipal,
            Margin = new Padding(0, 1, 0, 0),
        };
        pilhaStatusBal.Controls.Add(indicadorBalanca);
        pilhaStatusBal.Controls.Add(lblStatusBalanca);
        pnlStatusBalanca.Controls.Add(pilhaStatusBal);
        pilha.Controls.Add(pnlStatusBalanca);

        cardPesagem.Controls.Add(pilha);
    }

    private static Label CriarRotuloDisplay(string texto)
    {
        return new Label
        {
            Text = texto,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = CorTextoSecundario,
            Margin = new Padding(4, 0, 4, 2),
        };
    }

    private static Label CriarUnidadeKg()
    {
        return new Label
        {
            Text = "kg",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 18,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F),
            ForeColor = CorTextoSecundario,
            Margin = new Padding(4, 0, 4, 0),
        };
    }

    private TextBox CriarDisplayPeso(bool somenteLeitura)
    {
        return new TextBox
        {
            Text = "0,000",
            ReadOnly = somenteLeitura,
            TextAlign = HorizontalAlignment.Center,
            Font = new Font("Segoe UI Semibold", 22F),
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Height = 44,
            Margin = new Padding(4, 2, 4, 2),
            BackColor = Color.White,
            ForeColor = CorTextoPrincipal,
        };
    }

    private void ConstruirCardImpressao()
    {
        cardImpressao = CriarCard();
        cardImpressao.Padding = new Padding(14, 8, 14, 8);

        var linha = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = new Padding(0),
        };

        var rotuloQtd = CriarRotulo("Quantidade de etiquetas");
        rotuloQtd.Margin = new Padding(0, 7, 10, 0);
        linha.Controls.Add(rotuloQtd);
        numQuantidade = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            Font = new Font("Segoe UI", 10F),
            Width = 80,
            Margin = new Padding(0, 3, 16, 0),
        };
        linha.Controls.Add(numQuantidade);

        btnAdicionar = new Button { Text = "Adicionar à fila" };
        EstilizarBotaoPrincipal(btnAdicionar);
        btnAdicionar.Margin = new Padding(0, 0, 0, 0);
        linha.Controls.Add(btnAdicionar);

        btnLimparFormulario = new Button { Text = "Limpar formulário" };
        EstilizarBotaoSecundario(btnLimparFormulario);
        btnLimparFormulario.Margin = new Padding(10, 0, 0, 0);
        linha.Controls.Add(btnLimparFormulario);

        cardImpressao.Controls.Add(linha);
    }

    private void ConstruirCardFila()
    {
        cardFila = CriarCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));

        var cabecalhoFila = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
        };
        cabecalhoFila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cabecalhoFila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        cabecalhoFila.Controls.Add(CriarTituloSecao("Fila de impressão"), 0, 0);

        lblContadorFila = new Label
        {
            Text = "0 etiquetas",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Font = new Font("Segoe UI", 10F),
            ForeColor = CorTextoSecundario,
        };
        cabecalhoFila.Controls.Add(lblContadorFila, 1, 0);
        layout.Controls.Add(cabecalhoFila, 0, 0);

        // Container com altura limitada para a fila, com scroll próprio do grid.
        var containerGrid = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 0, 0),
        };
        dgvFila = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Color.White,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 10F),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 30 },
            GridColor = CorBorda,
        };
        dgvFila.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        dgvFila.ColumnHeadersDefaultCellStyle.ForeColor = CorTextoSecundario;
        dgvFila.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        dgvFila.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgvFila.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgvFila.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvFila.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgvFila.DefaultCellStyle.SelectionBackColor = CorVerdeClaro;
        dgvFila.DefaultCellStyle.SelectionForeColor = CorTextoPrincipal;
        dgvFila.RowsDefaultCellStyle.BackColor = Color.White;
        containerGrid.Controls.Add(dgvFila);
        layout.Controls.Add(containerGrid, 0, 1);

        var rodape = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Margin = new Padding(0, 6, 0, 0),
        };
        btnImprimir = new Button { Text = "Imprimir etiquetas" };
        EstilizarBotaoPrincipal(btnImprimir);
        btnLimpar = new Button { Text = "Limpar fila" };
        EstilizarBotaoSecundario(btnLimpar);
        var btnRemover = new Button { Text = "Remover selecionado", Name = "btnRemover" };
        EstilizarBotaoSecundario(btnRemover);
        this.btnRemover = btnRemover;

        rodape.Controls.Add(btnImprimir);
        rodape.Controls.Add(btnLimpar);
        rodape.Controls.Add(btnRemover);
        layout.Controls.Add(rodape, 0, 2);

        cardFila.Controls.Add(layout);
    }

    private Button btnRemover;
}
