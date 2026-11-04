using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using curseuch;
namespace curseuch
{
    public partial class Models : Form
    {
        #region 1. Поля формы и состояние программы

        private readonly OpenFileDialog _openFileDialog;
        private curseuch _engine;
        private string _databasePath = string.Empty;
        private string _currentTableName = string.Empty;
        private DataTable _sourceTable;
        private List<MeasurementCycle> _cycles = new List<MeasurementCycle>();
        private TabControl tabControlMain;
        private TabPage tabPageData;
        private TabPage tabPageLevel1;
        private TabPage tabPageLevel2;
        private TabPage tabPageLevel3;
        private TabPage tabPageLevel4;
        private Button buttonOpenDb;
        private Button buttonLoadTable;
        private Button buttonAddRow;
        private Button buttonDeleteRow;
        private Button buttonSaveTable;
        private Button buttonReplaceScheme;
        private Button buttonLevel1;
        private ComboBox comboBoxTables;
        private DataGridView dataGridViewSource;
        private DataGridView dataGridViewLevel1;
        private Panel panelSchemeScroll;
        private PictureBox pictureBoxScheme;
        private PictureBox pictureBoxLevel2Scheme;
        private double _schemeZoom = 1.0;
        private TextBox textBoxEpsilon;
        private TextBox textBoxAlpha;
        private Label labelDb;
        private Label labelTables;
        private Label labelEpsilon;
        private Label labelAlpha;
        private Label labelSource;
        private Label labelImage;
        private Label labelLevel1;
        private TableLayoutPanel mainDataLayout;
        private TableLayoutPanel dataTopPanel;
        private TableLayoutPanel dataContentLayout;
        private TableLayoutPanel level1Layout;
        private Button buttonShowLevel1Charts;
        private Level1AnalysisResult _lastLevel1Result;
        private TabControl tabControlLevel2;
        private TabPage tabPageLevel2Distribution;
        private TabPage tabPageLevel2Calculation;
        private Label labelBlocksCount;
        private NumericUpDown numericUpDownBlocksCount;
        private Button buttonCreateBlocks;
        private ComboBox comboBoxBlocks;
        private ListBox listBoxAllPoints;
        private ListBox listBoxBlockPoints;
        private Button buttonMoveToBlock;
        private Button buttonRemoveFromBlock;
        private Button buttonAutoDistribute;
        private Button buttonConfirmBlocks;
        private Label labelRequiredCount;
        private Label labelAllPoints;
        private Label labelBlockPoints;
        private Label labelChooseBlock;
        private Button buttonLevel2;
        private Button buttonShowLevel2Charts;
        private DataGridView dataGridViewLevel2;
        private Dictionary<string, List<string>> _blockData = new Dictionary<string, List<string>>();
        private bool _blocksConfirmed = false;
        private Level2AnalysisResult _lastLevel2Result;
        private ComboBox comboBoxLevel3Blocks;
        private Button buttonLevel3;
        private Button buttonShowLevel3Charts;
        private DataGridView dataGridViewLevel3Links;
        private DataGridView dataGridViewLevel3Clusters;
        private DataGridView dataGridViewLevel3Standalone;
        private Label labelLevel3Block;
        private Label labelLevel3Tolerance;
        private Label labelLevel3Summary;
        private NumericUpDown numericUpDownLevel3Tolerance;
        private Level3StructuralResult _lastLevel3Result;
        private ComboBox comboBoxLevel3Clusters;
        private Button buttonLevel3Response;
        private Button buttonShowLevel3ResponseCharts;
        private DataGridView dataGridViewLevel3Response;
        private Level3ResponseResult _lastLevel3ResponseResult;
        private TabControl tabControlLevel3;
        private TabPage tabPageLevel3Distribution;
        private TabPage tabPageLevel3Calculation;
        private ListBox listBoxLevel3AvailableBlockPoints;
        private ListBox listBoxLevel3BlockPoints;
        private Button buttonLevel3AddPointToBlock;
        private Button buttonLevel3RemovePointFromBlock;
        private NumericUpDown numericUpDownLevel3SubblocksCount;
        private NumericUpDown numericUpDownLevel3PointsPerSubblock;
        private Button buttonLevel3CreateSubblocks;
        private ComboBox comboBoxLevel3Subblocks;
        private ListBox listBoxLevel3FreeSubblockPoints;
        private ListBox listBoxLevel3SelectedSubblockPoints;
        private Button buttonLevel3MoveToSubblock;
        private Button buttonLevel3RemoveFromSubblock;
        private Button buttonLevel3AutoDistribute;
        private Button buttonLevel3ConfirmSubblocks;
        private Label labelLevel3SubblockRequired;
        private DataGridView dataGridViewLevel3Differences;
        private Dictionary<string, List<string>> _level3SubblockData = new Dictionary<string, List<string>>();
        private List<string> _level3PreparedBlockPoints = new List<string>();
        private string _level3PreparedBlockName = string.Empty;
        private bool _level3BlockPrepared = false;
        private bool _level3SubblocksConfirmed = false;
        private CheckedListBox checkedListBoxLevel4Points;
        private RadioButton radioButtonLevel4AllPoints;
        private RadioButton radioButtonLevel4StandalonePoints;
        private Chart chartLevel4Preview;
        private Button buttonLevel4;
        private Button buttonLevel4SelectAll;
        private Button buttonLevel4ClearSelection;
        private Button buttonShowLevel4Charts;
        private DataGridView dataGridViewLevel4;
        private Label labelLevel4Summary;
        private Level4AnalysisResult _lastLevel4Result;
        private List<string> _lastLevel4SelectedPoints = new List<string>();
        private TableLayoutPanel rootLayout;
        private FlowLayoutPanel topHelpPanel;
        private Button buttonHelp;
        #endregion

        #region 2. Инициализация формы и вкладок

        public Models()
        {
            InitializeComponent();
            InitializeFormSettings();
            _openFileDialog = new OpenFileDialog
            {
                Filter = "SQLite Database files (*.db;*.sqlite)|*.db;*.sqlite|All files (*.*)|*.*",
                Title = "Выберите файл базы данных"
            };
            InitializeEngine();
            InitializeUi();
            BindEvents();
        }
        private void InitializeFormSettings()
        {
            Text = "curseuch - анализ техногенного объекта";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1300, 850);
            Size = new Size(1500, 920);
        }
        private void InitializeEngine()
        {
            _engine = new curseuch();
        }
        private void InitializeUi()
        {
            rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            topHelpPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(10, 8, 10, 4)
            };

            buttonHelp = new Button
            {
                Name = "buttonHelp",
                Text = "Справка",
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            topHelpPanel.Controls.Add(buttonHelp);

            tabControlMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            tabPageData = new TabPage("Данные");
            tabPageLevel1 = new TabPage("I уровень декомпозиции");
            tabPageLevel2 = new TabPage("II уровень декомпозиции");
            tabPageLevel3 = new TabPage("III уровень декомпозиции");
            tabPageLevel4 = new TabPage("IV уровень декомпозиции");
            tabControlMain.TabPages.Add(tabPageData);
            tabControlMain.TabPages.Add(tabPageLevel1);
            tabControlMain.TabPages.Add(tabPageLevel2);
            tabControlMain.TabPages.Add(tabPageLevel3);
            tabControlMain.TabPages.Add(tabPageLevel4);
            InitializeDataTab();
            InitializeLevel1Tab();
            InitializePlaceholderTabs();
            rootLayout.Controls.Add(topHelpPanel, 0, 0);
            rootLayout.Controls.Add(tabControlMain, 0, 1);
            Controls.Add(rootLayout);
        }
        private void InitializePlaceholderTabs()
        {
            InitializeLevel2Tab();
            InitializeLevel3Tab();
            InitializeLevel4Tab();
        }
        private void InitializeDataTab()
        {
            mainDataLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainDataLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainDataLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            dataTopPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 13,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dataTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            labelDb = new Label
            {
                Text = "База данных:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(0, 9, 10, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            buttonOpenDb = new Button
            {
                Name = "buttonOpenDb",
                Text = "Открыть БД",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            labelTables = new Label
            {
                Text = "Таблица:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(20, 9, 10, 0)
            };
            comboBoxTables = new ComboBox
            {
                Name = "comboBoxTables",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            buttonLoadTable = new Button
            {
                Name = "buttonLoadTable",
                Text = "Загрузить таблицу",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Enabled = false
            };
            buttonAddRow = new Button
            {
                Name = "buttonAddRow",
                Text = "Добавить строку",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Enabled = false
            };
            buttonDeleteRow = new Button
            {
                Name = "buttonDeleteRow",
                Text = "Удалить строку",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Enabled = false
            };
            buttonSaveTable = new Button
            {
                Name = "buttonSaveTable",
                Text = "Сохранить изменения",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Enabled = false
            };
            buttonReplaceScheme = new Button
            {
                Name = "buttonReplaceScheme",
                Text = "Заменить схему",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Enabled = false
            };
            labelEpsilon = new Label
            {
                Text = "E:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(20, 9, 4, 0)
            };
            textBoxEpsilon = new TextBox
            {
                Name = "textBoxEpsilon",
                Width = 90,
                Text = "0.0007",
                Anchor = AnchorStyles.Left
            };
            labelAlpha = new Label
            {
                Text = "A:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(20, 9, 4, 0)
            };
            textBoxAlpha = new TextBox
            {
                Name = "textBoxAlpha",
                Width = 90,
                Text = "0.9",
                Anchor = AnchorStyles.Left
            };
            dataTopPanel.Controls.Add(labelDb, 0, 0);
            dataTopPanel.Controls.Add(buttonOpenDb, 1, 0);
            dataTopPanel.Controls.Add(labelTables, 2, 0);
            dataTopPanel.Controls.Add(comboBoxTables, 3, 0);
            dataTopPanel.Controls.Add(buttonLoadTable, 4, 0);
            dataTopPanel.Controls.Add(buttonAddRow, 5, 0);
            dataTopPanel.Controls.Add(buttonDeleteRow, 6, 0);
            dataTopPanel.Controls.Add(buttonSaveTable, 7, 0);
            dataTopPanel.Controls.Add(labelEpsilon, 8, 0);
            dataTopPanel.Controls.Add(textBoxEpsilon, 9, 0);
            dataTopPanel.Controls.Add(labelAlpha, 10, 0);
            dataTopPanel.Controls.Add(textBoxAlpha, 11, 0);
            dataTopPanel.Controls.Add(buttonReplaceScheme, 12, 0);
            dataContentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 10, 0, 0)
            };
            dataContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            dataContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            dataContentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            dataContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            labelSource = new Label
            {
                Text = "Исходные данные",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8)
            };
            labelImage = new Label
            {
                Text = "Схема / изображение объекта",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(10, 0, 0, 8)
            };
            dataGridViewSource = new DataGridView
            {
                Name = "dataGridViewSource",
                Dock = DockStyle.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            panelSchemeScroll = new Panel
            {
                Name = "panelSchemeScroll",
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.White
            };
            pictureBoxScheme = new PictureBox
            {
                Name = "pictureBoxScheme",
                Location = new Point(0, 0),
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pictureBoxScheme.MouseWheel += PictureBoxScheme_MouseWheel;
            panelSchemeScroll.MouseWheel += PictureBoxScheme_MouseWheel;
            panelSchemeScroll.Controls.Add(pictureBoxScheme);
            dataContentLayout.Controls.Add(labelSource, 0, 0);
            dataContentLayout.Controls.Add(labelImage, 1, 0);
            dataContentLayout.Controls.Add(dataGridViewSource, 0, 1);
            dataContentLayout.Controls.Add(panelSchemeScroll, 1, 1);
            mainDataLayout.Controls.Add(dataTopPanel, 0, 0);
            mainDataLayout.Controls.Add(dataContentLayout, 0, 1);
            tabPageData.Controls.Add(mainDataLayout);
        }
        private void InitializeLevel1Tab()
        {
            level1Layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            level1Layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            level1Layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            level1Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            labelLevel1 = new Label
            {
                Text = "Результаты I уровня декомпозиции",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false
            };
            buttonLevel1 = new Button
            {
                Name = "buttonLevel1",
                Text = "Запустить I уровень декомпозиции",
                AutoSize = true,
                Enabled = false
            };
            buttonShowLevel1Charts = new Button
            {
                Name = "buttonShowLevel1Charts",
                Text = "Показать графики",
                AutoSize = true,
                Enabled = false
            };
            buttonPanel.Controls.Add(buttonLevel1);
            buttonPanel.Controls.Add(buttonShowLevel1Charts);
            dataGridViewLevel1 = new DataGridView
            {
                Name = "dataGridViewLevel1",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false
            };
            level1Layout.Controls.Add(labelLevel1, 0, 0);
            level1Layout.Controls.Add(buttonPanel, 0, 1);
            level1Layout.Controls.Add(dataGridViewLevel1, 0, 2);
            tabPageLevel1.Controls.Add(level1Layout);
        }
        private void InitializeLevel2Tab()
        {
            tabControlLevel2 = new TabControl
            {
                Dock = DockStyle.Fill
            };
            tabPageLevel2Distribution = new TabPage("Распределение точек");
            tabPageLevel2Calculation = new TabPage("Расчёт блока");
            tabControlLevel2.TabPages.Add(tabPageLevel2Distribution);
            tabControlLevel2.TabPages.Add(tabPageLevel2Calculation);
            InitializeLevel2DistributionTab();
            InitializeLevel2CalculationTab();
            tabPageLevel2.Controls.Add(tabControlLevel2);
        }
        private void InitializeLevel3Tab()
        {
            tabControlLevel3 = new TabControl
            {
                Dock = DockStyle.Fill
            };
            tabPageLevel3Distribution = new TabPage("Распределение контрольных точек по подблокам");
            tabPageLevel3Calculation = new TabPage("Расчёты и графики") { Enabled = false };
            tabControlLevel3.TabPages.Add(tabPageLevel3Distribution);
            tabControlLevel3.TabPages.Add(tabPageLevel3Calculation);
            InitializeLevel3DistributionTab();
            InitializeLevel3CalculationTab();
            tabPageLevel3.Controls.Add(tabControlLevel3);
        }

        private void InitializeLevel3DistributionTab()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Text = "Подготовка блока и распределение контрольных точек по подблокам",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 6)
            };

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                AutoScroll = true
            };
            labelLevel3Block = new Label
            {
                Text = "Разбиваемый блок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 0)
            };
            comboBoxLevel3Blocks = new ComboBox
            {
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            labelLevel3Tolerance = new Label
            {
                Text = "Допуск жёсткости:",
                AutoSize = true,
                Margin = new Padding(20, 8, 8, 0)
            };
            numericUpDownLevel3Tolerance = new NumericUpDown
            {
                Width = 95,
                DecimalPlaces = 4,
                Increment = 0.0001M,
                Minimum = 0.0001M,
                Maximum = 10M,
                Value = 0.0007M
            };
            buttonLevel3 = new Button
            {
                Text = "Подтвердить блок и рассчитать таблицы",
                AutoSize = true,
                Enabled = false
            };
            buttonShowLevel3Charts = new Button
            {
                Text = "Показать график связей",
                AutoSize = true,
                Enabled = false
            };
            topPanel.Controls.Add(labelLevel3Block);
            topPanel.Controls.Add(comboBoxLevel3Blocks);
            topPanel.Controls.Add(labelLevel3Tolerance);
            topPanel.Controls.Add(numericUpDownLevel3Tolerance);
            topPanel.Controls.Add(buttonLevel3);
            topPanel.Controls.Add(buttonShowLevel3Charts);

            labelLevel3Summary = new Label
            {
                Text = "Выберите блок и подготовьте набор контрольных точек.",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(0, 5, 0, 7)
            };

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));

            var blockGroup = new GroupBox
            {
                Text = "1. Подготовка контрольных точек разбиваемого блока",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            var blockLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            blockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            blockLayout.Controls.Add(new Label
            {
                Text = "Нераспределённые точки",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            }, 0, 0);
            blockLayout.Controls.Add(new Label
            {
                Text = "Точки блока",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            }, 2, 0);
            listBoxLevel3AvailableBlockPoints = new ListBox { Dock = DockStyle.Fill };
            listBoxLevel3BlockPoints = new ListBox { Dock = DockStyle.Fill };
            var blockButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            blockButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            buttonLevel3AddPointToBlock = new Button
            {
                Text = ">",
                Width = 42,
                Height = 32,
                Anchor = AnchorStyles.None
            };
            buttonLevel3RemovePointFromBlock = new Button
            {
                Text = "<",
                Width = 42,
                Height = 32,
                Anchor = AnchorStyles.None
            };
            blockButtons.Controls.Add(buttonLevel3AddPointToBlock, 0, 1);
            blockButtons.Controls.Add(buttonLevel3RemovePointFromBlock, 0, 2);
            blockLayout.Controls.Add(listBoxLevel3AvailableBlockPoints, 0, 1);
            blockLayout.Controls.Add(blockButtons, 1, 1);
            blockLayout.Controls.Add(listBoxLevel3BlockPoints, 2, 1);
            blockGroup.Controls.Add(blockLayout);

            var subblockGroup = new GroupBox
            {
                Text = "2. Распределение контрольных точек по подблокам",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            var subblockLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var subblockSettings = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };
            subblockSettings.Controls.Add(new Label
            {
                Text = "Количество подблоков:",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            });
            numericUpDownLevel3SubblocksCount = new NumericUpDown
            {
                Width = 65,
                Minimum = 1,
                Maximum = 20,
                Value = 2,
                Enabled = false
            };
            subblockSettings.Controls.Add(numericUpDownLevel3SubblocksCount);
            subblockSettings.Controls.Add(new Label
            {
                Text = "Точек на подблок:",
                AutoSize = true,
                Margin = new Padding(12, 8, 6, 0)
            });
            numericUpDownLevel3PointsPerSubblock = new NumericUpDown
            {
                Width = 65,
                Minimum = 2,
                Maximum = 100,
                Value = 2,
                Enabled = false
            };
            subblockSettings.Controls.Add(numericUpDownLevel3PointsPerSubblock);
            buttonLevel3CreateSubblocks = new Button
            {
                Text = "Создать подблоки",
                AutoSize = true,
                Enabled = false
            };
            subblockSettings.Controls.Add(buttonLevel3CreateSubblocks);

            var selectPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };
            selectPanel.Controls.Add(new Label
            {
                Text = "Выберите подблок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            });
            comboBoxLevel3Subblocks = new ComboBox
            {
                Width = 135,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            selectPanel.Controls.Add(comboBoxLevel3Subblocks);
            labelLevel3SubblockRequired = new Label
            {
                Text = "Требуется точек: -",
                AutoSize = true,
                Margin = new Padding(12, 8, 6, 0)
            };
            selectPanel.Controls.Add(labelLevel3SubblockRequired);
            buttonLevel3AutoDistribute = new Button
            {
                Text = "Автораспределение",
                AutoSize = true,
                Enabled = false
            };
            selectPanel.Controls.Add(buttonLevel3AutoDistribute);

            var assignLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            assignLayout.Controls.Add(new Label
            {
                Text = "Свободные точки",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            }, 0, 0);
            assignLayout.Controls.Add(new Label
            {
                Text = "Точки выбранного подблока",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            }, 2, 0);
            listBoxLevel3FreeSubblockPoints = new ListBox
            {
                Dock = DockStyle.Fill,
                Enabled = false
            };
            listBoxLevel3SelectedSubblockPoints = new ListBox
            {
                Dock = DockStyle.Fill,
                Enabled = false
            };
            var moveButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            moveButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            buttonLevel3MoveToSubblock = new Button
            {
                Text = ">",
                Width = 42,
                Height = 32,
                Anchor = AnchorStyles.None,
                Enabled = false
            };
            buttonLevel3RemoveFromSubblock = new Button
            {
                Text = "<",
                Width = 42,
                Height = 32,
                Anchor = AnchorStyles.None,
                Enabled = false
            };
            moveButtons.Controls.Add(buttonLevel3MoveToSubblock, 0, 1);
            moveButtons.Controls.Add(buttonLevel3RemoveFromSubblock, 0, 2);
            assignLayout.Controls.Add(listBoxLevel3FreeSubblockPoints, 0, 1);
            assignLayout.Controls.Add(moveButtons, 1, 1);
            assignLayout.Controls.Add(listBoxLevel3SelectedSubblockPoints, 2, 1);

            buttonLevel3ConfirmSubblocks = new Button
            {
                Text = "Подтвердить распределение по подблокам",
                Dock = DockStyle.Right,
                AutoSize = true,
                Enabled = false
            };
            subblockLayout.Controls.Add(subblockSettings, 0, 0);
            subblockLayout.Controls.Add(selectPanel, 0, 1);
            subblockLayout.Controls.Add(assignLayout, 0, 2);
            subblockLayout.Controls.Add(buttonLevel3ConfirmSubblocks, 0, 3);
            subblockGroup.Controls.Add(subblockLayout);

            var tablesGroup = new GroupBox
            {
                Text = "3. Таблицы разностей координат и контроля жёсткости",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            var tablesSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 280
            };
            var differencePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            differencePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            differencePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            differencePanel.Controls.Add(new Label
            {
                Text = "Таблица разности координат Z контрольных точек",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4)
            }, 0, 0);
            dataGridViewLevel3Differences = CreateLevel3ReadOnlyGrid();
            differencePanel.Controls.Add(dataGridViewLevel3Differences, 0, 1);

            var rigidityPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rigidityPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rigidityPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rigidityPanel.Controls.Add(new Label
            {
                Text = "Жёсткость связей между контрольными точками",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 4, 0, 4)
            }, 0, 0);
            dataGridViewLevel3Links = CreateLevel3ReadOnlyGrid();
            rigidityPanel.Controls.Add(dataGridViewLevel3Links, 0, 1);

            tablesSplit.Panel1.Controls.Add(differencePanel);
            tablesSplit.Panel2.Controls.Add(rigidityPanel);
            tablesGroup.Controls.Add(tablesSplit);

            content.Controls.Add(blockGroup, 0, 0);
            content.Controls.Add(subblockGroup, 1, 0);
            content.Controls.Add(tablesGroup, 2, 0);

            root.Controls.Add(title, 0, 0);
            root.Controls.Add(topPanel, 0, 1);
            root.Controls.Add(labelLevel3Summary, 0, 2);
            root.Controls.Add(content, 0, 3);

            tabPageLevel3Distribution.Controls.Add(root);
        }

        private DataGridView CreateLevel3ReadOnlyGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            };
        }

        private void InitializeLevel3CalculationTab()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Text = "Результаты III уровня декомпозиции",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            var controls = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };
            controls.Controls.Add(new Label
            {
                Text = "Выберите подблок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 0)
            });
            comboBoxLevel3Clusters = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            buttonLevel3Response = new Button
            {
                Text = "Рассчитать III уровень",
                AutoSize = true,
                Enabled = false
            };
            buttonShowLevel3ResponseCharts = new Button
            {
                Text = "Показать графики",
                AutoSize = true,
                Enabled = false
            };
            controls.Controls.Add(comboBoxLevel3Clusters);
            controls.Controls.Add(buttonLevel3Response);
            controls.Controls.Add(buttonShowLevel3ResponseCharts);

            dataGridViewLevel3Response = CreateLevel3ReadOnlyGrid();
            dataGridViewLevel3Response.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(controls, 0, 1);
            layout.Controls.Add(dataGridViewLevel3Response, 0, 2);
            tabPageLevel3Calculation.Controls.Add(layout);
        }
        private void InitializeLevel4Tab()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            var title = new Label
            {
                Text = "IV уровень декомпозиции: анализ контрольных точек Zk,i(t)",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };
            radioButtonLevel4AllPoints = new RadioButton
            {
                Text = "Все точки",
                Checked = true,
                AutoSize = true,
                Margin = new Padding(0, 8, 12, 0)
            };
            radioButtonLevel4StandalonePoints = new RadioButton
            {
                Text = "Только точки вне кластеров III уровня",
                AutoSize = true,
                Margin = new Padding(0, 8, 20, 0)
            };
            buttonLevel4SelectAll = new Button
            {
                Text = "Выбрать все точки",
                AutoSize = true,
                Enabled = false
            };
            buttonLevel4ClearSelection = new Button
            {
                Text = "Снять выбор",
                AutoSize = true,
                Enabled = false
            };
            buttonLevel4 = new Button
            {
                Text = "Подтвердить и рассчитать IV уровень",
                AutoSize = true,
                Enabled = false
            };
            buttonShowLevel4Charts = new Button
            {
                Text = "Показать графики Z(t)",
                AutoSize = true,
                Enabled = false
            };
            labelLevel4Summary = new Label
            {
                Text = "Выберите контрольные точки для построения Zk,i(t)",
                AutoSize = true,
                Margin = new Padding(20, 8, 8, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            topPanel.Controls.Add(radioButtonLevel4AllPoints);
            topPanel.Controls.Add(radioButtonLevel4StandalonePoints);
            topPanel.Controls.Add(buttonLevel4SelectAll);
            topPanel.Controls.Add(buttonLevel4ClearSelection);
            topPanel.Controls.Add(buttonLevel4);
            topPanel.Controls.Add(buttonShowLevel4Charts);
            topPanel.Controls.Add(labelLevel4Summary);
            checkedListBoxLevel4Points = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            chartLevel4Preview = new Chart { Dock = DockStyle.Fill };
            var previewArea = new ChartArea("MainArea");
            previewArea.AxisX.Title = "Цикл наблюдения, t";
            previewArea.AxisY.Title = "Координата Z";
            previewArea.AxisX.Interval = 1;
            previewArea.AxisX.MajorGrid.Enabled = true;
            previewArea.AxisY.MajorGrid.Enabled = true;
            chartLevel4Preview.ChartAreas.Add(previewArea);
            chartLevel4Preview.Legends.Add(new Legend("MainLegend"));
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 260
            };
            var pointsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 8, 0)
            };
            pointsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pointsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pointsPanel.Controls.Add(new Label
            {
                Text = "Контрольные точки",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 6)
            }, 0, 0);
            pointsPanel.Controls.Add(checkedListBoxLevel4Points, 0, 1);
            split.Panel1.Controls.Add(pointsPanel);
            split.Panel2.Controls.Add(chartLevel4Preview);
            dataGridViewLevel4 = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false
            };
            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(topPanel, 0, 1);
            layout.Controls.Add(split, 0, 2);
            layout.Controls.Add(dataGridViewLevel4, 0, 3);
            tabPageLevel4.Controls.Add(layout);
        }
        private void BindEvents()
        {
            buttonOpenDb.Click += ButtonOpenDb_Click;
            buttonHelp.Click += ButtonHelp_Click;
            buttonLoadTable.Click += ButtonLoadTable_Click;
            buttonAddRow.Click += ButtonAddRow_Click;
            buttonDeleteRow.Click += ButtonDeleteRow_Click;
            buttonSaveTable.Click += ButtonSaveTable_Click;
            buttonReplaceScheme.Click += ButtonReplaceScheme_Click;
            buttonLevel1.Click += ButtonLevel1_Click;
            buttonShowLevel1Charts.Click += ButtonShowLevel1Charts_Click;
            dataGridViewLevel1.CellFormatting += DataGridViewLevel1_CellFormatting;
            dataGridViewSource.DataError += DataGridViewSource_DataError;

            buttonCreateBlocks.Click += ButtonCreateBlocks_Click;
            comboBoxBlocks.SelectedIndexChanged += ComboBoxBlocks_SelectedIndexChanged;
            buttonMoveToBlock.Click += ButtonMoveToBlock_Click;
            buttonRemoveFromBlock.Click += ButtonRemoveFromBlock_Click;
            listBoxAllPoints.DoubleClick += ListBoxAllPoints_DoubleClick;
            listBoxBlockPoints.DoubleClick += ListBoxBlockPoints_DoubleClick;
            buttonAutoDistribute.Click += ButtonAutoDistribute_Click;
            buttonConfirmBlocks.Click += ButtonConfirmBlocks_Click;
            buttonLevel2.Click += ButtonLevel2_Click;
            buttonShowLevel2Charts.Click += ButtonShowLevel2Charts_Click;
            dataGridViewLevel2.CellFormatting += DataGridViewLevel2_CellFormatting;

            comboBoxLevel3Blocks.SelectedIndexChanged += ComboBoxLevel3Blocks_SelectedIndexChanged;
            buttonLevel3AddPointToBlock.Click += ButtonLevel3AddPointToBlock_Click;
            buttonLevel3RemovePointFromBlock.Click += ButtonLevel3RemovePointFromBlock_Click;
            listBoxLevel3AvailableBlockPoints.DoubleClick += ButtonLevel3AddPointToBlock_Click;
            listBoxLevel3BlockPoints.DoubleClick += ButtonLevel3RemovePointFromBlock_Click;
            buttonLevel3.Click += ButtonLevel3_Click;
            buttonShowLevel3Charts.Click += ButtonShowLevel3Charts_Click;
            numericUpDownLevel3SubblocksCount.ValueChanged += Level3SubblockSettings_ValueChanged;
            numericUpDownLevel3PointsPerSubblock.ValueChanged += Level3SubblockSettings_ValueChanged;
            buttonLevel3CreateSubblocks.Click += ButtonLevel3CreateSubblocks_Click;
            comboBoxLevel3Subblocks.SelectedIndexChanged += ComboBoxLevel3Subblocks_SelectedIndexChanged;
            buttonLevel3MoveToSubblock.Click += ButtonLevel3MoveToSubblock_Click;
            buttonLevel3RemoveFromSubblock.Click += ButtonLevel3RemoveFromSubblock_Click;
            listBoxLevel3FreeSubblockPoints.DoubleClick += ButtonLevel3MoveToSubblock_Click;
            listBoxLevel3SelectedSubblockPoints.DoubleClick += ButtonLevel3RemoveFromSubblock_Click;
            buttonLevel3AutoDistribute.Click += ButtonLevel3AutoDistribute_Click;
            buttonLevel3ConfirmSubblocks.Click += ButtonLevel3ConfirmSubblocks_Click;
            dataGridViewLevel3Differences.CellFormatting += DataGridViewLevel3_CellFormatting;
            dataGridViewLevel3Links.CellFormatting += DataGridViewLevel3_CellFormatting;
            buttonLevel3Response.Click += ButtonLevel3Response_Click;
            buttonShowLevel3ResponseCharts.Click += ButtonShowLevel3ResponseCharts_Click;
            dataGridViewLevel3Response.CellFormatting += DataGridViewLevel3_CellFormatting;

            buttonLevel4.Click += ButtonLevel4_Click;
            buttonLevel4SelectAll.Click += ButtonLevel4SelectAll_Click;
            buttonLevel4ClearSelection.Click += ButtonLevel4ClearSelection_Click;
            buttonShowLevel4Charts.Click += ButtonShowLevel4Charts_Click;
            checkedListBoxLevel4Points.ItemCheck += (s, e) => BeginInvoke(new Action(() =>
            {
                _lastLevel4Result = null;
                _lastLevel4SelectedPoints.Clear();
                buttonShowLevel4Charts.Enabled = GetSelectedLevel4Points().Count > 0;
                UpdateLevel4Buttons();
                UpdateLevel4PreviewChart();
            }));
            radioButtonLevel4AllPoints.CheckedChanged += (s, e) => UpdateLevel4Points();
            radioButtonLevel4StandalonePoints.CheckedChanged += (s, e) => UpdateLevel4Points();
            dataGridViewLevel4.CellFormatting += DataGridViewLevel3_CellFormatting;
        }
        #endregion

        #region 3. Ввод данных, загрузка БД, фото и сохранение

        private void ButtonOpenDb_Click(object sender, EventArgs e)
        {
            try
            {
                if (_openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                _databasePath = _openFileDialog.FileName;
                List<string> tables = _engine.GetTableNames(_databasePath);
                comboBoxTables.Items.Clear();
                foreach (string tableName in tables)
                    comboBoxTables.Items.Add(tableName);
                comboBoxTables.Enabled = comboBoxTables.Items.Count > 0;
                buttonLoadTable.Enabled = comboBoxTables.Items.Count > 0;
                buttonAddRow.Enabled = false;
                buttonDeleteRow.Enabled = false;
                buttonSaveTable.Enabled = false;
                buttonReplaceScheme.Enabled = true;
                buttonLevel1.Enabled = false;
                LoadObjectMetadataFromDatabase();
                LoadObjectImage();
                dataGridViewSource.DataSource = null;
                dataGridViewLevel1.DataSource = null;
                pictureBoxScheme.Image = null;
                if (comboBoxTables.Items.Count > 0)
                    comboBoxTables.SelectedIndex = 0;
                MessageBox.Show("База данных успешно подключена.", "Подключение БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения базы данных: {ex.Message}");
            }
        }
        private void ButtonLoadTable_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databasePath))
                {
                    ShowError("Сначала необходимо подключить базу данных.");
                    return;
                }
                if (comboBoxTables.SelectedItem == null)
                {
                    ShowError("Выберите таблицу.");
                    return;
                }
                _currentTableName = comboBoxTables.SelectedItem.ToString();
                _sourceTable = _engine.LoadTable(_databasePath, _currentTableName);
                if (_sourceTable == null)
                {
                    ShowError("Не удалось загрузить таблицу.");
                    return;
                }
                dataGridViewSource.DataSource = _sourceTable;
                buttonAddRow.Enabled = true;
                buttonDeleteRow.Enabled = true;
                buttonSaveTable.Enabled = true;
                RebuildCyclesFromCurrentTable();
                LoadObjectMetadataFromDatabase();
                LoadObjectImage();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки таблицы: {ex.Message}");
            }
        }
        private void ButtonAddRow_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sourceTable == null)
                {
                    ShowError("Сначала загрузите таблицу.");
                    return;
                }
                _engine.AddEmptyRow(_sourceTable);
                dataGridViewSource.Refresh();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления строки: {ex.Message}");
            }
        }
        private void ButtonDeleteRow_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sourceTable == null)
                {
                    ShowError("Сначала загрузите таблицу.");
                    return;
                }
                if (dataGridViewSource.SelectedRows.Count == 0)
                {
                    ShowError("Выберите строку для удаления.");
                    return;
                }
                int rowIndex = dataGridViewSource.SelectedRows[0].Index;
                _engine.DeleteRow(_sourceTable, rowIndex);
                dataGridViewSource.Refresh();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления строки: {ex.Message}");
            }
        }
        private void ButtonSaveTable_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sourceTable == null)
                {
                    ShowError("Нет загруженной таблицы.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(_databasePath) || string.IsNullOrWhiteSpace(_currentTableName))
                {
                    ShowError("Не указаны база данных или таблица.");
                    return;
                }
                dataGridViewSource.EndEdit();
                _engine.SaveTable(_databasePath, _currentTableName, _sourceTable);
                if (TryReadParameters(out double saveEpsilon, out double saveAlpha))
                    _engine.SaveObjectMetadata(_databasePath, numericUpDownBlocksCount == null ? 0 : (int)numericUpDownBlocksCount.Value, saveEpsilon, saveAlpha);
                RebuildCyclesFromCurrentTable();
                LoadObjectImage();
                MessageBox.Show("Изменения сохранены.", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения таблицы: {ex.Message}");
            }
        }
        private void ButtonReplaceScheme_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databasePath))
                {
                    ShowError("Сначала откройте базу данных.");
                    return;
                }
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*";
                    dialog.Title = "Выберите новую схему объекта";
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    _engine.SaveImageToDatabase(_databasePath, dialog.FileName);
                    LoadObjectImage();
                    MessageBox.Show("Схема объекта заменена и сохранена в базе данных.", "Схема",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка замены схемы: {ex.Message}");
            }
        }
        private void LoadObjectMetadataFromDatabase()
        {
            try
            {
                if (_engine.TryLoadObjectMetadata(_databasePath, out int blocksCount, out double epsilon, out double alpha))
                {
                    if (epsilon > 0)
                        textBoxEpsilon.Text = epsilon.ToString("0.######", CultureInfo.InvariantCulture);
                    if (alpha > 0)
                        textBoxAlpha.Text = alpha.ToString("0.######", CultureInfo.InvariantCulture);
                    if (numericUpDownBlocksCount != null && blocksCount >= numericUpDownBlocksCount.Minimum && blocksCount <= numericUpDownBlocksCount.Maximum)
                        numericUpDownBlocksCount.Value = blocksCount;
                }
            }
            catch
            {
            }
        }
        private void LoadObjectImage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databasePath))
                {
                    pictureBoxScheme.Image = null;
                    if (pictureBoxLevel2Scheme != null)
                        pictureBoxLevel2Scheme.Image = null;
                    return;
                }
                Image img = _engine.LoadImageFromDatabase(_databasePath);
                if (pictureBoxScheme.Image != null)
                    pictureBoxScheme.Image.Dispose();
                pictureBoxScheme.Image = img;
                _schemeZoom = 1.0;
                ApplySchemeZoom();
                if (pictureBoxLevel2Scheme != null)
                    pictureBoxLevel2Scheme.Image = img;
            }
            catch (Exception ex)
            {
                pictureBoxScheme.Image = null;
                if (pictureBoxLevel2Scheme != null)
                    pictureBoxLevel2Scheme.Image = null;
                ShowError($"Ошибка загрузки схемы объекта: {ex.Message}");
            }
        }
        private void PictureBoxScheme_MouseWheel(object sender, MouseEventArgs e)
        {
            if (pictureBoxScheme.Image == null)
                return;
            _schemeZoom += e.Delta > 0 ? 0.10 : -0.10;
            if (_schemeZoom < 0.20) _schemeZoom = 0.20;
            if (_schemeZoom > 5.00) _schemeZoom = 5.00;
            ApplySchemeZoom();
        }
        private void ApplySchemeZoom()
        {
            if (pictureBoxScheme == null)
                return;
            if (pictureBoxScheme.Image == null)
            {
                pictureBoxScheme.Size = Size.Empty;
                return;
            }
            int width = Math.Max(1, (int)(pictureBoxScheme.Image.Width * _schemeZoom));
            int height = Math.Max(1, (int)(pictureBoxScheme.Image.Height * _schemeZoom));
            pictureBoxScheme.Size = new Size(width, height);
        }
        private void RebuildCyclesFromCurrentTable()
        {
            _cycles = _engine.ConvertTableToMeasurementCycles(_sourceTable);
            buttonLevel1.Enabled = _cycles.Count > 0;
            UpdateLevel4Points();
            UpdateLevel4PreviewChart();
        }
        private bool TryReadParameters(out double epsilon, out double alpha)
        {
            epsilon = 0.0;
            alpha = 0.0;
            if (!double.TryParse(textBoxEpsilon.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out epsilon))
            {
                ShowError("Некорректное значение E.");
                return false;
            }
            if (!double.TryParse(textBoxAlpha.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out alpha))
            {
                ShowError("Некорректное значение A.");
                return false;
            }
            if (epsilon <= 0)
            {
                ShowError("Параметр E должен быть больше 0.");
                return false;
            }
            if (alpha <= 0 || alpha >= 1)
            {
                ShowError("Параметр A должен принадлежать интервалу (0;1).");
                return false;
            }
            return true;
        }
        private void DataGridViewSource_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            ShowError("Некорректное значение в ячейке таблицы.");
            e.Cancel = false;
        }
        #endregion

        #region 4. I уровень декомпозиции

        private void ButtonLevel1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cycles == null || _cycles.Count == 0)
                {
                    ShowError("Нет данных для первого уровня декомпозиции.");
                    return;
                }
                if (!TryReadParameters(out double epsilon, out double alpha))
                    return;
                Level1AnalysisResult result = _engine.RunLevel1(_cycles, epsilon, alpha);
                _lastLevel1Result = result;
                dataGridViewLevel1.DataSource = BuildLevel1ResultTable(result);
                buttonShowLevel1Charts.Enabled = true;
                tabControlMain.SelectedTab = tabPageLevel1;
                MessageBox.Show("Расчёт первого уровня декомпозиции завершён.", "I уровень",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка выполнения первого уровня декомпозиции: {ex.Message}");
            }
        }
        private void ButtonShowLevel1Charts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lastLevel1Result == null || _lastLevel1Result.Items.Count == 0)
                {
                    ShowError("Сначала выполните расчёт I уровня декомпозиции.");
                    return;
                }
                using (Level1ChartsForm chartsForm = new Level1ChartsForm(_lastLevel1Result))
                {
                    chartsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка открытия окна графиков: {ex.Message}");
            }
        }
        private DataTable BuildLevel1ResultTable(Level1AnalysisResult result)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Цикл", typeof(string));
            table.Columns.Add("M", typeof(double));
            table.Columns.Add("M+E", typeof(double));
            table.Columns.Add("M-E", typeof(double));
            table.Columns.Add("A", typeof(double));
            table.Columns.Add("A+E", typeof(double));
            table.Columns.Add("A-E", typeof(double));
            table.Columns.Add("R", typeof(double));
            table.Columns.Add("L", typeof(double));
            table.Columns.Add("Статус", typeof(string));
            foreach (PhaseStateItem item in result.Items)
            {
                DataRow row = table.NewRow();
                row["Цикл"] = item.Label;
                row["M"] = item.M;
                row["M+E"] = item.MPlusE;
                row["M-E"] = item.MMinusE;
                row["A"] = item.A;
                row["A+E"] = item.APlusE;
                row["A-E"] = item.AMinusE;
                row["R"] = item.R;
                row["L"] = item.L;
                row["Статус"] = item.StatusText;
                table.Rows.Add(row);
            }
            DataRow forecastRow = table.NewRow();
            forecastRow["Цикл"] = "Прогноз";
            forecastRow["M"] = result.ForecastM;
            forecastRow["M+E"] = result.ForecastMPlusE;
            forecastRow["M-E"] = result.ForecastMMinusE;
            forecastRow["A"] = result.ForecastA;
            forecastRow["A+E"] = result.ForecastAPlusE;
            forecastRow["A-E"] = result.ForecastAMinusE;
            forecastRow["R"] = result.ForecastR;
            forecastRow["L"] = result.ForecastL;
            forecastRow["Статус"] = result.ForecastStatusText;
            table.Rows.Add(forecastRow);
            return table;
        }
        private void DataGridViewLevel1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is DataGridView grid && grid.Columns[e.ColumnIndex].Name == "Статус")
            {
                string status = Convert.ToString(e.Value)?.ToLower();
                if (status.Contains("без изменений"))
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (status.Contains("авар"))
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = Color.Black;
                }
                return;
            }
            FormatNumericCell(e);
        }
        #endregion

        #region 5. II уровень декомпозиции

        private void InitializeLevel2DistributionTab()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false
            };
            labelBlocksCount = new Label
            {
                Text = "Количество блоков:",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 0)
            };
            numericUpDownBlocksCount = new NumericUpDown
            {
                Width = 70,
                Minimum = 1,
                Maximum = 10,
                Value = 2
            };
            buttonCreateBlocks = new Button
            {
                Text = "Создать блоки",
                AutoSize = true
            };
            labelChooseBlock = new Label
            {
                Text = "Выберите блок:",
                AutoSize = true,
                Margin = new Padding(20, 8, 8, 0)
            };
            comboBoxBlocks = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            labelRequiredCount = new Label
            {
                Text = "Точек в блоке: -",
                AutoSize = true,
                Margin = new Padding(15, 8, 8, 0)
            };
            buttonAutoDistribute = new Button
            {
                Text = "Автораспределение",
                AutoSize = true
            };
            buttonConfirmBlocks = new Button
            {
                Text = "Подтвердить распределение",
                AutoSize = true
            };
            topPanel.Controls.Add(labelBlocksCount);
            topPanel.Controls.Add(numericUpDownBlocksCount);
            topPanel.Controls.Add(buttonCreateBlocks);
            topPanel.Controls.Add(labelChooseBlock);
            topPanel.Controls.Add(comboBoxBlocks);
            topPanel.Controls.Add(labelRequiredCount);
            topPanel.Controls.Add(buttonAutoDistribute);
            topPanel.Controls.Add(buttonConfirmBlocks);
            var assignLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            labelAllPoints = new Label
            {
                Text = "Свободные точки",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            labelBlockPoints = new Label
            {
                Text = "Точки выбранного блока",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            listBoxAllPoints = new ListBox { Dock = DockStyle.Fill };
            listBoxBlockPoints = new ListBox { Dock = DockStyle.Fill };
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            buttonMoveToBlock = new Button
            {
                Text = ">",
                Width = 50,
                Height = 35,
                Anchor = AnchorStyles.None
            };
            buttonRemoveFromBlock = new Button
            {
                Text = "<",
                Width = 50,
                Height = 35,
                Anchor = AnchorStyles.None
            };
            buttonPanel.Controls.Add(buttonMoveToBlock, 0, 1);
            buttonPanel.Controls.Add(buttonRemoveFromBlock, 0, 2);
            assignLayout.Controls.Add(labelAllPoints, 0, 0);
            assignLayout.Controls.Add(labelBlockPoints, 2, 0);
            assignLayout.Controls.Add(listBoxAllPoints, 0, 1);
            assignLayout.Controls.Add(buttonPanel, 1, 1);
            assignLayout.Controls.Add(listBoxBlockPoints, 2, 1);
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 850
            };
            split.Panel1.Controls.Add(assignLayout);
            pictureBoxLevel2Scheme = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = pictureBoxScheme?.Image
            };
            split.Panel2.Controls.Add(pictureBoxLevel2Scheme);
            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(split, 0, 1);
            tabPageLevel2Distribution.Controls.Add(layout);
        }
        private void InitializeLevel2CalculationTab()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            var title = new Label
            {
                Text = "Результаты II уровня декомпозиции",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false
            };
            buttonLevel2 = new Button
            {
                Text = "Запустить II уровень декомпозиции",
                AutoSize = true,
                Enabled = false
            };
            buttonShowLevel2Charts = new Button
            {
                Text = "Показать графики",
                AutoSize = true,
                Enabled = false
            };
            buttonPanel.Controls.Add(buttonLevel2);
            buttonPanel.Controls.Add(buttonShowLevel2Charts);
            dataGridViewLevel2 = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false
            };
            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(buttonPanel, 0, 1);
            layout.Controls.Add(dataGridViewLevel2, 0, 2);
            tabPageLevel2Calculation.Controls.Add(layout);
        }
        private void ButtonCreateBlocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sourceTable == null)
                {
                    ShowError("Сначала загрузите таблицу данных.");
                    return;
                }
                int blocksCount = (int)numericUpDownBlocksCount.Value;
                CreateBlocks(blocksCount);
                MessageBox.Show("Блоки успешно созданы.", "II уровень",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка создания блоков: {ex.Message}");
            }
        }
        private void CreateBlocks(int count)
        {
            if (count < 1)
                throw new InvalidOperationException("Количество блоков должно быть больше 0.");
            comboBoxBlocks.Items.Clear();
            _blockData.Clear();
            _blocksConfirmed = false;
            buttonLevel2.Enabled = false;
            buttonShowLevel2Charts.Enabled = false;
            for (int i = 0; i < count; i++)
            {
                string blockName = ((char)('А' + i)).ToString();
                comboBoxBlocks.Items.Add(blockName);
                _blockData[blockName] = new List<string>();
            }
            if (comboBoxBlocks.Items.Count > 0)
                comboBoxBlocks.SelectedIndex = 0;
            UpdateRequiredCountLabel();
            UpdateAllPointsList();
        }
        private void ComboBoxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBlockPointsList();
            UpdateAllPointsList();
        }
        private void ButtonConfirmBlocks_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateBlocksDistribution();
                _blocksConfirmed = true;
                UpdateLevel3Blocks();
                UpdateLevel4Points();
                buttonLevel2.Enabled = true;
                MessageBox.Show("Распределение точек по блокам подтверждено.", "II уровень",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _blocksConfirmed = false;
                buttonLevel2.Enabled = false;
            }
        }
        private void ButtonMoveToBlock_Click(object sender, EventArgs e)
        {
            if (listBoxAllPoints.SelectedItem == null || comboBoxBlocks.SelectedItem == null)
                return;
            string block = comboBoxBlocks.SelectedItem.ToString();
            string point = listBoxAllPoints.SelectedItem.ToString();
            if (!_blockData.ContainsKey(block))
                return;
            int requiredCount = GetRequiredPointsPerBlock();
            if (requiredCount <= 0)
                return;
            if (_blockData[block].Count >= requiredCount)
            {
                ShowError($"В блоке {block} уже достигнуто допустимое количество точек: {requiredCount}.");
                return;
            }
            _blockData[block].Add(point);
            UpdateAllPointsList();
        }
        private void ButtonRemoveFromBlock_Click(object sender, EventArgs e)
        {
            if (listBoxBlockPoints.SelectedItem == null || comboBoxBlocks.SelectedItem == null)
                return;
            string block = comboBoxBlocks.SelectedItem.ToString();
            string point = listBoxBlockPoints.SelectedItem.ToString();
            if (!_blockData.ContainsKey(block))
                return;
            _blockData[block].Remove(point);
            UpdateAllPointsList();
        }
        private void ListBoxAllPoints_DoubleClick(object sender, EventArgs e)
        {
            ButtonMoveToBlock_Click(sender, e);
        }
        private void ListBoxBlockPoints_DoubleClick(object sender, EventArgs e)
        {
            ButtonRemoveFromBlock_Click(sender, e);
        }
        private void ButtonAutoDistribute_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sourceTable == null)
                {
                    ShowError("Сначала загрузите таблицу данных.");
                    return;
                }
                int blocksCount = _blockData.Count;
                if (blocksCount == 0)
                {
                    ShowError("Сначала создайте блоки.");
                    return;
                }
                var allPoints = _sourceTable.Columns
                    .Cast<DataColumn>()
                    .Skip(1)
                    .Select(c => c.ColumnName)
                    .ToList();
                int perBlock = GetRequiredPointsPerBlock();
                if (perBlock < 2)
                {
                    ShowError("Недостаточно точек для распределения по блокам.");
                    return;
                }
                foreach (var key in _blockData.Keys.ToList())
                    _blockData[key].Clear();
                int index = 0;
                foreach (string block in _blockData.Keys.OrderBy(x => x))
                {
                    for (int i = 0; i < perBlock && index < allPoints.Count; i++)
                    {
                        _blockData[block].Add(allPoints[index]);
                        index++;
                    }
                }
                UpdateAllPointsList();
                UpdateRequiredCountLabel();
                int remainder = allPoints.Count - perBlock * blocksCount;
                MessageBox.Show(
                    remainder > 0
                        ? $"Точки автоматически распределены по блокам.\nВ каждом блоке: {perBlock} точек.\nНераспределённых точек осталось: {remainder}."
                        : $"Точки автоматически распределены по блокам.\nВ каждом блоке: {perBlock} точек.",
                    "II уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка автораспределения: {ex.Message}");
            }
        }
        private void UpdateAllPointsList()
        {
            listBoxAllPoints.Items.Clear();
            if (_sourceTable == null)
                return;
            var allColumns = _sourceTable.Columns
                .Cast<DataColumn>()
                .Skip(1)
                .Select(c => c.ColumnName)
                .ToList();
            var assigned = _blockData.Values.SelectMany(x => x).ToHashSet();
            foreach (var col in allColumns)
            {
                if (!assigned.Contains(col))
                    listBoxAllPoints.Items.Add(col);
            }
            UpdateBlockPointsList();
        }
        private void UpdateBlockPointsList()
        {
            listBoxBlockPoints.Items.Clear();
            if (comboBoxBlocks.SelectedItem == null)
                return;
            string blockName = comboBoxBlocks.SelectedItem.ToString();
            if (!_blockData.ContainsKey(blockName))
                return;
            foreach (string p in _blockData[blockName])
                listBoxBlockPoints.Items.Add(p);
        }
        private void ValidateBlocksDistribution()
        {
            if (_blockData.Count == 0)
                throw new InvalidOperationException("Блоки не созданы.");
            int required = GetRequiredPointsPerBlock();
            if (required < 2)
                throw new InvalidOperationException("Недостаточно точек для корректного распределения по блокам.");
            foreach (var block in _blockData)
            {
                if (block.Value.Count != required)
                    throw new InvalidOperationException(
                        $"Блок {block.Key} содержит {block.Value.Count} точек. Требуется строго {required}.");
            }
        }
        private int GetRequiredPointsPerBlock()
        {
            if (_sourceTable == null || _blockData.Count == 0)
                return 0;
            int totalPoints = _sourceTable.Columns.Count - 1;
            int blocksCount = _blockData.Count;
            if (blocksCount <= 0)
                return 0;
            int pointsPerBlock = (int)Math.Floor((double)totalPoints / blocksCount);
            if (pointsPerBlock < 2)
                return -1;
            return pointsPerBlock;
        }
        private void UpdateRequiredCountLabel()
        {
            int required = GetRequiredPointsPerBlock();
            if (required == -1)
                labelRequiredCount.Text = "Точек в блоке: недостаточно данных";
            else if (required == 0)
                labelRequiredCount.Text = "Точек в блоке: -";
            else
                labelRequiredCount.Text = $"Точек в блоке: {required}";
        }
        private void ButtonLevel2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_blocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по блокам.");
                    return;
                }
                if (comboBoxBlocks.SelectedItem == null)
                {
                    ShowError("Выберите блок.");
                    return;
                }
                if (!TryReadParameters(out double epsilon, out double alpha))
                    return;
                string blockName = comboBoxBlocks.SelectedItem.ToString();
                tabPageLevel2Calculation.Text = $"Расчёт блока: {blockName}";
                List<string> selectedColumns = _blockData[blockName];
                Level2AnalysisResult result = _engine.RunLevel2(_cycles, selectedColumns, epsilon, alpha, blockName);
                _lastLevel2Result = result;
                dataGridViewLevel2.DataSource = BuildLevel2ResultTable(result);
                buttonShowLevel2Charts.Enabled = true;
                tabControlLevel2.SelectedTab = tabPageLevel2Calculation;
                MessageBox.Show($"Расчёт II уровня завершён для блока {blockName}.", "II уровень",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка расчёта II уровня: {ex.Message}");
            }
        }
        private void ButtonShowLevel2Charts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lastLevel2Result == null || _lastLevel2Result.Items.Count == 0)
                {
                    ShowError("Сначала выполните расчёт II уровня декомпозиции.");
                    return;
                }
                using (Level2ChartsForm chartsForm = new Level2ChartsForm(_lastLevel2Result))
                {
                    chartsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка открытия графиков II уровня: {ex.Message}");
            }
        }
        private DataTable BuildLevel2ResultTable(Level2AnalysisResult result)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Цикл", typeof(string));
            table.Columns.Add("M", typeof(double));
            table.Columns.Add("M+E", typeof(double));
            table.Columns.Add("M-E", typeof(double));
            table.Columns.Add("A", typeof(double));
            table.Columns.Add("A+E", typeof(double));
            table.Columns.Add("A-E", typeof(double));
            table.Columns.Add("R", typeof(double));
            table.Columns.Add("L", typeof(double));
            table.Columns.Add("Статус", typeof(string));
            foreach (PhaseStateItem item in result.Items)
            {
                DataRow row = table.NewRow();
                row["Цикл"] = item.Label;
                row["M"] = item.M;
                row["M+E"] = item.MPlusE;
                row["M-E"] = item.MMinusE;
                row["A"] = item.A;
                row["A+E"] = item.APlusE;
                row["A-E"] = item.AMinusE;
                row["R"] = item.R;
                row["L"] = item.L;
                row["Статус"] = item.StatusText;
                table.Rows.Add(row);
            }
            DataRow forecastRow = table.NewRow();
            forecastRow["Цикл"] = "Прогноз";
            forecastRow["M"] = result.ForecastM;
            forecastRow["M+E"] = result.ForecastMPlusE;
            forecastRow["M-E"] = result.ForecastMMinusE;
            forecastRow["A"] = result.ForecastA;
            forecastRow["A+E"] = result.ForecastAPlusE;
            forecastRow["A-E"] = result.ForecastAMinusE;
            forecastRow["R"] = result.ForecastR;
            forecastRow["L"] = result.ForecastL;
            forecastRow["Статус"] = result.ForecastStatusText;
            table.Rows.Add(forecastRow);
            return table;
        }
        private void DataGridViewLevel2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is DataGridView grid && grid.Columns[e.ColumnIndex].Name == "Статус")
            {
                string status = Convert.ToString(e.Value)?.ToLower();
                if (status.Contains("без изменений"))
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (status.Contains("авар"))
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = Color.Black;
                }
                return;
            }
            if (e.Value == null || e.Value == DBNull.Value)
                return;
            if (e.Value is double || e.Value is float || e.Value is decimal)
            {
                double value = Convert.ToDouble(e.Value);
                e.Value = value.ToString("0.######", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
                return;
            }
            string text = Convert.ToString(e.Value);
            if (string.IsNullOrWhiteSpace(text))
                return;
            text = text.Replace(',', '.');
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                e.Value = parsed.ToString("0.######", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
            }
        }
        #endregion

        #region 6. III уровень декомпозиции

        private void UpdateLevel3Blocks()
        {
            if (comboBoxLevel3Blocks == null)
                return;

            comboBoxLevel3Blocks.Items.Clear();
            foreach (string blockName in _blockData.Keys.OrderBy(x => x))
                comboBoxLevel3Blocks.Items.Add(blockName);

            ResetLevel3State();
            buttonLevel3.Enabled = comboBoxLevel3Blocks.Items.Count > 0;

            if (comboBoxLevel3Blocks.Items.Count > 0)
                comboBoxLevel3Blocks.SelectedIndex = 0;
        }

        private void ResetLevel3State()
        {
            _level3PreparedBlockName = string.Empty;
            _level3PreparedBlockPoints.Clear();
            _level3SubblockData.Clear();
            _level3BlockPrepared = false;
            _level3SubblocksConfirmed = false;
            _lastLevel3Result = null;
            _lastLevel3ResponseResult = null;

            if (dataGridViewLevel3Differences != null)
                dataGridViewLevel3Differences.DataSource = null;
            if (dataGridViewLevel3Links != null)
                dataGridViewLevel3Links.DataSource = null;
            if (dataGridViewLevel3Response != null)
                dataGridViewLevel3Response.DataSource = null;

            if (comboBoxLevel3Subblocks != null)
            {
                comboBoxLevel3Subblocks.Items.Clear();
                comboBoxLevel3Subblocks.Enabled = false;
            }
            if (comboBoxLevel3Clusters != null)
            {
                comboBoxLevel3Clusters.Items.Clear();
                comboBoxLevel3Clusters.Enabled = false;
            }
            if (listBoxLevel3FreeSubblockPoints != null)
                listBoxLevel3FreeSubblockPoints.Items.Clear();
            if (listBoxLevel3SelectedSubblockPoints != null)
                listBoxLevel3SelectedSubblockPoints.Items.Clear();

            if (numericUpDownLevel3SubblocksCount != null)
                numericUpDownLevel3SubblocksCount.Enabled = false;
            if (numericUpDownLevel3PointsPerSubblock != null)
                numericUpDownLevel3PointsPerSubblock.Enabled = false;
            if (buttonLevel3CreateSubblocks != null)
                buttonLevel3CreateSubblocks.Enabled = false;
            if (buttonLevel3MoveToSubblock != null)
                buttonLevel3MoveToSubblock.Enabled = false;
            if (buttonLevel3RemoveFromSubblock != null)
                buttonLevel3RemoveFromSubblock.Enabled = false;
            if (buttonLevel3AutoDistribute != null)
                buttonLevel3AutoDistribute.Enabled = false;
            if (buttonLevel3ConfirmSubblocks != null)
                buttonLevel3ConfirmSubblocks.Enabled = false;
            if (buttonShowLevel3Charts != null)
                buttonShowLevel3Charts.Enabled = false;
            if (buttonLevel3Response != null)
                buttonLevel3Response.Enabled = false;
            if (buttonShowLevel3ResponseCharts != null)
                buttonShowLevel3ResponseCharts.Enabled = false;
            if (labelLevel3SubblockRequired != null)
                labelLevel3SubblockRequired.Text = "Требуется точек: -";
            if (labelLevel3Summary != null)
                labelLevel3Summary.Text = "Выберите блок и подготовьте набор контрольных точек.";
            if (tabPageLevel3Calculation != null)
                tabPageLevel3Calculation.Enabled = false;
            if (tabControlLevel3 != null && tabPageLevel3Distribution != null)
                tabControlLevel3.SelectedTab = tabPageLevel3Distribution;
        }

        private void ComboBoxLevel3Blocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedLevel3Block();
        }

        private void LoadSelectedLevel3Block()
        {
            ResetLevel3State();

            if (comboBoxLevel3Blocks == null || comboBoxLevel3Blocks.SelectedItem == null)
            {
                RefreshLevel3BlockPreparationLists();
                return;
            }

            string blockName = comboBoxLevel3Blocks.SelectedItem.ToString();
            _level3PreparedBlockName = blockName;
            if (_blockData.ContainsKey(blockName))
                _level3PreparedBlockPoints = new List<string>(_blockData[blockName]);

            RefreshLevel3BlockPreparationLists();
            buttonLevel3.Enabled = _level3PreparedBlockPoints.Count >= 2;
            labelLevel3Summary.Text =
                $"Блок {blockName}: {_level3PreparedBlockPoints.Count} контрольных точек. " +
                "При необходимости добавьте нераспределённые точки или удалите лишние.";
        }

        private void RefreshLevel3BlockPreparationLists()
        {
            if (listBoxLevel3BlockPoints == null || listBoxLevel3AvailableBlockPoints == null)
                return;

            listBoxLevel3BlockPoints.Items.Clear();
            foreach (string point in _level3PreparedBlockPoints)
                listBoxLevel3BlockPoints.Items.Add(point);

            listBoxLevel3AvailableBlockPoints.Items.Clear();
            if (_sourceTable == null)
                return;

            HashSet<string> assignedToOtherBlocks = new HashSet<string>(
                _blockData
                    .Where(pair => pair.Key != _level3PreparedBlockName)
                    .SelectMany(pair => pair.Value));

            foreach (string point in _sourceTable.Columns
                         .Cast<DataColumn>()
                         .Skip(1)
                         .Select(column => column.ColumnName))
            {
                if (!assignedToOtherBlocks.Contains(point) &&
                    !_level3PreparedBlockPoints.Contains(point))
                {
                    listBoxLevel3AvailableBlockPoints.Items.Add(point);
                }
            }
        }

        private void ButtonLevel3AddPointToBlock_Click(object sender, EventArgs e)
        {
            if (listBoxLevel3AvailableBlockPoints.SelectedItem == null)
                return;

            string point = listBoxLevel3AvailableBlockPoints.SelectedItem.ToString();
            if (!_level3PreparedBlockPoints.Contains(point))
                _level3PreparedBlockPoints.Add(point);

            InvalidateLevel3PreparedCalculation();
            RefreshLevel3BlockPreparationLists();
        }

        private void ButtonLevel3RemovePointFromBlock_Click(object sender, EventArgs e)
        {
            if (listBoxLevel3BlockPoints.SelectedItem == null)
                return;

            string point = listBoxLevel3BlockPoints.SelectedItem.ToString();
            _level3PreparedBlockPoints.Remove(point);

            InvalidateLevel3PreparedCalculation();
            RefreshLevel3BlockPreparationLists();
        }

        private void InvalidateLevel3PreparedCalculation()
        {
            _level3BlockPrepared = false;
            _level3SubblocksConfirmed = false;
            _level3SubblockData.Clear();
            _lastLevel3Result = null;
            _lastLevel3ResponseResult = null;

            dataGridViewLevel3Differences.DataSource = null;
            dataGridViewLevel3Links.DataSource = null;
            dataGridViewLevel3Response.DataSource = null;
            comboBoxLevel3Subblocks.Items.Clear();
            comboBoxLevel3Subblocks.Enabled = false;
            comboBoxLevel3Clusters.Items.Clear();
            comboBoxLevel3Clusters.Enabled = false;
            listBoxLevel3FreeSubblockPoints.Items.Clear();
            listBoxLevel3SelectedSubblockPoints.Items.Clear();

            numericUpDownLevel3SubblocksCount.Enabled = false;
            numericUpDownLevel3PointsPerSubblock.Enabled = false;
            buttonLevel3CreateSubblocks.Enabled = false;
            buttonLevel3MoveToSubblock.Enabled = false;
            buttonLevel3RemoveFromSubblock.Enabled = false;
            buttonLevel3AutoDistribute.Enabled = false;
            buttonLevel3ConfirmSubblocks.Enabled = false;
            buttonShowLevel3Charts.Enabled = false;
            buttonLevel3Response.Enabled = false;
            buttonShowLevel3ResponseCharts.Enabled = false;
            buttonLevel3.Enabled = _level3PreparedBlockPoints.Count >= 2;
            labelLevel3SubblockRequired.Text = "Требуется точек: -";
        }

        private void ButtonLevel3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_blocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по блокам на II уровне.");
                    return;
                }
                if (comboBoxLevel3Blocks.SelectedItem == null)
                {
                    ShowError("Выберите блок для разбиения.");
                    return;
                }
                if (_level3PreparedBlockPoints.Count < 2)
                {
                    ShowError("Для III уровня требуется минимум две контрольные точки.");
                    return;
                }

                _level3PreparedBlockName = comboBoxLevel3Blocks.SelectedItem.ToString();
                _blockData[_level3PreparedBlockName] =
                    new List<string>(_level3PreparedBlockPoints);

                double rigidTolerance = (double)numericUpDownLevel3Tolerance.Value;
                Level3StructuralResult result = _engine.RunLevel3Structural(
                    _cycles,
                    new List<string>(_level3PreparedBlockPoints),
                    _level3PreparedBlockName,
                    GetSelectedClusteringMethod(),
                    rigidTolerance);

                _lastLevel3Result = result;
                _level3BlockPrepared = true;
                ClearLevel3SubblockDistribution();

                dataGridViewLevel3Differences.DataSource =
                    BuildLevel3DifferencesTable(_level3PreparedBlockPoints);
                dataGridViewLevel3Links.DataSource = BuildLevel3LinksTable(result);
                dataGridViewLevel3Response.DataSource = null;

                PrepareLevel3SubblockControls();
                buttonShowLevel3Charts.Enabled = result.Links.Count > 0;

                labelLevel3Summary.Text =
                    $"Блок {_level3PreparedBlockName} подтверждён. " +
                    $"Точек: {_level3PreparedBlockPoints.Count}; связей проверено: {result.Links.Count}. " +
                    "Укажите количество подблоков и точек на каждом подблоке.";

                MessageBox.Show(
                    "Состав блока подтверждён. Таблицы разностей координат и жёсткости связей рассчитаны.",
                    "III уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подготовки III уровня: {ex.Message}");
            }
        }

        private void PrepareLevel3SubblockControls()
        {
            int pointCount = _level3PreparedBlockPoints.Count;
            int maxSubblocks = Math.Max(1, pointCount / 2);

            numericUpDownLevel3SubblocksCount.Maximum = maxSubblocks;
            numericUpDownLevel3SubblocksCount.Value = Math.Min(2, maxSubblocks);
            numericUpDownLevel3SubblocksCount.Enabled = true;

            numericUpDownLevel3PointsPerSubblock.Maximum = Math.Max(2, pointCount);
            int initialSubblocks = (int)numericUpDownLevel3SubblocksCount.Value;
            int initialPoints = Math.Max(2, pointCount / initialSubblocks);
            numericUpDownLevel3PointsPerSubblock.Value =
                Math.Min(numericUpDownLevel3PointsPerSubblock.Maximum, initialPoints);
            numericUpDownLevel3PointsPerSubblock.Enabled = true;

            buttonLevel3CreateSubblocks.Enabled = true;
            UpdateLevel3SubblockRequirementLabel();
        }

        private void Level3SubblockSettings_ValueChanged(object sender, EventArgs e)
        {
            if (_level3SubblockData.Count > 0)
                ClearLevel3SubblockDistribution();
            UpdateLevel3SubblockRequirementLabel();
        }

        private void ClearLevel3SubblockDistribution()
        {
            _level3SubblockData.Clear();
            _level3SubblocksConfirmed = false;
            _lastLevel3ResponseResult = null;

            comboBoxLevel3Subblocks.Items.Clear();
            comboBoxLevel3Subblocks.Enabled = false;
            listBoxLevel3FreeSubblockPoints.Items.Clear();
            listBoxLevel3SelectedSubblockPoints.Items.Clear();
            listBoxLevel3FreeSubblockPoints.Enabled = false;
            listBoxLevel3SelectedSubblockPoints.Enabled = false;
            buttonLevel3MoveToSubblock.Enabled = false;
            buttonLevel3RemoveFromSubblock.Enabled = false;
            buttonLevel3AutoDistribute.Enabled = false;
            buttonLevel3ConfirmSubblocks.Enabled = false;

            comboBoxLevel3Clusters.Items.Clear();
            comboBoxLevel3Clusters.Enabled = false;
            buttonLevel3Response.Enabled = false;
            buttonShowLevel3ResponseCharts.Enabled = false;
            dataGridViewLevel3Response.DataSource = null;
            tabPageLevel3Calculation.Enabled = false;
        }

        private void UpdateLevel3SubblockRequirementLabel()
        {
            if (labelLevel3SubblockRequired == null)
                return;

            int subblocks = (int)numericUpDownLevel3SubblocksCount.Value;
            int perSubblock = (int)numericUpDownLevel3PointsPerSubblock.Value;
            int required = subblocks * perSubblock;
            labelLevel3SubblockRequired.Text =
                $"Нужно: {required} из {_level3PreparedBlockPoints.Count}";
        }

        private void ButtonLevel3CreateSubblocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_level3BlockPrepared)
                {
                    ShowError("Сначала подтвердите состав разбиваемого блока.");
                    return;
                }

                int count = (int)numericUpDownLevel3SubblocksCount.Value;
                int perSubblock = (int)numericUpDownLevel3PointsPerSubblock.Value;
                int required = count * perSubblock;

                if (required != _level3PreparedBlockPoints.Count)
                {
                    ShowError(
                        $"Количество точек должно распределяться без остатка.\n" +
                        $"Сейчас требуется {required}, а в блоке {_level3PreparedBlockPoints.Count} точек.\n" +
                        "Измените количество подблоков, число точек на подблоке или состав блока.");
                    return;
                }

                _level3SubblockData.Clear();
                comboBoxLevel3Subblocks.Items.Clear();
                for (int i = 1; i <= count; i++)
                {
                    string name = $"Подблок {i}";
                    _level3SubblockData[name] = new List<string>();
                    comboBoxLevel3Subblocks.Items.Add(name);
                }

                comboBoxLevel3Subblocks.Enabled = true;
                listBoxLevel3FreeSubblockPoints.Enabled = true;
                listBoxLevel3SelectedSubblockPoints.Enabled = true;
                buttonLevel3MoveToSubblock.Enabled = true;
                buttonLevel3RemoveFromSubblock.Enabled = true;
                buttonLevel3AutoDistribute.Enabled = true;
                buttonLevel3ConfirmSubblocks.Enabled = true;
                _level3SubblocksConfirmed = false;

                if (comboBoxLevel3Subblocks.Items.Count > 0)
                    comboBoxLevel3Subblocks.SelectedIndex = 0;

                RefreshLevel3SubblockLists();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка создания подблоков: {ex.Message}");
            }
        }

        private void ComboBoxLevel3Subblocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshLevel3SubblockLists();
        }

        private void RefreshLevel3SubblockLists()
        {
            if (listBoxLevel3FreeSubblockPoints == null ||
                listBoxLevel3SelectedSubblockPoints == null)
                return;

            listBoxLevel3SelectedSubblockPoints.Items.Clear();
            if (comboBoxLevel3Subblocks.SelectedItem != null)
            {
                string selectedName = comboBoxLevel3Subblocks.SelectedItem.ToString();
                if (_level3SubblockData.ContainsKey(selectedName))
                {
                    foreach (string point in _level3SubblockData[selectedName])
                        listBoxLevel3SelectedSubblockPoints.Items.Add(point);
                }
            }

            listBoxLevel3FreeSubblockPoints.Items.Clear();
            HashSet<string> assigned = new HashSet<string>(
                _level3SubblockData.Values.SelectMany(points => points));
            foreach (string point in _level3PreparedBlockPoints)
            {
                if (!assigned.Contains(point))
                    listBoxLevel3FreeSubblockPoints.Items.Add(point);
            }
        }

        private void ButtonLevel3MoveToSubblock_Click(object sender, EventArgs e)
        {
            if (comboBoxLevel3Subblocks.SelectedItem == null ||
                listBoxLevel3FreeSubblockPoints.SelectedItem == null)
                return;

            string subblockName = comboBoxLevel3Subblocks.SelectedItem.ToString();
            string point = listBoxLevel3FreeSubblockPoints.SelectedItem.ToString();
            int required = (int)numericUpDownLevel3PointsPerSubblock.Value;

            if (_level3SubblockData[subblockName].Count >= required)
            {
                ShowError($"{subblockName} уже содержит требуемое количество точек: {required}.");
                return;
            }

            _level3SubblockData[subblockName].Add(point);
            _level3SubblocksConfirmed = false;
            RefreshLevel3SubblockLists();
        }

        private void ButtonLevel3RemoveFromSubblock_Click(object sender, EventArgs e)
        {
            if (comboBoxLevel3Subblocks.SelectedItem == null ||
                listBoxLevel3SelectedSubblockPoints.SelectedItem == null)
                return;

            string subblockName = comboBoxLevel3Subblocks.SelectedItem.ToString();
            string point = listBoxLevel3SelectedSubblockPoints.SelectedItem.ToString();
            _level3SubblockData[subblockName].Remove(point);
            _level3SubblocksConfirmed = false;
            RefreshLevel3SubblockLists();
        }

        private void ButtonLevel3AutoDistribute_Click(object sender, EventArgs e)
        {
            try
            {
                if (_level3SubblockData.Count == 0)
                {
                    ShowError("Сначала создайте подблоки.");
                    return;
                }

                foreach (List<string> points in _level3SubblockData.Values)
                    points.Clear();

                int perSubblock = (int)numericUpDownLevel3PointsPerSubblock.Value;
                int index = 0;
                foreach (string subblockName in _level3SubblockData.Keys.OrderBy(ParseLevel3SubblockNumber))
                {
                    for (int i = 0; i < perSubblock; i++)
                    {
                        _level3SubblockData[subblockName].Add(_level3PreparedBlockPoints[index]);
                        index++;
                    }
                }

                _level3SubblocksConfirmed = false;
                RefreshLevel3SubblockLists();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка автораспределения по подблокам: {ex.Message}");
            }
        }

        private int ParseLevel3SubblockNumber(string name)
        {
            string number = new string(name.Where(char.IsDigit).ToArray());
            return int.TryParse(number, out int value) ? value : int.MaxValue;
        }

        private void ButtonLevel3ConfirmSubblocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (_level3SubblockData.Count == 0)
                {
                    ShowError("Сначала создайте подблоки.");
                    return;
                }

                int required = (int)numericUpDownLevel3PointsPerSubblock.Value;
                foreach (var pair in _level3SubblockData)
                {
                    if (pair.Value.Count != required)
                    {
                        ShowError(
                            $"{pair.Key} содержит {pair.Value.Count} точек. Требуется строго {required}.");
                        return;
                    }
                }

                HashSet<string> assigned = new HashSet<string>(
                    _level3SubblockData.Values.SelectMany(points => points));
                if (assigned.Count != _level3PreparedBlockPoints.Count)
                {
                    ShowError("Не все точки блока распределены по подблокам.");
                    return;
                }

                Level3StructuralResult manualResult = new Level3StructuralResult
                {
                    BlockName = _level3PreparedBlockName,
                    MethodType = GetSelectedClusteringMethod()
                };

                if (_lastLevel3Result != null)
                {
                    foreach (RigidLink link in _lastLevel3Result.Links)
                        manualResult.Links.Add(link);
                }

                int clusterId = 0;
                foreach (string subblockName in _level3SubblockData.Keys.OrderBy(ParseLevel3SubblockNumber))
                {
                    StructuralCluster cluster = new StructuralCluster
                    {
                        ClusterId = clusterId,
                        IsIsolatedPoint = false
                    };
                    foreach (string point in _level3SubblockData[subblockName])
                        cluster.PointNames.Add(point);
                    manualResult.Clusters.Add(cluster);
                    clusterId++;
                }
                manualResult.RecommendedClusterId = manualResult.Clusters.Count > 0 ? 0 : -1;
                _lastLevel3Result = manualResult;
                _level3SubblocksConfirmed = true;
                _lastLevel3ResponseResult = null;

                comboBoxLevel3Clusters.Items.Clear();
                foreach (string subblockName in _level3SubblockData.Keys.OrderBy(ParseLevel3SubblockNumber))
                    comboBoxLevel3Clusters.Items.Add(subblockName);

                comboBoxLevel3Clusters.Enabled = comboBoxLevel3Clusters.Items.Count > 0;
                buttonLevel3Response.Enabled = comboBoxLevel3Clusters.Items.Count > 0;
                buttonShowLevel3ResponseCharts.Enabled = false;
                dataGridViewLevel3Response.DataSource = null;

                if (comboBoxLevel3Clusters.Items.Count > 0)
                    comboBoxLevel3Clusters.SelectedIndex = 0;

                tabPageLevel3Calculation.Text =
                    $"Расчёты и графики — блок {_level3PreparedBlockName}";
                tabPageLevel3Calculation.Enabled = true;
                tabControlLevel3.SelectedTab = tabPageLevel3Calculation;
                labelLevel3Summary.Text =
                    $"Распределение подтверждено: {_level3SubblockData.Count} подблоков, " +
                    $"{required} точек на каждом.";

                UpdateLevel4Points();

                MessageBox.Show(
                    "Распределение контрольных точек по подблокам подтверждено. " +
                    "Вкладка «Расчёты и графики» доступна.",
                    "III уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подтверждения подблоков: {ex.Message}");
            }
        }

        private DataTable BuildLevel3DifferencesTable(List<string> points)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Цикл", typeof(string));

            var pairs = new List<Tuple<string, string>>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    string pointA = points[i];
                    string pointB = points[j];
                    pairs.Add(Tuple.Create(pointA, pointB));
                    table.Columns.Add($"ΔZ({pointA}-{pointB})", typeof(double));
                }
            }

            foreach (MeasurementCycle cycle in _cycles)
            {
                DataRow row = table.NewRow();
                row["Цикл"] = cycle.Label;
                foreach (Tuple<string, string> pair in pairs)
                {
                    if (cycle.PointValues.TryGetValue(pair.Item1, out double valueA) &&
                        cycle.PointValues.TryGetValue(pair.Item2, out double valueB))
                    {
                        row[$"ΔZ({pair.Item1}-{pair.Item2})"] = valueA - valueB;
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }

        private void ButtonShowLevel3Charts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lastLevel3Result == null || _lastLevel3Result.Links.Count == 0)
                {
                    ShowError("Сначала подтвердите блок и рассчитайте таблицу жёсткости.");
                    return;
                }
                using (Level3ChartsForm chartsForm = new Level3ChartsForm(_lastLevel3Result))
                {
                    chartsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка открытия графиков III уровня: {ex.Message}");
            }
        }

        private DataTable BuildLevel3LinksTable(Level3StructuralResult result)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Точка A", typeof(string));
            table.Columns.Add("Точка B", typeof(string));
            table.Columns.Add("Эталонное расстояние", typeof(double));
            table.Columns.Add("Макс. отклонение", typeof(double));
            table.Columns.Add("Связь", typeof(string));
            foreach (RigidLink link in result.Links)
            {
                DataRow row = table.NewRow();
                row["Точка A"] = link.PointA;
                row["Точка B"] = link.PointB;
                row["Эталонное расстояние"] = link.ReferenceDistance;
                row["Макс. отклонение"] = link.MaxDeviation;
                row["Связь"] = link.IsRigid ? "Жёсткая" : "Не жёсткая";
                table.Rows.Add(row);
            }
            return table;
        }

        private void ButtonLevel3Response_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_level3SubblocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по подблокам.");
                    return;
                }
                if (comboBoxLevel3Clusters.SelectedItem == null)
                {
                    ShowError("Выберите подблок.");
                    return;
                }
                if (!TryReadParameters(out double epsilon, out double alpha))
                    return;

                string subblockName = comboBoxLevel3Clusters.SelectedItem.ToString();
                if (!_level3SubblockData.ContainsKey(subblockName))
                {
                    ShowError("Не найден выбранный подблок.");
                    return;
                }

                int clusterId = comboBoxLevel3Clusters.SelectedIndex;
                Level3ResponseResult result = _engine.RunLevel3Response(
                    _cycles,
                    _level3SubblockData[subblockName],
                    _level3PreparedBlockName,
                    clusterId,
                    epsilon,
                    alpha);

                _lastLevel3ResponseResult = result;
                dataGridViewLevel3Response.DataSource = BuildLevel3ResponseTable(result);
                buttonShowLevel3ResponseCharts.Enabled = true;

                MessageBox.Show(
                    $"Расчёт III уровня завершён для {subblockName}.",
                    "III уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка расчёта III уровня: {ex.Message}");
            }
        }

        private void ButtonShowLevel3ResponseCharts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lastLevel3ResponseResult == null || _lastLevel3ResponseResult.Items.Count == 0)
                {
                    ShowError("Сначала выполните расчёт выбранного подблока.");
                    return;
                }
                using (Level3ResponseChartsForm chartsForm =
                       new Level3ResponseChartsForm(_lastLevel3ResponseResult))
                {
                    chartsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка открытия графиков III уровня: {ex.Message}");
            }
        }

        private DataTable BuildLevel3ResponseTable(Level3ResponseResult result)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Цикл", typeof(string));
            table.Columns.Add("M", typeof(double));
            table.Columns.Add("M+E", typeof(double));
            table.Columns.Add("M-E", typeof(double));
            table.Columns.Add("A", typeof(double));
            table.Columns.Add("A+E", typeof(double));
            table.Columns.Add("A-E", typeof(double));
            table.Columns.Add("R", typeof(double));
            table.Columns.Add("L", typeof(double));
            table.Columns.Add("Статус", typeof(string));

            foreach (PhaseStateItem item in result.Items)
            {
                DataRow row = table.NewRow();
                row["Цикл"] = item.Label;
                row["M"] = item.M;
                row["M+E"] = item.MPlusE;
                row["M-E"] = item.MMinusE;
                row["A"] = item.A;
                row["A+E"] = item.APlusE;
                row["A-E"] = item.AMinusE;
                row["R"] = item.R;
                row["L"] = item.L;
                row["Статус"] = item.StatusText;
                table.Rows.Add(row);
            }

            DataRow forecastRow = table.NewRow();
            forecastRow["Цикл"] = "Прогноз";
            forecastRow["M"] = result.ForecastM;
            forecastRow["M+E"] = result.ForecastMPlusE;
            forecastRow["M-E"] = result.ForecastMMinusE;
            forecastRow["A"] = result.ForecastA;
            forecastRow["A+E"] = result.ForecastAPlusE;
            forecastRow["A-E"] = result.ForecastAMinusE;
            forecastRow["R"] = result.ForecastR;
            forecastRow["L"] = result.ForecastL;
            forecastRow["Статус"] = result.ForecastStatusText;
            table.Rows.Add(forecastRow);
            return table;
        }

        private void DataGridViewLevel3_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is DataGridView grid)
            {
                if (grid.Columns[e.ColumnIndex].Name == "Статус")
                {
                    string status = Convert.ToString(e.Value)?.ToLower();
                    if (status.Contains("без изменений"))
                    {
                        e.CellStyle.BackColor = Color.LightGreen;
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    else if (status.Contains("авар"))
                    {
                        e.CellStyle.BackColor = Color.LightCoral;
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    return;
                }

                if (grid.Columns[e.ColumnIndex].Name == "Связь")
                {
                    string linkStatus = Convert.ToString(e.Value)?.ToLower();
                    e.CellStyle.BackColor =
                        linkStatus.Contains("не жёсткая") ? Color.LightCoral : Color.LightGreen;
                    e.CellStyle.ForeColor = Color.Black;
                    return;
                }
            }

            if (e.Value == null || e.Value == DBNull.Value)
                return;

            if (e.Value is double || e.Value is float || e.Value is decimal)
            {
                double value = Convert.ToDouble(e.Value);
                e.Value = value.ToString("0.######", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
                return;
            }

            string text = Convert.ToString(e.Value);
            if (string.IsNullOrWhiteSpace(text))
                return;

            text = text.Replace(',', '.');
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                e.Value = parsed.ToString("0.######", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
            }
        }

        private void DataGridViewLevel3Features_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewLevel3_CellFormatting(sender, e);
        }

        private ClusteringMethodType GetSelectedClusteringMethod()
        {
            return ClusteringMethodType.Hierarchical;
        }

        #endregion

        #region 7. IV уровень декомпозиции

        private void UpdateLevel4Points()
        {
            if (checkedListBoxLevel4Points == null)
                return;
            HashSet<string> previousSelected = new HashSet<string>(GetSelectedLevel4Points());
            checkedListBoxLevel4Points.Items.Clear();
            _lastLevel4Result = null;
            _lastLevel4SelectedPoints.Clear();
            buttonShowLevel4Charts.Enabled = false;
            dataGridViewLevel4.DataSource = null;
            IEnumerable<string> points = Enumerable.Empty<string>();
            if (_cycles != null && _cycles.Count > 0)
            {
                points = _cycles[0].PointValues.Keys.OrderBy(x => x);
                if (radioButtonLevel4StandalonePoints != null && radioButtonLevel4StandalonePoints.Checked)
                {
                    var standalone = GetLevel3StandalonePointNames();
                    points = points.Where(p => standalone.Contains(p)).OrderBy(p => p);
                }
            }
            bool hasCheckedPoint = false;
            foreach (string pointName in points)
            {
                bool isChecked = previousSelected.Contains(pointName);
                checkedListBoxLevel4Points.Items.Add(pointName, isChecked);
                hasCheckedPoint |= isChecked;
            }
            if (!hasCheckedPoint && previousSelected.Count == 0 && checkedListBoxLevel4Points.Items.Count > 0)
                checkedListBoxLevel4Points.SetItemChecked(0, true);
            UpdateLevel4Buttons();
            UpdateLevel4PreviewChart();
        }
        private void UpdateLevel4Buttons()
        {
            int itemsCount = checkedListBoxLevel4Points == null ? 0 : checkedListBoxLevel4Points.Items.Count;
            int selectedCount = GetSelectedLevel4Points().Count;
            if (buttonLevel4 != null)
                buttonLevel4.Enabled = selectedCount > 0;
            if (buttonLevel4SelectAll != null)
                buttonLevel4SelectAll.Enabled = itemsCount > 0;
            if (buttonLevel4ClearSelection != null)
                buttonLevel4ClearSelection.Enabled = selectedCount > 0;
        }
        private List<string> GetSelectedLevel4Points()
        {
            if (checkedListBoxLevel4Points == null)
                return new List<string>();
            return checkedListBoxLevel4Points.CheckedItems
                .Cast<object>()
                .Select(item => Convert.ToString(item))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();
        }
        private void ButtonLevel4SelectAll_Click(object sender, EventArgs e)
        {
            if (checkedListBoxLevel4Points == null)
                return;
            for (int i = 0; i < checkedListBoxLevel4Points.Items.Count; i++)
                checkedListBoxLevel4Points.SetItemChecked(i, true);
            _lastLevel4Result = null;
            _lastLevel4SelectedPoints.Clear();
            buttonShowLevel4Charts.Enabled = checkedListBoxLevel4Points.Items.Count > 0;
            UpdateLevel4Buttons();
            UpdateLevel4PreviewChart();
        }
        private void ButtonLevel4ClearSelection_Click(object sender, EventArgs e)
        {
            if (checkedListBoxLevel4Points == null)
                return;
            for (int i = 0; i < checkedListBoxLevel4Points.Items.Count; i++)
                checkedListBoxLevel4Points.SetItemChecked(i, false);
            _lastLevel4Result = null;
            _lastLevel4SelectedPoints.Clear();
            buttonShowLevel4Charts.Enabled = false;
            UpdateLevel4Buttons();
            UpdateLevel4PreviewChart();
        }
        private HashSet<string> GetLevel3StandalonePointNames()
        {
            var result = new HashSet<string>();
            if (_lastLevel3Result == null)
                return result;
            foreach (string point in _lastLevel3Result.StandalonePoints)
                result.Add(point);
            var clustered = new HashSet<string>();
            foreach (var cluster in _lastLevel3Result.Clusters)
            {
                foreach (string point in cluster.PointNames)
                    clustered.Add(point);
            }
            if (_cycles != null && _cycles.Count > 0)
            {
                foreach (string point in _cycles[0].PointValues.Keys)
                {
                    if (!clustered.Contains(point))
                        result.Add(point);
                }
            }
            return result;
        }
        private void UpdateLevel4PreviewChart()
        {
            if (chartLevel4Preview == null)
                return;
            chartLevel4Preview.Series.Clear();
            if (_cycles == null || _cycles.Count == 0)
                return;
            List<string> selectedPoints = GetSelectedLevel4Points();
            if (selectedPoints.Count == 0)
                return;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            foreach (string pointName in selectedPoints)
            {
                var series = new Series($"Z(t), {pointName}")
                {
                    ChartType = SeriesChartType.Spline,
                    BorderWidth = 2,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 5,
                    IsValueShownAsLabel = selectedPoints.Count <= 3
                };
                series["SplineTension"] = "0.12";
                ChartForecastHelper.ApplyReadablePointLabels(series);
                for (int i = 0; i < _cycles.Count; i++)
                {
                    if (!_cycles[i].PointValues.TryGetValue(pointName, out double z))
                        continue;
                    int p = series.Points.AddXY(i, z);
                    series.Points[p].AxisLabel = _cycles[i].Label;
                    if (selectedPoints.Count <= 3)
                        series.Points[p].Label = z.ToString("0.#####", CultureInfo.InvariantCulture);
                    minY = Math.Min(minY, z);
                    maxY = Math.Max(maxY, z);
                }
                chartLevel4Preview.Series.Add(series);
            }
            var area = chartLevel4Preview.ChartAreas[0];
            ChartForecastHelper.PrepareTimeArea(area, "Координата Z", _cycles.Select(cycle => cycle.Label).ToList(), false);
            area.AxisX.Title = "Цикл наблюдения, t";
            if (minY < maxY)
            {
                double dy = (maxY - minY) * 0.10;
                if (dy == 0) dy = 0.001;
                area.AxisY.Minimum = minY - dy;
                area.AxisY.Maximum = maxY + dy;
            }
            else
            {
                area.AxisY.Minimum = double.NaN;
                area.AxisY.Maximum = double.NaN;
            }
        }
        private void ButtonLevel4_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cycles == null || _cycles.Count == 0)
                {
                    ShowError("Нет данных для IV уровня декомпозиции.");
                    return;
                }
                List<string> selectedPoints = GetSelectedLevel4Points();
                if (selectedPoints.Count == 0)
                {
                    ShowError("Выберите одну или несколько контрольных точек.");
                    return;
                }
                if (!TryReadParameters(out double epsilon, out double alpha))
                    return;
                Level4AnalysisResult result = _engine.RunLevel4(_cycles, selectedPoints, epsilon, alpha);
                _lastLevel4Result = result;
                _lastLevel4SelectedPoints = selectedPoints.ToList();
                dataGridViewLevel4.DataSource = BuildLevel4ResultTable(result);
                buttonShowLevel4Charts.Enabled = true;
                int changedCount = result.Forecasts.Count(f =>
                    !string.Equals(f.ForecastStatusText, "Без изменений", StringComparison.OrdinalIgnoreCase));
                labelLevel4Summary.Text = $"Выбрано точек: {result.PointNames.Count}. Прогноз с изменениями: {changedCount}.";
                tabControlMain.SelectedTab = tabPageLevel4;
                MessageBox.Show($"Расчёт IV уровня завершён. Выбрано точек: {result.PointNames.Count}.", "IV уровень",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка расчёта IV уровня: {ex.Message}");
            }
        }
        private void ButtonShowLevel4Charts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cycles == null || _cycles.Count == 0)
                {
                    ShowError("Нет данных для IV уровня декомпозиции.");
                    return;
                }

                List<string> selectedPoints = GetSelectedLevel4Points();
                if (selectedPoints.Count == 0)
                {
                    ShowError("Выберите одну или несколько контрольных точек для построения графиков Z(t).");
                    return;
                }

                if (!TryReadParameters(out double epsilon, out double alpha))
                    return;

                Level4AnalysisResult chartResult = _engine.RunLevel4(_cycles, selectedPoints, epsilon, alpha);
                _lastLevel4Result = chartResult;
                _lastLevel4SelectedPoints = selectedPoints.ToList();
                dataGridViewLevel4.DataSource = BuildLevel4ResultTable(chartResult);
                int changedCount = chartResult.Forecasts.Count(f =>
                    !string.Equals(f.ForecastStatusText, "Без изменений", StringComparison.OrdinalIgnoreCase));
                labelLevel4Summary.Text = $"Выбрано точек: {chartResult.PointNames.Count}. Прогноз с изменениями: {changedCount}.";

                using (Level4ChartsForm chartsForm = new Level4ChartsForm(chartResult, selectedPoints))
                {
                    chartsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка открытия графиков IV уровня: {ex.Message}");
            }
        }
        private DataTable BuildLevel4ResultTable(Level4AnalysisResult result)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Цикл", typeof(string));
            table.Columns.Add("Точка", typeof(string));
            table.Columns.Add("Z", typeof(double));
            table.Columns.Add("Z+E", typeof(double));
            table.Columns.Add("Z-E", typeof(double));
            table.Columns.Add("R", typeof(double));
            table.Columns.Add("L", typeof(double));
            table.Columns.Add("Статус", typeof(string));
            foreach (Level4PointStateItem item in result.Items)
            {
                DataRow row = table.NewRow();
                row["Цикл"] = item.CycleLabel;
                row["Точка"] = item.PointName;
                row["Z"] = item.Z;
                row["Z+E"] = item.ZPlusE;
                row["Z-E"] = item.ZMinusE;
                row["R"] = item.R;
                row["L"] = item.L;
                row["Статус"] = item.StatusText;
                table.Rows.Add(row);
            }
            foreach (Level4PointForecastItem forecast in result.Forecasts)
            {
                DataRow forecastRow = table.NewRow();
                forecastRow["Цикл"] = "Прогноз";
                forecastRow["Точка"] = forecast.PointName;
                forecastRow["Z"] = forecast.ForecastZ;
                forecastRow["Z+E"] = forecast.ForecastZPlusE;
                forecastRow["Z-E"] = forecast.ForecastZMinusE;
                forecastRow["R"] = forecast.R;
                forecastRow["L"] = forecast.L;
                forecastRow["Статус"] = forecast.ForecastStatusText;
                table.Rows.Add(forecastRow);
            }
            return table;
        }
        #endregion

        #region 8. Справка и руководство пользователя

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            using (HelpGuideForm helpForm = new HelpGuideForm())
            {
                helpForm.ShowDialog(this);
            }
        }

        private sealed class HelpGuideForm : Form
        {
            private readonly TabControl tabControlHelp;

            private const string UserManualText = @"Справка по использованию программы

1. Данные
- Нажмите кнопку подключения базы данных (Открыть БД) и выберите файл SQLite (*.db, *.sqlite).
- После загрузки в списке (Таблица) выберите ""Данные"", затем загрузите таблицу (Загрузить таблицу)
- Появится таблица с исходными данными (количество точек - строки; количество циклов - столбцы), которые можно редактировать с помощью кнопок удаления или добавления строки(Добавить строку/Удалить строку), а также непосредственного изменения значений в ячейках таблицы.
- В правом окне можно будет приблизить или отдалить с помощью колёсика мыши схему объекта, которая прикреплена к данной таблице.
- Схему объекта можно заменить кнопкой (Заменить схему), выбрав файл изображения (*.png, *.jpg)
- Поля E: и A: являются коэффициентами (коэффициент погрешности и коэффициент экспоненциального сглаживания соответственно) , влияющие на отображение дальнейших расчётов. При загрузке БД они будут установлены по умолчанию, но их так же можно менять следующим образом:
• Е - в разных диапазонах, но для адекватности отображения придерживаться значений от 0.01 до 0.00001.
• А - строго в диапазонах от 0 до 1.
- После внесённых изменений необходимо их сохранить (Сохранить изменения)

2. Первый уровень декомпозиции
- Чтобы увидеть расчётные значения по всему техногенному объекту, необходимо запустить первый уровень декомпозиции (Запустить l уровень декомпозиции)
- После можно будет увидеть расчётные значения в виде таблицы по каждому циклу, а так же статус объекта на каждый момент времени (Без изменений/Предаварийное/Аварийное)
- Чтобы просмотреть все построенные графики, необходимо нажать на соответствующую кнопку (Показать графики). В выпадающем списке можно изменить тип отображаемого графика в зависимости от того, какую информацию или какой график Вам необходим:
• Фазовая траектория альфа от мю
• Функция мю от т
• Функция альфа от т
• Сравнение сглаживания мю от т
• Сравнение сглаживания альфа от т
- В данном окне можно отключить и включить отображение Основной траектории функции, её отклонения на коэффициент погрешности, а так же прогноз любой из рассматриваемых функций. Для того, чтобы это сделать снимите или поставьте галочку на соответствующей кнопке (Основная траектория, +Е, -Е, Прогноз)
После выбора таблицы автоматически загружаются координаты высот.

3. Второй уровень декомпозиции
Второй уровень декомпозиции располагает двумя вкладками:

• Распределение точек:

- Здесь можно поменять количество блоков.
- Чтобы продолжить выполнение программы, нужно создать блоки (Создать блоки).
- Для того, чтобы распределить точки по блокам, нужно воспользоваться схемой в правой части экрана в отдельном окне, масштаб которого можно регулировать с помощью ЛКМ. Выбираем блок и перемещаем из окна ""Свободные точки"" в окно ""Точки выбранного блока"" либо двойным нажатием на точку, либо с помощью кнопок-стрелочек (> и <), те точки, которые расположены на том же блоке на схеме объекта. 

ПРИМЕЧАНИЕ: Количество точек на блоках должно быть одинаковым, т. е. если 2 блока и 11 точек, то на первом блоке 5 и на втором блоке 5

- После чего необходимо подтвердить распределение блоков (Подтвердить распределение) и можно переходить на следующую вкладку.
- Также можно воспользоваться кнопкой ""Автораспределение""

• Расчёт блока:

- По тому же принципу, как и на первом уровне декомпозиции, запускаем расчёт и показываются данные по текущему блоку. Чтобы сменить рассматриваемый блок, необходимо вернуться на вкладку ""Распределение точек"" и выбрать блок в выпадающем списке, после чего вернуться на вкладку ""Расчёт блока"" и снова нажать кнопку запуска второго уровня декомпозиции (Запустить ll уровень декомпозиции)
- Чтобы посмотреть графики, нужно нажать кнопку ""Показать графики"", где так же, как и на первом уровне, будут отображены графики фазовых траекторий, функций мю, функций альфа и сравнение их с разными значениями коэффициентов экспоненциального сглаживания.

4. Третий уровень декомпозиции

Третий уровень декомпозиции располагает двумя вкладками:

    • Распределение контрольных точек по подблокам:

- Здесь можно выбрать разбиваемый блок в соответствующем выпадающем списке, а также, по желанию, перераспределить точки по блокам в зоне 1 (Подготовка контрольных точек разбиваемого блока).
- Можно изменить допуск жёсткости (коэффициент Е).
- Далее подтверждаем выбранный блок и рассчитываем для него таблицы, в которых рассматривается:
1) Разность координат Z контрольных точек;
2) Жёсткость связей между контрольными точками.
- После подтверждения, станет зона 2 (Распределение контрольных точек по подблокам) и график связей, показывающий отклонение между попарно соединёнными точками
- Зона 2 (Распределение контрольных точек по подблокам) предоставляет возможность разбить рассматриваемый блок на подблоки по тому же принципу, что и ранее был реализован на втором уровне декомпозиции. Вы можете изменить количество подблоков, количество точек на каждый подблок или воспользоваться кнопкой Автораспределения, предварительно создав подблоки, нажав именуемую кнопку.
- Чтобы вкладка ""Расчёты и графики"" стала доступной, необходимо разбить рассматриваемый блок на подблоки


⚠ ПРИМЕЧАНИЕ: Количество точек на блоках должно быть одинаковым, т. е. если 2 блока и 11 точек, то на первом блоке 5 и на втором блоке 5. То же правило относится и к подблокам.


    • Расчёты и графики:

- Этот подраздел почти ничем не отличается от предыдущих уровней, потому что имеет тот же принцип расчёта блоков (подблоков).
- Выбираете необходимый к просмотру подблок в выпадающем списке.
- Нажимаете кнопку ""Рассчитать lll уровень"".
- После чего в нижнем окне будет предоставлена таблица со всеми ранее известными показателями и статусом.
- Графики строятся по тому же принципу, что и во всех предыдущих уровнях, но для более конкретной области объекта (Фазовая траектория, функция m, функция a, их сравнение с разными коэффициентами сглаживания)

5. Четвёртый уровень декомпозиции
- Здесь можно рассмотреть, как меняется та или иная точка по высоте в зависимости от времени.
- Выберите рассматриваемые точки, нажав либо ""Все точки"", что позволяет рассмотреть динамику каждой точки отдельной либо ""Только точки вне кластеров lll уровня"", о чём и говорит сама кнопка
- В окне с контрольными точками вы можете выбирать, какие точки рассматривать, а так же выбирать, какие точки необходимо убрать из просмотра.
- Для удобства реализована кнопка ""Выбрать все точки"", что позволяет выбрать сразу все точки, чтобы просмотреть всю картину в целом, и кнопка ""Снять выбор"", которая снимает галочку со всех точек.
- Чтобы просмотреть, как меняется точка во времени, нажмите ""Подтвердить и рассчитать lV уровень"", после чего в нижнем поле появится таблица, где по каждой выбранной точке будет показан статус в каждом диапазоне времени
- В графиках (Показать графики Z(t)) отображаются:
• Изменения координат;
• Допустимые границы;
• Прогноз состояния точки;
• Сглаживание по четырём основным коэффициентам экспоненциального сглаживания.";

            public HelpGuideForm()
            {
                Text = "Справка";
                StartPosition = FormStartPosition.CenterParent;
                MinimumSize = new Size(900, 600);
                Size = new Size(1100, 760);

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(12)
                };
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var title = new Label
                {
                    Text = "Руководство пользователя",
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Margin = new Padding(0, 0, 0, 10)
                };

                tabControlHelp = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular)
                };

                var closePanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(0, 10, 0, 0)
                };
                var buttonClose = new Button
                {
                    Text = "Закрыть",
                    AutoSize = true,
                    DialogResult = DialogResult.OK
                };
                closePanel.Controls.Add(buttonClose);

                BuildHelpTabs();
                root.Controls.Add(title, 0, 0);
                root.Controls.Add(tabControlHelp, 0, 1);
                root.Controls.Add(closePanel, 0, 2);
                Controls.Add(root);
                AcceptButton = buttonClose;
            }

            private void BuildHelpTabs()
            {
                foreach (HelpSection section in ParseManualText(UserManualText))
                {
                    TabPage page = new TabPage(section.Number + ". " + section.Title);
                    RichTextBox textBox = CreateHelpTextBox();
                    FillSectionText(textBox, section);
                    page.Controls.Add(textBox);
                    tabControlHelp.TabPages.Add(page);
                }
            }

            private static RichTextBox CreateHelpTextBox()
            {
                return new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White,
                    DetectUrls = false,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ScrollBars = RichTextBoxScrollBars.Vertical,
                    Margin = new Padding(10)
                };
            }

            private static void FillSectionText(RichTextBox textBox, HelpSection section)
            {
                AppendHelpLine(textBox, section.Number + ". " + section.Title, new Font("Segoe UI", 13, FontStyle.Bold), Color.Black, Color.Empty);
                AppendHelpLine(textBox, string.Empty, textBox.Font, Color.Black, Color.Empty);

                foreach (string line in section.Lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        AppendHelpLine(textBox, string.Empty, textBox.Font, Color.Black, Color.Empty);
                        continue;
                    }

                    if (IsNoteLine(trimmed))
                    {
                        AppendHelpLine(textBox, "⚠ " + trimmed, new Font("Segoe UI", 10, FontStyle.Bold), Color.FromArgb(120, 75, 0), Color.FromArgb(255, 247, 205));
                    }
                    else if (trimmed.StartsWith("•"))
                    {
                        AppendHelpLine(textBox, "    " + trimmed, new Font("Segoe UI", 10, FontStyle.Regular), Color.FromArgb(35, 60, 95), Color.Empty);
                    }
                    else
                    {
                        AppendHelpLine(textBox, trimmed, textBox.Font, Color.Black, Color.Empty);
                    }
                }

                textBox.SelectionStart = 0;
                textBox.SelectionLength = 0;
            }

            private static bool IsNoteLine(string text)
            {
                return text.StartsWith("ПРИМЕЧАНИЕ", StringComparison.OrdinalIgnoreCase);
            }

            private static void AppendHelpLine(RichTextBox textBox, string text, Font font, Color color, Color backColor)
            {
                textBox.SelectionStart = textBox.TextLength;
                textBox.SelectionLength = 0;
                textBox.SelectionFont = font;
                textBox.SelectionColor = color;
                textBox.SelectionBackColor = backColor == Color.Empty ? textBox.BackColor : backColor;
                textBox.AppendText(text + Environment.NewLine);
                textBox.SelectionBackColor = textBox.BackColor;
            }

            private static List<HelpSection> ParseManualText(string text)
            {
                List<HelpSection> sections = new List<HelpSection>();
                HelpSection current = null;
                string[] lines = text.Replace("\r\n", "\n").Split('\n');

                foreach (string rawLine in lines)
                {
                    string line = rawLine.TrimEnd();
                    if (TryReadSectionHeader(line.Trim(), out int number, out string title))
                    {
                        current = new HelpSection(number, title);
                        sections.Add(current);
                        continue;
                    }

                    if (current != null)
                        current.Lines.Add(line);
                }

                return sections;
            }

            private static bool TryReadSectionHeader(string line, out int number, out string title)
            {
                number = 0;
                title = string.Empty;
                int dotIndex = line.IndexOf('.');
                if (dotIndex <= 0)
                    return false;

                string possibleNumber = line.Substring(0, dotIndex).Trim();
                if (!int.TryParse(possibleNumber, out number))
                    return false;

                title = line.Substring(dotIndex + 1).Trim();
                return !string.IsNullOrWhiteSpace(title);
            }

            private sealed class HelpSection
            {
                public int Number { get; }
                public string Title { get; }
                public List<string> Lines { get; } = new List<string>();

                public HelpSection(int number, string title)
                {
                    Number = number;
                    Title = title;
                }
            }
        }

        #endregion

        #region 9. Общие вспомогательные методы формы

        private void FormatNumericCell(DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.Value == DBNull.Value)
                return;
            if (e.Value is double || e.Value is float || e.Value is decimal)
            {
                double value = Convert.ToDouble(e.Value);
                e.Value = value.ToString("0.#####", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
                return;
            }
            if (double.TryParse(Convert.ToString(e.Value)?.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                e.Value = parsed.ToString("0.#####", CultureInfo.InvariantCulture);
                e.FormattingApplied = true;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    }
}

#region 10. Общие помощники построения графиков и прогнозов

public static class ChartForecastHelper
{
    private static readonly double[] CompareAlphas = new double[] { 0.1, 0.4, 0.7, 0.9 };
    private static readonly Color[] CompareColors = new Color[] { Color.Red, Color.Gold, Color.Orange, Color.Green };
    public static double[] GetCompareAlphas()
    {
        return CompareAlphas;
    }
    public static Color GetCompareColor(int index)
    {
        if (index < 0 || index >= CompareColors.Length)
            return Color.Black;
        return CompareColors[index];
    }
    public static string FormatValue(double value)
    {
        return value.ToString("0.#####", CultureInfo.InvariantCulture);
    }
    public static string FormatAlpha(double value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }
    public static void ApplyReadableXAxisLabels(ChartArea area)
    {
        if (area == null)
            return;
        area.AxisX.LabelStyle.Enabled = true;
        area.AxisX.LabelStyle.Angle = -45;
        area.AxisX.LabelStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
        area.AxisX.IsLabelAutoFit = false;
    }
    public static void ApplyReadablePointLabels(Series series)
    {
        if (series == null)
            return;
        series.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        series.LabelAngle = -45;
        series.SmartLabelStyle.Enabled = true;
        series.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
        series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom | LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
    }
    public static double ExponentialForecast(List<double> values, double alpha)
    {
        if (values == null || values.Count == 0)
            return 0.0;
        double smoothed = values[0];
        for (int i = 1; i < values.Count; i++)
            smoothed = alpha * values[i] + (1.0 - alpha) * smoothed;
        return smoothed;
    }
    public static List<double> ExponentialSmooth(List<double> values, double alpha)
    {
        List<double> result = new List<double>();
        if (values == null || values.Count == 0)
            return result;
        double smoothed = values[0];
        result.Add(smoothed);
        for (int i = 1; i < values.Count; i++)
        {
            smoothed = alpha * values[i] + (1.0 - alpha) * smoothed;
            result.Add(smoothed);
        }
        return result;
    }
    public static void AddConnectedForecast(
        Chart chart,
        string baseSeriesName,
        string forecastSeriesName,
        double x,
        double y,
        string label,
        bool isVisibleInLegend,
        Color fallbackColor)
    {
        Series baseSeries = chart.Series.FindByName(baseSeriesName);
        if (baseSeries == null || baseSeries.Points.Count == 0)
            return;
        DataPoint lastPoint = baseSeries.Points[baseSeries.Points.Count - 1];
        Color color = baseSeries.Color.IsEmpty ? fallbackColor : baseSeries.Color;
        Series connection = new Series("Соединение " + forecastSeriesName)
        {
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2,
            BorderDashStyle = ChartDashStyle.Dot,
            IsVisibleInLegend = false,
            Color = color,
            BorderColor = color
        };
        connection["SplineTension"] = "0.12";
        double lastX = lastPoint.XValue;
        double lastY = lastPoint.YValues[0];
        double middleX = (lastX + x) / 2.0;
        double middleY = (lastY + y) / 2.0;
        connection.Points.AddXY(lastX, lastY);
        connection.Points.AddXY(middleX, middleY);
        connection.Points.AddXY(x, y);
        chart.Series.Add(connection);
        Series forecast = new Series(forecastSeriesName)
        {
            ChartType = SeriesChartType.Point,
            MarkerStyle = MarkerStyle.Diamond,
            MarkerSize = 11,
            IsValueShownAsLabel = true,
            IsVisibleInLegend = isVisibleInLegend,
            Color = color,
            BorderColor = color,
            MarkerColor = color
        };
        ApplyReadablePointLabels(forecast);
        int p = forecast.Points.AddXY(x, y);
        forecast.Points[p].AxisLabel = FormatValue(x);
        forecast.Points[p].Label = label;
        chart.Series.Add(forecast);
    }
    public static void AddSplineDashedForecast(
        Chart chart,
        string baseSeriesName,
        string forecastSeriesName,
        double x,
        double y,
        string label,
        bool isVisibleInLegend,
        Color fallbackColor)
    {
        Series baseSeries = chart.Series.FindByName(baseSeriesName);
        if (baseSeries == null || baseSeries.Points.Count == 0)
            return;

        Color color = baseSeries.Color.IsEmpty ? fallbackColor : baseSeries.Color;

        baseSeries.ChartType = SeriesChartType.Spline;
        baseSeries.BorderDashStyle = ChartDashStyle.Solid;
        baseSeries["SplineTension"] = "0.12";

        int p = baseSeries.Points.AddXY(x, y);
        DataPoint forecastPoint = baseSeries.Points[p];
        forecastPoint.AxisLabel = "Прогноз";
        forecastPoint.Label = label;
        forecastPoint.MarkerStyle = MarkerStyle.Diamond;
        forecastPoint.MarkerSize = 11;
        forecastPoint.MarkerColor = color;
        forecastPoint.Color = color;
        forecastPoint.BorderColor = color;
        forecastPoint.LabelAngle = -45;
    }
    public static void ApplyYScale(ChartArea area, double minY, double maxY)
    {
        if (minY == double.MaxValue || maxY == double.MinValue)
        {
            area.AxisY.Minimum = double.NaN;
            area.AxisY.Maximum = double.NaN;
            return;
        }
        if (minY < maxY)
        {
            double dy = (maxY - minY) * 0.10;
            if (dy == 0) dy = Math.Max(Math.Abs(minY) * 0.001, 0.001);
            area.AxisY.Minimum = minY - dy;
            area.AxisY.Maximum = maxY + dy;
        }
        else
        {
            double dy = Math.Max(Math.Abs(minY) * 0.001, 0.001);
            area.AxisY.Minimum = minY - dy;
            area.AxisY.Maximum = maxY + dy;
        }
    }
    public static void PrepareTimeArea(ChartArea area, string yTitle, int itemsCount)
    {
        List<string> labels = new List<string>();
        for (int i = 0; i < itemsCount; i++)
            labels.Add(i.ToString(CultureInfo.InvariantCulture));
        PrepareTimeArea(area, yTitle, labels, false);
    }
    public static void PrepareTimeArea(ChartArea area, string yTitle, IList<string> cycleLabels, bool showForecastLabel)
    {
        if (area == null)
            return;
        int itemsCount = cycleLabels == null ? 0 : cycleLabels.Count;
        area.AxisX.Title = "Цикл наблюдения";
        area.AxisY.Title = yTitle;
        area.AxisX.Interval = 1;
        area.AxisX.Minimum = -0.5;
        area.AxisX.Maximum = Math.Max(0.5, showForecastLabel ? itemsCount + 0.8 : itemsCount - 0.5);
        area.AxisX.MajorGrid.Enabled = true;
        area.AxisY.MajorGrid.Enabled = true;
        area.AxisX.LabelStyle.Format = string.Empty;
        ApplyReadableXAxisLabels(area);
        area.AxisY.LabelStyle.Format = "0.#####";
        area.AxisX.CustomLabels.Clear();
        for (int i = 0; i < itemsCount; i++)
        {
            string label = cycleLabels[i];
            if (string.IsNullOrWhiteSpace(label))
                label = i.ToString(CultureInfo.InvariantCulture);
            area.AxisX.CustomLabels.Add(new CustomLabel(i - 0.5, i + 0.5, label, 0, LabelMarkStyle.None));
        }
        if (showForecastLabel)
            area.AxisX.CustomLabels.Add(new CustomLabel(itemsCount - 0.5, itemsCount + 0.5, "Прогноз", 0, LabelMarkStyle.None));
    }
    public static void ApplyNumericXLabels(ChartArea area, IEnumerable<double> xValues)
    {
        if (area == null)
            return;
        area.AxisX.CustomLabels.Clear();
        area.AxisX.LabelStyle.Enabled = true;
        area.AxisX.LabelStyle.Format = string.Empty;
        area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        ApplyReadableXAxisLabels(area);
        if (xValues == null)
            return;
        List<double> sorted = xValues
            .Where(value => !double.IsNaN(value) && !double.IsInfinity(value))
            .OrderBy(value => value)
            .ToList();
        if (sorted.Count == 0)
            return;
        List<double> distinct = new List<double>();
        foreach (double value in sorted)
        {
            if (distinct.Count == 0)
            {
                distinct.Add(value);
                continue;
            }
            double previous = distinct[distinct.Count - 1];
            double tolerance = Math.Max(Math.Max(Math.Abs(previous), Math.Abs(value)) * 0.00000001, 0.0000001);
            if (Math.Abs(value - previous) > tolerance)
                distinct.Add(value);
        }
        int step = Math.Max(1, (int)Math.Ceiling(distinct.Count / 35.0));
        List<double> visibleValues = new List<double>();
        for (int i = 0; i < distinct.Count; i++)
        {
            if (i % step == 0 || i == distinct.Count - 1)
                visibleValues.Add(distinct[i]);
        }
        double labelWidth = 0.001;
        if (visibleValues.Count > 1)
        {
            double minDiff = double.MaxValue;
            for (int i = 1; i < visibleValues.Count; i++)
                minDiff = Math.Min(minDiff, visibleValues[i] - visibleValues[i - 1]);
            if (minDiff > 0 && minDiff != double.MaxValue)
                labelWidth = minDiff * 0.45;
        }
        else
        {
            labelWidth = Math.Max(Math.Abs(visibleValues[0]) * 0.001, 0.001);
        }
        double axisMin = double.IsNaN(area.AxisX.Minimum) ? sorted.First() : area.AxisX.Minimum;
        double axisMax = double.IsNaN(area.AxisX.Maximum) ? sorted.Last() : area.AxisX.Maximum;
        double axisRange = Math.Abs(axisMax - axisMin);
        if (axisRange > 0)
            labelWidth = Math.Max(labelWidth, axisRange * 0.006);

        foreach (double value in visibleValues)
            area.AxisX.CustomLabels.Add(new CustomLabel(value - labelWidth, value + labelWidth, FormatValue(value), 0, LabelMarkStyle.None));
    }
}
#endregion

#region 11. Базовое окно графиков фазовой траектории и функций μ(t), α(t)

public abstract class PhaseChartsFormBase : Form
{
    private readonly string _windowTitle;
    private readonly List<PhaseStateItem> _items;
    private readonly double _forecastM;
    private readonly double _forecastMPlusE;
    private readonly double _forecastMMinusE;
    private readonly double _forecastA;
    private readonly double _forecastAPlusE;
    private readonly double _forecastAMinusE;
    private readonly double _alpha;
    private ComboBox comboBoxMode;
    private CheckBox checkBoxMain;
    private CheckBox checkBoxPlus;
    private CheckBox checkBoxMinus;
    private CheckBox checkBoxForecast;
    private Chart chart;
    protected PhaseChartsFormBase(
        string windowTitle,
        List<PhaseStateItem> items,
        double forecastM,
        double forecastMPlusE,
        double forecastMMinusE,
        double forecastA,
        double forecastAPlusE,
        double forecastAMinusE,
        double alpha)
    {
        _windowTitle = windowTitle;
        _items = items ?? new List<PhaseStateItem>();
        _forecastM = forecastM;
        _forecastMPlusE = forecastMPlusE;
        _forecastMMinusE = forecastMMinusE;
        _forecastA = forecastA;
        _forecastAPlusE = forecastAPlusE;
        _forecastAMinusE = forecastAMinusE;
        _alpha = alpha;
        InitializeFormSettings();
        InitializeUi();
        BindEvents();
        UpdateChart();
    }
    private void InitializeFormSettings()
    {
        Text = _windowTitle;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 650);
        Size = new Size(1150, 760);
    }
    private void InitializeUi()
    {
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        FlowLayoutPanel modePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        modePanel.Controls.Add(new Label
        {
            Text = "Тип графика:",
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 0)
        });
        comboBoxMode = new ComboBox
        {
            Width = 330,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        comboBoxMode.Items.Add("Фазовые траектории α(μ)");
        comboBoxMode.Items.Add("Функция μ(t)");
        comboBoxMode.Items.Add("Функция α(t), рад");
        comboBoxMode.Items.Add("Сравнение сглаживания μ(t)");
        comboBoxMode.Items.Add("Сравнение сглаживания α(t), рад");
        comboBoxMode.SelectedIndex = 0;
        modePanel.Controls.Add(comboBoxMode);
        FlowLayoutPanel optionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        checkBoxMain = new CheckBox { Text = "Основная траектория", Checked = true, AutoSize = true };
        checkBoxPlus = new CheckBox { Text = "+E", Checked = true, AutoSize = true };
        checkBoxMinus = new CheckBox { Text = "-E", Checked = true, AutoSize = true };
        checkBoxForecast = new CheckBox { Text = "Прогноз", Checked = true, AutoSize = true };
        optionsPanel.Controls.Add(checkBoxMain);
        optionsPanel.Controls.Add(checkBoxPlus);
        optionsPanel.Controls.Add(checkBoxMinus);
        optionsPanel.Controls.Add(checkBoxForecast);
        chart = new Chart { Dock = DockStyle.Fill };
        ChartArea area = new ChartArea("MainArea");
        area.AxisX.IsStartedFromZero = false;
        area.AxisY.IsStartedFromZero = false;
        area.AxisX.MajorGrid.Enabled = true;
        area.AxisY.MajorGrid.Enabled = true;
        chart.ChartAreas.Add(area);
        chart.Legends.Add(new Legend("MainLegend"));
        layout.Controls.Add(modePanel, 0, 0);
        layout.Controls.Add(optionsPanel, 0, 1);
        layout.Controls.Add(chart, 0, 2);
        Controls.Add(layout);
    }
    private void BindEvents()
    {
        comboBoxMode.SelectedIndexChanged += delegate { UpdateChart(); };
        checkBoxMain.CheckedChanged += delegate { UpdateChart(); };
        checkBoxPlus.CheckedChanged += delegate { UpdateChart(); };
        checkBoxMinus.CheckedChanged += delegate { UpdateChart(); };
        checkBoxForecast.CheckedChanged += delegate { UpdateChart(); };
    }
    private static double RadToArcSeconds(double radians)
    {
        return radians * 206264.80624709636;
    }
    private void UpdateChart()
    {
        chart.Series.Clear();
        if (comboBoxMode.SelectedIndex == 0)
            DrawPhaseTrajectoryChart();
        else if (comboBoxMode.SelectedIndex == 1)
            DrawTimeChart(false);
        else if (comboBoxMode.SelectedIndex == 2)
            DrawTimeChart(true);
        else if (comboBoxMode.SelectedIndex == 3)
            DrawSmoothingComparisonChart(false);
        else
            DrawSmoothingComparisonChart(true);
    }
    private List<string> GetCycleLabels()
    {
        List<string> labels = new List<string>();
        for (int i = 0; i < _items.Count; i++)
        {
            string label = _items[i].Label;
            if (string.IsNullOrWhiteSpace(label))
                label = i.ToString(CultureInfo.InvariantCulture);
            labels.Add(label);
        }
        return labels;
    }
    private void ConfigurePhaseSeries(Series series, Color color)
    {
        series.ChartType = SeriesChartType.Spline;
        series["SplineTension"] = "0.12";
        series.BorderWidth = 2;
        series.MarkerStyle = MarkerStyle.Circle;
        series.MarkerSize = 6;
        series.IsVisibleInLegend = true;
        series.IsValueShownAsLabel = true;
        series.Color = color;
        series.BorderColor = color;
        series.MarkerColor = color;
        ChartForecastHelper.ApplyReadablePointLabels(series);
    }
    private void DrawPhaseTrajectoryChart()
    {
        ChartArea area = chart.ChartAreas[0];
        area.AxisX.Title = "μ";
        area.AxisY.Title = "α, сек";
        area.AxisX.CustomLabels.Clear();
        area.AxisX.LabelStyle.Format = "0.###";
        area.AxisY.LabelStyle.Format = "0.#####";
        area.AxisX.LabelStyle.Angle = -45;
        area.AxisX.Interval = 0;
        area.AxisX.Minimum = double.NaN;
        area.AxisX.Maximum = double.NaN;
        area.AxisY.Minimum = double.NaN;
        area.AxisY.Maximum = double.NaN;
        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        if (checkBoxMain.Checked)
        {
            Series series = new Series("M vs A");
            ConfigurePhaseSeries(series, Color.RoyalBlue);
            AddPhasePoints(series, delegate (PhaseStateItem item) { return item.M; }, delegate (PhaseStateItem item) { return RadToArcSeconds(item.A); }, ref minX, ref maxX, ref minY, ref maxY);
            chart.Series.Add(series);
        }
        if (checkBoxPlus.Checked)
        {
            Series series = new Series("M+E vs A+E");
            ConfigurePhaseSeries(series, Color.Orange);
            AddPhasePoints(series, delegate (PhaseStateItem item) { return item.MPlusE; }, delegate (PhaseStateItem item) { return RadToArcSeconds(item.APlusE); }, ref minX, ref maxX, ref minY, ref maxY);
            chart.Series.Add(series);
        }
        if (checkBoxMinus.Checked)
        {
            Series series = new Series("M-E vs A-E");
            ConfigurePhaseSeries(series, Color.OrangeRed);
            AddPhasePoints(series, delegate (PhaseStateItem item) { return item.MMinusE; }, delegate (PhaseStateItem item) { return RadToArcSeconds(item.AMinusE); }, ref minX, ref maxX, ref minY, ref maxY);
            chart.Series.Add(series);
        }
        if (checkBoxForecast.Checked)
        {
            if (checkBoxMain.Checked)
            {
                double y = RadToArcSeconds(_forecastA);
                ChartForecastHelper.AddConnectedForecast(chart, "M vs A", "Прогноз M", _forecastM, y, "Прогноз", true, Color.RoyalBlue);
                AddBounds(_forecastM, y, ref minX, ref maxX, ref minY, ref maxY);
            }
            if (checkBoxPlus.Checked)
            {
                double y = RadToArcSeconds(_forecastAPlusE);
                ChartForecastHelper.AddConnectedForecast(chart, "M+E vs A+E", "Прогноз M+E", _forecastMPlusE, y, "Прогноз", true, Color.Orange);
                AddBounds(_forecastMPlusE, y, ref minX, ref maxX, ref minY, ref maxY);
            }
            if (checkBoxMinus.Checked)
            {
                double y = RadToArcSeconds(_forecastAMinusE);
                ChartForecastHelper.AddConnectedForecast(chart, "M-E vs A-E", "Прогноз M-E", _forecastMMinusE, y, "Прогноз", true, Color.OrangeRed);
                AddBounds(_forecastMMinusE, y, ref minX, ref maxX, ref minY, ref maxY);
            }
        }
        ApplyXYScale(area, minX, maxX, minY, maxY);
        ChartForecastHelper.ApplyNumericXLabels(area,
            chart.Series
                .Cast<Series>()
                .SelectMany(series => series.Points.Cast<DataPoint>())
                .Select(point => point.XValue));
    }
    private void AddPhasePoints(
        Series series,
        Func<PhaseStateItem, double> xSelector,
        Func<PhaseStateItem, double> ySelector,
        ref double minX,
        ref double maxX,
        ref double minY,
        ref double maxY)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            double x = xSelector(_items[i]);
            double y = ySelector(_items[i]);
            int p = series.Points.AddXY(x, y);
            series.Points[p].Label = _items[i].Label;
            series.Points[p].AxisLabel = ChartForecastHelper.FormatValue(x);
            AddBounds(x, y, ref minX, ref maxX, ref minY, ref maxY);
        }
    }
    private void DrawTimeChart(bool alphaChart)
    {
        ChartArea area = chart.ChartAreas[0];
        ChartForecastHelper.PrepareTimeArea(area, alphaChart ? "α, рад" : "μ", GetCycleLabels(), checkBoxForecast.Checked);
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        if (checkBoxMain.Checked)
            AddFunctionSeries(alphaChart ? "A(t)" : "M(t)", alphaChart, 0, Color.RoyalBlue, ref minY, ref maxY);
        if (checkBoxPlus.Checked)
            AddFunctionSeries(alphaChart ? "A+E(t)" : "M+E(t)", alphaChart, 1, Color.Orange, ref minY, ref maxY);
        if (checkBoxMinus.Checked)
            AddFunctionSeries(alphaChart ? "A-E(t)" : "M-E(t)", alphaChart, 2, Color.OrangeRed, ref minY, ref maxY);
        if (checkBoxForecast.Checked)
        {
            if (checkBoxMain.Checked)
            {
                double y = alphaChart ? _forecastA : _forecastM;
                ChartForecastHelper.AddSplineDashedForecast(chart, alphaChart ? "A(t)" : "M(t)", alphaChart ? "Прогноз A" : "Прогноз M", _items.Count, y, "Прогноз: " + ChartForecastHelper.FormatValue(y) + " (α=" + ChartForecastHelper.FormatAlpha(_alpha) + ")", true, Color.RoyalBlue);
                AddBoundsY(y, ref minY, ref maxY);
            }
            if (checkBoxPlus.Checked)
            {
                double y = alphaChart ? _forecastAPlusE : _forecastMPlusE;
                ChartForecastHelper.AddSplineDashedForecast(chart, alphaChart ? "A+E(t)" : "M+E(t)", alphaChart ? "Прогноз A+E" : "Прогноз M+E", _items.Count, y, ChartForecastHelper.FormatValue(y), true, Color.Orange);
                AddBoundsY(y, ref minY, ref maxY);
            }
            if (checkBoxMinus.Checked)
            {
                double y = alphaChart ? _forecastAMinusE : _forecastMMinusE;
                ChartForecastHelper.AddSplineDashedForecast(chart, alphaChart ? "A-E(t)" : "M-E(t)", alphaChart ? "Прогноз A-E" : "Прогноз M-E", _items.Count, y, ChartForecastHelper.FormatValue(y), true, Color.OrangeRed);
                AddBoundsY(y, ref minY, ref maxY);
            }
        }
        ChartForecastHelper.ApplyYScale(area, minY, maxY);
    }
    private void AddFunctionSeries(string name, bool alphaChart, int kind, Color color, ref double minY, ref double maxY)
    {
        Series series = new Series(name)
        {
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = 6,
            IsValueShownAsLabel = true,
            Color = color,
            BorderColor = color,
            MarkerColor = color
        };
        series["SplineTension"] = "0.12";
        ChartForecastHelper.ApplyReadablePointLabels(series);
        for (int i = 0; i < _items.Count; i++)
        {
            double value = GetValue(_items[i], alphaChart, kind);
            int p = series.Points.AddXY(i, value);
            series.Points[p].AxisLabel = _items[i].Label;
            series.Points[p].Label = ChartForecastHelper.FormatValue(value);
            AddBoundsY(value, ref minY, ref maxY);
        }
        chart.Series.Add(series);
    }
    private void DrawSmoothingComparisonChart(bool alphaChart)
    {
        ChartArea area = chart.ChartAreas[0];
        ChartForecastHelper.PrepareTimeArea(area, alphaChart ? "α, рад" : "μ", GetCycleLabels(), checkBoxForecast.Checked);
        List<double> values = new List<double>();
        for (int i = 0; i < _items.Count; i++)
            values.Add(GetValue(_items[i], alphaChart, 0));
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        if (checkBoxMain.Checked)
            AddSimpleComparisonLine(alphaChart ? "Исходная α" : "Исходная μ", values, Color.RoyalBlue, true, ref minY, ref maxY);
        double[] alphas = ChartForecastHelper.GetCompareAlphas();
        for (int a = 0; a < alphas.Length; a++)
        {
            double alpha = alphas[a];
            List<double> smoothed = ChartForecastHelper.ExponentialSmooth(values, alpha);
            Color color = ChartForecastHelper.GetCompareColor(a);
            string seriesName = "Сглаж. α=" + ChartForecastHelper.FormatAlpha(alpha);
            AddSimpleComparisonLine(seriesName, smoothed, color, true, ref minY, ref maxY);
            if (checkBoxForecast.Checked && smoothed.Count > 0)
            {
                double forecast = ChartForecastHelper.ExponentialForecast(values, alpha);
                ChartForecastHelper.AddSplineDashedForecast(chart, seriesName, "Прогноз α=" + ChartForecastHelper.FormatAlpha(alpha), _items.Count, forecast, "Прогноз", true, color);
                AddBoundsY(forecast, ref minY, ref maxY);
            }
        }
        ChartForecastHelper.ApplyYScale(area, minY, maxY);
    }
    private void AddSimpleComparisonLine(string name, List<double> values, Color color, bool showCycleLabels, ref double minY, ref double maxY)
    {
        Series series = new Series(name)
        {
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = 5,
            IsValueShownAsLabel = showCycleLabels && values.Count <= 25,
            Color = color,
            BorderColor = color,
            MarkerColor = color
        };
        series["SplineTension"] = "0.12";
        ChartForecastHelper.ApplyReadablePointLabels(series);
        for (int i = 0; i < values.Count; i++)
        {
            int p = series.Points.AddXY(i, values[i]);
            series.Points[p].AxisLabel = _items[i].Label;
            if (showCycleLabels && values.Count <= 25)
                series.Points[p].Label = _items[i].Label;
            AddBoundsY(values[i], ref minY, ref maxY);
        }
        chart.Series.Add(series);
    }
    private static double GetValue(PhaseStateItem item, bool alphaChart, int kind)
    {
        if (alphaChart)
        {
            if (kind == 1) return item.APlusE;
            if (kind == 2) return item.AMinusE;
            return item.A;
        }
        if (kind == 1) return item.MPlusE;
        if (kind == 2) return item.MMinusE;
        return item.M;
    }
    private static void AddBounds(double x, double y, ref double minX, ref double maxX, ref double minY, ref double maxY)
    {
        if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
            return;
        minX = Math.Min(minX, x);
        maxX = Math.Max(maxX, x);
        minY = Math.Min(minY, y);
        maxY = Math.Max(maxY, y);
    }
    private static void AddBoundsY(double y, ref double minY, ref double maxY)
    {
        if (double.IsNaN(y) || double.IsInfinity(y))
            return;
        minY = Math.Min(minY, y);
        maxY = Math.Max(maxY, y);
    }
    private static void ApplyXYScale(ChartArea area, double minX, double maxX, double minY, double maxY)
    {
        if (minX != double.MaxValue && maxX != double.MinValue)
        {
            double dx = maxX - minX;
            if (Math.Abs(dx) < 0.000001)
                dx = Math.Max(Math.Abs(minX) * 0.001, 0.001);
            area.AxisX.Minimum = minX - dx * 0.10;
            area.AxisX.Maximum = maxX + dx * 0.10;
        }
        ChartForecastHelper.ApplyYScale(area, minY, maxY);
    }
}
public class Level1ChartsForm : PhaseChartsFormBase
{
    public Level1ChartsForm(Level1AnalysisResult result)
        : base(
            "Графики I уровня декомпозиции",
            result == null ? null : result.Items,
            result == null ? 0.0 : result.ForecastM,
            result == null ? 0.0 : result.ForecastMPlusE,
            result == null ? 0.0 : result.ForecastMMinusE,
            result == null ? 0.0 : result.ForecastA,
            result == null ? 0.0 : result.ForecastAPlusE,
            result == null ? 0.0 : result.ForecastAMinusE,
            result == null ? 0.0 : result.Alpha)
    {
    }
}
public class Level2ChartsForm : PhaseChartsFormBase
{
    public Level2ChartsForm(Level2AnalysisResult result)
        : base(
            result == null ? "Графики II уровня" : "Графики II уровня — блок " + result.BlockName,
            result == null ? null : result.Items,
            result == null ? 0.0 : result.ForecastM,
            result == null ? 0.0 : result.ForecastMPlusE,
            result == null ? 0.0 : result.ForecastMMinusE,
            result == null ? 0.0 : result.ForecastA,
            result == null ? 0.0 : result.ForecastAPlusE,
            result == null ? 0.0 : result.ForecastAMinusE,
            result == null ? 0.0 : result.Alpha)
    {
    }
}
#endregion

#region 12. Окно графиков III уровня для структурных связей

public class Level3ChartsForm : Form
{
    private readonly Level3StructuralResult _result;
    private Chart chart;
    public Level3ChartsForm(Level3StructuralResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        InitializeFormSettings();
        InitializeUi();
        DrawLinksChart();
    }
    private void InitializeFormSettings()
    {
        Text = $"Графики III уровня — {_result.BlockName}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 750);
    }
    private void InitializeUi()
    {
        chart = new Chart
        {
            Dock = DockStyle.Fill
        };
        ChartArea area = new ChartArea("MainArea");
        area.AxisX.Title = "Индекс точки A";
        area.AxisY.Title = "Максимальное отклонение связи";
        area.AxisX.Interval = 1;
        area.AxisY.LabelStyle.Format = "0.000000";
        chart.ChartAreas.Add(area);
        chart.Legends.Add(new Legend("MainLegend"));
        Panel scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        chart.Width = 2200;
        chart.Height = 900;
        scrollPanel.Controls.Add(chart);
        Controls.Add(scrollPanel);
    }
    private void DrawLinksChart()
    {
        chart.Series.Clear();
        ChartArea area = chart.ChartAreas[0];
        area.AxisX.Title = "Связи между точками";
        area.AxisY.Title = "Максимальное отклонение";
        area.AxisX.Interval = 1;
        area.AxisX.ScaleView.Zoomable = true;
        area.CursorX.AutoScroll = true;
        area.CursorX.IsUserEnabled = true;
        area.CursorX.IsUserSelectionEnabled = true;
        area.AxisX.ScrollBar.Enabled = true;
        area.AxisX.ScrollBar.IsPositionedInside = true;
        area.AxisX.ScaleView.Size = 20;
        ChartForecastHelper.ApplyReadableXAxisLabels(area);
        area.AxisY.LabelStyle.Format = "0.####";
        List<RigidLink> linksToShow = _result.Links
            .Where(l => !l.IsRigid)
            .OrderByDescending(l => l.MaxDeviation)
            .ToList();
        if (linksToShow.Count == 0)
        {
            linksToShow = _result.Links
                .OrderByDescending(l => l.MaxDeviation)
                .Take(15)
                .ToList();
        }
        Series series = new Series("Отклонения связей")
        {
            ChartType = SeriesChartType.Column,
            IsValueShownAsLabel = true
        };
        series["PointWidth"] = "0.8";
        ChartForecastHelper.ApplyReadablePointLabels(series);
        int index = 1;
        foreach (RigidLink link in linksToShow)
        {
            int p = series.Points.AddXY(index, link.MaxDeviation);
            string label = $"{link.PointA}-{link.PointB}";
            series.Points[p].AxisLabel = label;
            series.Points[p].Label = FormatNumber(link.MaxDeviation);
            index++;
        }
        chart.Series.Add(series);
        area.RecalculateAxesScale();
    }
    private static string FormatNumber(double value)
    {
        if (Math.Abs(value - Math.Round(value)) < 0.0000001)
            return Math.Round(value).ToString(CultureInfo.InvariantCulture);
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
#endregion

#region 13. Окно графиков отклика III уровня

public class Level3ResponseChartsForm : PhaseChartsFormBase
{
    public Level3ResponseChartsForm(Level3ResponseResult result)
        : base(
            result == null ? "Графики III уровня" : "Графики III уровня — блок " + result.BlockName + ", часть " + result.ClusterId,
            result == null ? null : result.Items,
            result == null ? 0.0 : result.ForecastM,
            result == null ? 0.0 : result.ForecastMPlusE,
            result == null ? 0.0 : result.ForecastMMinusE,
            result == null ? 0.0 : result.ForecastA,
            result == null ? 0.0 : result.ForecastAPlusE,
            result == null ? 0.0 : result.ForecastAMinusE,
            result == null ? 0.0 : result.Alpha)
    {
    }
}
#endregion

#region 14. Окно графиков IV уровня Zk,i(t)

public class Level4ChartsForm : Form
{
    private readonly Level4AnalysisResult _result;
    private readonly HashSet<string> _selectedPointNames;
    private CheckBox checkBoxMain;
    private CheckBox checkBoxLimits;
    private CheckBox checkBoxForecast;
    private CheckBox checkBoxSmoothingComparison;
    private Chart chart;
    public Level4ChartsForm(Level4AnalysisResult result)
        : this(result, null)
    {
    }
    public Level4ChartsForm(Level4AnalysisResult result, IEnumerable<string> selectedPointNames)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        _selectedPointNames = new HashSet<string>(
            (selectedPointNames ?? Enumerable.Empty<string>())
                .Where(point => !string.IsNullOrWhiteSpace(point))
                .Select(point => point.Trim()),
            StringComparer.OrdinalIgnoreCase);
        InitializeFormSettings();
        InitializeUi();
        BindEvents();
        UpdateChart();
    }
    private void InitializeFormSettings()
    {
        Text = "Графики IV уровня — Zk,i(t)";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 650);
        Size = new Size(1150, 760);
    }
    private void InitializeUi()
    {
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        FlowLayoutPanel topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true
        };
        topPanel.Controls.Add(new Label
        {
            Text = "График функции Zk,i(t):",
            AutoSize = true,
            Margin = new Padding(0, 8, 16, 0),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        });
        checkBoxMain = new CheckBox { Text = "Исходные Z(t)", Checked = true, AutoSize = true, Margin = new Padding(0, 6, 12, 0) };
        checkBoxSmoothingComparison = new CheckBox { Text = "Сглаживание α=0.1; 0.4; 0.7; 0.9", Checked = true, AutoSize = true, Margin = new Padding(0, 6, 12, 0) };
        checkBoxLimits = new CheckBox { Text = "Предельные Z+E и Z-E", Checked = false, AutoSize = true, Margin = new Padding(0, 6, 12, 0) };
        checkBoxForecast = new CheckBox { Text = "Прогноз", Checked = true, AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
        topPanel.Controls.Add(checkBoxMain);
        topPanel.Controls.Add(checkBoxSmoothingComparison);
        topPanel.Controls.Add(checkBoxLimits);
        topPanel.Controls.Add(checkBoxForecast);
        chart = new Chart { Dock = DockStyle.Fill };
        ChartArea area = new ChartArea("MainArea");
        area.AxisX.Title = "Цикл наблюдения";
        area.AxisY.Title = "Значение точки Z";
        area.AxisX.Interval = 1;
        ChartForecastHelper.ApplyReadableXAxisLabels(area);
        area.AxisX.MajorGrid.Enabled = true;
        area.AxisY.MajorGrid.Enabled = true;
        chart.ChartAreas.Add(area);
        chart.Legends.Add(new Legend("MainLegend"));
        layout.Controls.Add(topPanel, 0, 0);
        layout.Controls.Add(chart, 0, 1);
        Controls.Add(layout);
    }
    private void BindEvents()
    {
        checkBoxMain.CheckedChanged += delegate { UpdateChart(); };
        checkBoxSmoothingComparison.CheckedChanged += delegate { UpdateChart(); };
        checkBoxLimits.CheckedChanged += delegate { UpdateChart(); };
        checkBoxForecast.CheckedChanged += delegate { UpdateChart(); };
    }
    private void UpdateChart()
    {
        chart.Series.Clear();
        DrawZTimeChart();
    }
    private List<string> GetVisiblePointNames()
    {

        List<string> sourceNames = _selectedPointNames.Count > 0
            ? _selectedPointNames.ToList()
            : _result.PointNames.ToList();

        return sourceNames
            .Where(pointName => !string.IsNullOrWhiteSpace(pointName))
            .Where(pointName => _result.Items.Any(item => string.Equals(item.PointName, pointName, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    private void DrawZTimeChart()
    {
        ChartArea area = chart.ChartAreas[0];
        List<string> pointNames = GetVisiblePointNames();
        HashSet<string> visiblePointSet = new HashSet<string>(pointNames, StringComparer.OrdinalIgnoreCase);
        List<string> cycleLabels = _result.Items
            .Where(item => visiblePointSet.Contains(item.PointName))
            .Select(x => x.CycleLabel)
            .Distinct()
            .ToList();
        ChartForecastHelper.PrepareTimeArea(area, "Значение точки Z", cycleLabels, checkBoxForecast.Checked);
        double minY = double.MaxValue;
        double maxY = double.MinValue;
        foreach (string pointName in pointNames)
        {
            List<Level4PointStateItem> items = _result.Items
                .Where(item => string.Equals(item.PointName, pointName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (items.Count == 0)
                continue;
            List<double> values = items.Select(item => item.Z).ToList();
            if (checkBoxMain.Checked)
            {
                string seriesName = "Исходные " + pointName;
                AddLevel4ValueSeries(seriesName, items, values, Color.Blue, true, ref minY, ref maxY);
                if (checkBoxForecast.Checked)
                {
                    Level4PointForecastItem forecast = _result.Forecasts.FirstOrDefault(item => string.Equals(item.PointName, pointName, StringComparison.OrdinalIgnoreCase));
                    if (forecast != null)
                    {
                        ChartForecastHelper.AddSplineDashedForecast(chart, seriesName, "Прогноз исходных " + pointName, items.Count, forecast.ForecastZ, "Прогноз", true, Color.Blue);
                        AddBoundsY(forecast.ForecastZ, ref minY, ref maxY);
                    }
                }
            }
            if (checkBoxSmoothingComparison.Checked)
            {
                double[] alphas = ChartForecastHelper.GetCompareAlphas();
                for (int i = 0; i < alphas.Length; i++)
                {
                    double alpha = alphas[i];
                    Color color = ChartForecastHelper.GetCompareColor(i);
                    List<double> smoothed = ChartForecastHelper.ExponentialSmooth(values, alpha);
                    string seriesName = "Сглаж. α=" + ChartForecastHelper.FormatAlpha(alpha) + ", " + pointName;
                    AddLevel4ValueSeries(seriesName, items, smoothed, color, true, ref minY, ref maxY);
                    if (checkBoxForecast.Checked)
                    {
                        double forecast = ChartForecastHelper.ExponentialForecast(values, alpha);
                        ChartForecastHelper.AddSplineDashedForecast(chart, seriesName, "Прогноз α=" + ChartForecastHelper.FormatAlpha(alpha) + ", " + pointName, items.Count, forecast, "Прогноз", true, color);
                        AddBoundsY(forecast, ref minY, ref maxY);
                    }
                }
            }
            if (checkBoxLimits.Checked)
            {
                AddLevel4LimitSeries("Z+E, " + pointName, items, delegate (Level4PointStateItem item) { return item.ZPlusE; }, Color.Gray, ref minY, ref maxY);
                AddLevel4LimitSeries("Z-E, " + pointName, items, delegate (Level4PointStateItem item) { return item.ZMinusE; }, Color.Gray, ref minY, ref maxY);
            }
        }
        ChartForecastHelper.ApplyYScale(area, minY, maxY);
    }
    private void AddLevel4ValueSeries(string name, List<Level4PointStateItem> items, List<double> values, Color color, bool showCycleLabels, ref double minY, ref double maxY)
    {
        Series series = new Series(name)
        {
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = 5,
            IsValueShownAsLabel = showCycleLabels && values.Count <= 25,
            Color = color,
            BorderColor = color,
            MarkerColor = color
        };
        series["SplineTension"] = "0.12";
        ChartForecastHelper.ApplyReadablePointLabels(series);
        for (int i = 0; i < values.Count; i++)
        {
            int p = series.Points.AddXY(i, values[i]);
            series.Points[p].AxisLabel = items[i].CycleLabel;
            if (showCycleLabels && values.Count <= 25)
                series.Points[p].Label = items[i].CycleLabel;
            AddBoundsY(values[i], ref minY, ref maxY);
        }
        chart.Series.Add(series);
    }
    private void AddLevel4LimitSeries(string name, List<Level4PointStateItem> items, Func<Level4PointStateItem, double> selector, Color color, ref double minY, ref double maxY)
    {
        Series series = new Series(name)
        {
            ChartType = SeriesChartType.Line,
            BorderWidth = 1,
            BorderDashStyle = ChartDashStyle.Dash,
            MarkerStyle = MarkerStyle.None,
            IsValueShownAsLabel = false,
            Color = color,
            BorderColor = color
        };
        for (int i = 0; i < items.Count; i++)
        {
            double value = selector(items[i]);
            int p = series.Points.AddXY(i, value);
            series.Points[p].AxisLabel = items[i].CycleLabel;
            AddBoundsY(value, ref minY, ref maxY);
        }
        chart.Series.Add(series);
    }
    private static void AddBoundsY(double y, ref double minY, ref double maxY)
    {
        if (double.IsNaN(y) || double.IsInfinity(y))
            return;
        minY = Math.Min(minY, y);
        maxY = Math.Max(maxY, y);
    }
}

#endregion
