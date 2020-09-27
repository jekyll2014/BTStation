using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace RFID_Station_control
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage_Station = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_resetStation = new System.Windows.Forms.Button();
            this.textBox_newStationNumber = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_checkedChips = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_lastCheck = new System.Windows.Forms.TextBox();
            this.comboBox_mode = new System.Windows.Forms.ComboBox();
            this.button_setMode = new System.Windows.Forms.Button();
            this.button_getLastErrors = new System.Windows.Forms.Button();
            this.button_getLastTeam = new System.Windows.Forms.Button();
            this.button_getConfig = new System.Windows.Forms.Button();
            this.button_getStatus = new System.Windows.Forms.Button();
            this.tabPage_Team = new System.Windows.Forms.TabPage();
            this.label23 = new System.Windows.Forms.Label();
            this.textBox_TeamNumber = new System.Windows.Forms.TextBox();
            this.button_getTeamsList = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.button_updTeamMask = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.button_getTeamRecord = new System.Windows.Forms.Button();
            this.textBox_teamMask = new System.Windows.Forms.TextBox();
            this.button_eraseTeamFlash = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_issueTime = new System.Windows.Forms.TextBox();
            this.tabPage_Rfid = new System.Windows.Forms.TabPage();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox_initMask = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_initTeamNum = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_uid = new System.Windows.Forms.TextBox();
            this.textBox_data = new System.Windows.Forms.TextBox();
            this.textBox_readChipPage = new System.Windows.Forms.TextBox();
            this.textBox_writeChipPage = new System.Windows.Forms.TextBox();
            this.button_readChipPage = new System.Windows.Forms.Button();
            this.button_eraseChip = new System.Windows.Forms.Button();
            this.button_writeChipPage = new System.Windows.Forms.Button();
            this.button_initChip = new System.Windows.Forms.Button();
            this.tabPage_Flash = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.textBox_flashData = new System.Windows.Forms.TextBox();
            this.textBox_readFlashLength = new System.Windows.Forms.TextBox();
            this.textBox_writeAddr = new System.Windows.Forms.TextBox();
            this.textBox_readFlashAddress = new System.Windows.Forms.TextBox();
            this.button_writeFlash = new System.Windows.Forms.Button();
            this.button_quickDump = new System.Windows.Forms.Button();
            this.button_readFlash = new System.Windows.Forms.Button();
            this.tabPage_Config = new System.Windows.Forms.TabPage();
            this.button_getConfig2 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox_sendBtCommand = new System.Windows.Forms.TextBox();
            this.button_sendBtCommand = new System.Windows.Forms.Button();
            this.button_SetBtName = new System.Windows.Forms.Button();
            this.textBox_BtPin = new System.Windows.Forms.TextBox();
            this.button_SetBtPin = new System.Windows.Forms.Button();
            this.textBox_BtName = new System.Windows.Forms.TextBox();
            this.textBox_eraseBlock = new System.Windows.Forms.TextBox();
            this.textBox_teamFlashSize = new System.Windows.Forms.TextBox();
            this.button_setEraseBlock = new System.Windows.Forms.Button();
            this.button_setTeamFlashSize = new System.Windows.Forms.Button();
            this.button_setChipType = new System.Windows.Forms.Button();
            this.button_setGain = new System.Windows.Forms.Button();
            this.comboBox_setGain = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.button_setAutoReport = new System.Windows.Forms.Button();
            this.button_setTime = new System.Windows.Forms.Button();
            this.checkBox_AutoReport = new System.Windows.Forms.CheckBox();
            this.textBox_setTime = new System.Windows.Forms.TextBox();
            this.checkBox_autoTime = new System.Windows.Forms.CheckBox();
            this.comboBox_flashSize = new System.Windows.Forms.ComboBox();
            this.textBox_packetLength = new System.Windows.Forms.TextBox();
            this.comboBox_chipType = new System.Windows.Forms.ComboBox();
            this.textBox_flashSize = new System.Windows.Forms.TextBox();
            this.button_setBatteryLimit = new System.Windows.Forms.Button();
            this.textBox_setBatteryLimit = new System.Windows.Forms.TextBox();
            this.button_setKoeff = new System.Windows.Forms.Button();
            this.textBox_koeff = new System.Windows.Forms.TextBox();
            this.button_dumpFlash = new System.Windows.Forms.Button();
            this.button_dumpChip = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_stationNumber = new System.Windows.Forms.TextBox();
            this.textBox_terminal = new System.Windows.Forms.TextBox();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.comboBox_portName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_openPort = new System.Windows.Forms.Button();
            this.button_clearLog = new System.Windows.Forms.Button();
            this.button_closePort = new System.Windows.Forms.Button();
            this.button_saveLog = new System.Windows.Forms.Button();
            this.checkBox_autoScroll = new System.Windows.Forms.CheckBox();
            this.button_refresh = new System.Windows.Forms.Button();
            this.checkBox_portMon = new System.Windows.Forms.CheckBox();
            this.dataGridView_teams = new System.Windows.Forms.DataGridView();
            this.tabControl_teamData = new System.Windows.Forms.TabControl();
            this.tabPage_terminal = new System.Windows.Forms.TabPage();
            this.tabPage_teams = new System.Windows.Forms.TabPage();
            this.button_dumpTeams = new System.Windows.Forms.Button();
            this.button_clearTeams = new System.Windows.Forms.Button();
            this.button_saveTeams = new System.Windows.Forms.Button();
            this.tabPage_cardContent = new System.Windows.Forms.TabPage();
            this.button_clearRfid = new System.Windows.Forms.Button();
            this.button_loadRfid = new System.Windows.Forms.Button();
            this.button_saveRfid = new System.Windows.Forms.Button();
            this.dataGridView_chipRawData = new System.Windows.Forms.DataGridView();
            this.tabPage_flashContent = new System.Windows.Forms.TabPage();
            this.button_clearFlash = new System.Windows.Forms.Button();
            this.button_loadFlash = new System.Windows.Forms.Button();
            this.button_saveFlash = new System.Windows.Forms.Button();
            this.dataGridView_flashRawData = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label22 = new System.Windows.Forms.Label();
            this.textBox_fwVersion = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabControl.SuspendLayout();
            this.tabPage_Station.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_Team.SuspendLayout();
            this.tabPage_Rfid.SuspendLayout();
            this.tabPage_Flash.SuspendLayout();
            this.tabPage_Config.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_teams)).BeginInit();
            this.tabControl_teamData.SuspendLayout();
            this.tabPage_terminal.SuspendLayout();
            this.tabPage_teams.SuspendLayout();
            this.tabPage_cardContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_chipRawData)).BeginInit();
            this.tabPage_flashContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_flashRawData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPage_Station);
            this.tabControl.Controls.Add(this.tabPage_Team);
            this.tabControl.Controls.Add(this.tabPage_Rfid);
            this.tabControl.Controls.Add(this.tabPage_Flash);
            this.tabControl.Controls.Add(this.tabPage_Config);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabControl.Location = new System.Drawing.Point(-2, 37);
            this.tabControl.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(461, 309);
            this.tabControl.TabIndex = 0;
            // 
            // tabPage_Station
            // 
            this.tabPage_Station.AutoScroll = true;
            this.tabPage_Station.Controls.Add(this.groupBox1);
            this.tabPage_Station.Controls.Add(this.comboBox_mode);
            this.tabPage_Station.Controls.Add(this.button_setMode);
            this.tabPage_Station.Controls.Add(this.button_getLastErrors);
            this.tabPage_Station.Controls.Add(this.button_getLastTeam);
            this.tabPage_Station.Controls.Add(this.button_getConfig);
            this.tabPage_Station.Controls.Add(this.button_getStatus);
            this.tabPage_Station.Location = new System.Drawing.Point(4, 33);
            this.tabPage_Station.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage_Station.Name = "tabPage_Station";
            this.tabPage_Station.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_Station.Size = new System.Drawing.Size(453, 272);
            this.tabPage_Station.TabIndex = 0;
            this.tabPage_Station.Text = "Station";
            this.tabPage_Station.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_resetStation);
            this.groupBox1.Controls.Add(this.textBox_newStationNumber);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox_checkedChips);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox_lastCheck);
            this.groupBox1.Location = new System.Drawing.Point(9, 117);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(435, 145);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reset station";
            // 
            // button_resetStation
            // 
            this.button_resetStation.BackColor = System.Drawing.Color.Red;
            this.button_resetStation.Enabled = false;
            this.button_resetStation.Location = new System.Drawing.Point(9, 31);
            this.button_resetStation.Margin = new System.Windows.Forms.Padding(6);
            this.button_resetStation.Name = "button_resetStation";
            this.button_resetStation.Size = new System.Drawing.Size(91, 97);
            this.button_resetStation.TabIndex = 5;
            this.button_resetStation.Text = "Reset station";
            this.button_resetStation.UseVisualStyleBackColor = false;
            this.button_resetStation.Click += new System.EventHandler(this.Button_resetStation_Click);
            // 
            // textBox_newStationNumber
            // 
            this.textBox_newStationNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_newStationNumber.Location = new System.Drawing.Point(229, 31);
            this.textBox_newStationNumber.MaxLength = 3;
            this.textBox_newStationNumber.Name = "textBox_newStationNumber";
            this.textBox_newStationNumber.Size = new System.Drawing.Size(200, 29);
            this.textBox_newStationNumber.TabIndex = 10;
            this.textBox_newStationNumber.Text = "0";
            this.textBox_newStationNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_newStationNumber.Leave += new System.EventHandler(this.TextBox_newStationNumber_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(109, 104);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 24);
            this.label5.TabIndex = 10;
            this.label5.Text = "last check";
            // 
            // textBox_checkedChips
            // 
            this.textBox_checkedChips.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_checkedChips.Location = new System.Drawing.Point(258, 66);
            this.textBox_checkedChips.MaxLength = 4;
            this.textBox_checkedChips.Name = "textBox_checkedChips";
            this.textBox_checkedChips.Size = new System.Drawing.Size(171, 29);
            this.textBox_checkedChips.TabIndex = 10;
            this.textBox_checkedChips.Text = "0";
            this.textBox_checkedChips.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_checkedChips.Leave += new System.EventHandler(this.TextBox_checkedChips_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 24);
            this.label4.TabIndex = 10;
            this.label4.Text = "chips checked#";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(109, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "new station#";
            // 
            // textBox_lastCheck
            // 
            this.textBox_lastCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_lastCheck.Location = new System.Drawing.Point(208, 101);
            this.textBox_lastCheck.MaxLength = 20;
            this.textBox_lastCheck.Name = "textBox_lastCheck";
            this.textBox_lastCheck.Size = new System.Drawing.Size(221, 29);
            this.textBox_lastCheck.TabIndex = 10;
            this.textBox_lastCheck.Text = "1970.01.01 00:00:00";
            this.textBox_lastCheck.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_lastCheck.Leave += new System.EventHandler(this.TextBox_lastCheck_Leave);
            // 
            // comboBox_mode
            // 
            this.comboBox_mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_mode.FormattingEnabled = true;
            this.comboBox_mode.Location = new System.Drawing.Point(119, 72);
            this.comboBox_mode.Name = "comboBox_mode";
            this.comboBox_mode.Size = new System.Drawing.Size(164, 32);
            this.comboBox_mode.TabIndex = 8;
            // 
            // button_setMode
            // 
            this.button_setMode.Enabled = false;
            this.button_setMode.Location = new System.Drawing.Point(6, 66);
            this.button_setMode.Margin = new System.Windows.Forms.Padding(6);
            this.button_setMode.Name = "button_setMode";
            this.button_setMode.Size = new System.Drawing.Size(101, 42);
            this.button_setMode.TabIndex = 0;
            this.button_setMode.Text = "Set mode";
            this.button_setMode.UseVisualStyleBackColor = true;
            this.button_setMode.Click += new System.EventHandler(this.Button_setMode_Click);
            // 
            // button_getLastErrors
            // 
            this.button_getLastErrors.Enabled = false;
            this.button_getLastErrors.Location = new System.Drawing.Point(292, 66);
            this.button_getLastErrors.Margin = new System.Windows.Forms.Padding(6);
            this.button_getLastErrors.Name = "button_getLastErrors";
            this.button_getLastErrors.Size = new System.Drawing.Size(152, 42);
            this.button_getLastErrors.TabIndex = 11;
            this.button_getLastErrors.Text = "Get last errors";
            this.button_getLastErrors.UseVisualStyleBackColor = true;
            this.button_getLastErrors.Click += new System.EventHandler(this.Button_getLastErrors_Click);
            // 
            // button_getLastTeam
            // 
            this.button_getLastTeam.Enabled = false;
            this.button_getLastTeam.Location = new System.Drawing.Point(292, 12);
            this.button_getLastTeam.Margin = new System.Windows.Forms.Padding(6);
            this.button_getLastTeam.Name = "button_getLastTeam";
            this.button_getLastTeam.Size = new System.Drawing.Size(152, 42);
            this.button_getLastTeam.TabIndex = 11;
            this.button_getLastTeam.Text = "Get last teams";
            this.button_getLastTeam.UseVisualStyleBackColor = true;
            this.button_getLastTeam.Click += new System.EventHandler(this.Button_getLastTeam_Click);
            // 
            // button_getConfig
            // 
            this.button_getConfig.Enabled = false;
            this.button_getConfig.Location = new System.Drawing.Point(133, 12);
            this.button_getConfig.Margin = new System.Windows.Forms.Padding(6);
            this.button_getConfig.Name = "button_getConfig";
            this.button_getConfig.Size = new System.Drawing.Size(147, 42);
            this.button_getConfig.TabIndex = 0;
            this.button_getConfig.Text = "Get config";
            this.button_getConfig.UseVisualStyleBackColor = true;
            this.button_getConfig.Click += new System.EventHandler(this.Button_getConfig_Click);
            // 
            // button_getStatus
            // 
            this.button_getStatus.Enabled = false;
            this.button_getStatus.Location = new System.Drawing.Point(6, 12);
            this.button_getStatus.Margin = new System.Windows.Forms.Padding(6);
            this.button_getStatus.Name = "button_getStatus";
            this.button_getStatus.Size = new System.Drawing.Size(115, 42);
            this.button_getStatus.TabIndex = 0;
            this.button_getStatus.Text = "Get status";
            this.button_getStatus.UseVisualStyleBackColor = true;
            this.button_getStatus.Click += new System.EventHandler(this.Button_getStatus_Click);
            // 
            // tabPage_Team
            // 
            this.tabPage_Team.AutoScroll = true;
            this.tabPage_Team.Controls.Add(this.label23);
            this.tabPage_Team.Controls.Add(this.textBox_TeamNumber);
            this.tabPage_Team.Controls.Add(this.button_getTeamsList);
            this.tabPage_Team.Controls.Add(this.label19);
            this.tabPage_Team.Controls.Add(this.label17);
            this.tabPage_Team.Controls.Add(this.button_updTeamMask);
            this.tabPage_Team.Controls.Add(this.label7);
            this.tabPage_Team.Controls.Add(this.button_getTeamRecord);
            this.tabPage_Team.Controls.Add(this.textBox_teamMask);
            this.tabPage_Team.Controls.Add(this.button_eraseTeamFlash);
            this.tabPage_Team.Controls.Add(this.label8);
            this.tabPage_Team.Controls.Add(this.textBox_issueTime);
            this.tabPage_Team.Location = new System.Drawing.Point(4, 33);
            this.tabPage_Team.Name = "tabPage_Team";
            this.tabPage_Team.Size = new System.Drawing.Size(453, 272);
            this.tabPage_Team.TabIndex = 4;
            this.tabPage_Team.Text = "Teams";
            this.tabPage_Team.UseVisualStyleBackColor = true;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(198, 15);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(53, 24);
            this.label23.TabIndex = 29;
            this.label23.Text = "start#";
            // 
            // textBox_TeamNumber
            // 
            this.textBox_TeamNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_TeamNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_TeamNumber.Location = new System.Drawing.Point(268, 22);
            this.textBox_TeamNumber.MaxLength = 4;
            this.textBox_TeamNumber.Name = "textBox_TeamNumber";
            this.textBox_TeamNumber.Size = new System.Drawing.Size(180, 80);
            this.textBox_TeamNumber.TabIndex = 28;
            this.textBox_TeamNumber.Text = "1";
            this.textBox_TeamNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox_TeamNumber.Leave += new System.EventHandler(this.TextBox_getTeamsList_Leave);
            // 
            // button_getTeamsList
            // 
            this.button_getTeamsList.Enabled = false;
            this.button_getTeamsList.Location = new System.Drawing.Point(6, 6);
            this.button_getTeamsList.Margin = new System.Windows.Forms.Padding(6);
            this.button_getTeamsList.Name = "button_getTeamsList";
            this.button_getTeamsList.Size = new System.Drawing.Size(183, 42);
            this.button_getTeamsList.TabIndex = 27;
            this.button_getTeamsList.Text = "Get teams list";
            this.button_getTeamsList.UseVisualStyleBackColor = true;
            this.button_getTeamsList.Click += new System.EventHandler(this.Button_getTeamsList_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(198, 69);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(61, 24);
            this.label19.TabIndex = 26;
            this.label19.Text = "team#";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(198, 114);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(61, 24);
            this.label17.TabIndex = 25;
            this.label17.Text = "team#";
            // 
            // button_updTeamMask
            // 
            this.button_updTeamMask.Enabled = false;
            this.button_updTeamMask.Location = new System.Drawing.Point(6, 159);
            this.button_updTeamMask.Margin = new System.Windows.Forms.Padding(6);
            this.button_updTeamMask.Name = "button_updTeamMask";
            this.button_updTeamMask.Size = new System.Drawing.Size(183, 76);
            this.button_updTeamMask.TabIndex = 0;
            this.button_updTeamMask.Text = "Update team mask";
            this.button_updTeamMask.UseVisualStyleBackColor = true;
            this.button_updTeamMask.Click += new System.EventHandler(this.Button_updateTeamMask_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(198, 209);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 24);
            this.label7.TabIndex = 11;
            this.label7.Text = "mask";
            // 
            // button_getTeamRecord
            // 
            this.button_getTeamRecord.Enabled = false;
            this.button_getTeamRecord.Location = new System.Drawing.Point(6, 60);
            this.button_getTeamRecord.Margin = new System.Windows.Forms.Padding(6);
            this.button_getTeamRecord.Name = "button_getTeamRecord";
            this.button_getTeamRecord.Size = new System.Drawing.Size(183, 42);
            this.button_getTeamRecord.TabIndex = 0;
            this.button_getTeamRecord.Text = "Get team record";
            this.button_getTeamRecord.UseVisualStyleBackColor = true;
            this.button_getTeamRecord.Click += new System.EventHandler(this.Button_getTeamRecord_Click);
            // 
            // textBox_teamMask
            // 
            this.textBox_teamMask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_teamMask.Location = new System.Drawing.Point(268, 206);
            this.textBox_teamMask.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_teamMask.MaxLength = 16;
            this.textBox_teamMask.Name = "textBox_teamMask";
            this.textBox_teamMask.Size = new System.Drawing.Size(180, 29);
            this.textBox_teamMask.TabIndex = 2;
            this.textBox_teamMask.Tag = "0";
            this.textBox_teamMask.Text = "0000000000000000";
            this.textBox_teamMask.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_teamMask.Leave += new System.EventHandler(this.TextBox_teamMask_Leave);
            // 
            // button_eraseTeamFlash
            // 
            this.button_eraseTeamFlash.BackColor = System.Drawing.Color.Red;
            this.button_eraseTeamFlash.Enabled = false;
            this.button_eraseTeamFlash.Location = new System.Drawing.Point(6, 105);
            this.button_eraseTeamFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_eraseTeamFlash.Name = "button_eraseTeamFlash";
            this.button_eraseTeamFlash.Size = new System.Drawing.Size(183, 42);
            this.button_eraseTeamFlash.TabIndex = 3;
            this.button_eraseTeamFlash.Text = "Erase team";
            this.button_eraseTeamFlash.UseVisualStyleBackColor = false;
            this.button_eraseTeamFlash.Click += new System.EventHandler(this.Button_eraseTeamFlash_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(198, 168);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 24);
            this.label8.TabIndex = 12;
            this.label8.Text = "issued";
            // 
            // textBox_issueTime
            // 
            this.textBox_issueTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_issueTime.Location = new System.Drawing.Point(268, 165);
            this.textBox_issueTime.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_issueTime.MaxLength = 20;
            this.textBox_issueTime.Name = "textBox_issueTime";
            this.textBox_issueTime.Size = new System.Drawing.Size(180, 29);
            this.textBox_issueTime.TabIndex = 3;
            this.textBox_issueTime.Text = "2000.01.01 00:00:00";
            this.textBox_issueTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_issueTime.Leave += new System.EventHandler(this.TextBox_issueTime_Leave);
            // 
            // tabPage_Rfid
            // 
            this.tabPage_Rfid.AutoScroll = true;
            this.tabPage_Rfid.Controls.Add(this.label16);
            this.tabPage_Rfid.Controls.Add(this.textBox_initMask);
            this.tabPage_Rfid.Controls.Add(this.label6);
            this.tabPage_Rfid.Controls.Add(this.textBox_initTeamNum);
            this.tabPage_Rfid.Controls.Add(this.label13);
            this.tabPage_Rfid.Controls.Add(this.label12);
            this.tabPage_Rfid.Controls.Add(this.label11);
            this.tabPage_Rfid.Controls.Add(this.label10);
            this.tabPage_Rfid.Controls.Add(this.textBox_uid);
            this.tabPage_Rfid.Controls.Add(this.textBox_data);
            this.tabPage_Rfid.Controls.Add(this.textBox_readChipPage);
            this.tabPage_Rfid.Controls.Add(this.textBox_writeChipPage);
            this.tabPage_Rfid.Controls.Add(this.button_readChipPage);
            this.tabPage_Rfid.Controls.Add(this.button_eraseChip);
            this.tabPage_Rfid.Controls.Add(this.button_writeChipPage);
            this.tabPage_Rfid.Controls.Add(this.button_initChip);
            this.tabPage_Rfid.Location = new System.Drawing.Point(4, 33);
            this.tabPage_Rfid.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage_Rfid.Name = "tabPage_Rfid";
            this.tabPage_Rfid.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_Rfid.Size = new System.Drawing.Size(453, 272);
            this.tabPage_Rfid.TabIndex = 1;
            this.tabPage_Rfid.Text = "RFID";
            this.tabPage_Rfid.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(135, 59);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(54, 24);
            this.label16.TabIndex = 31;
            this.label16.Text = "mask";
            // 
            // textBox_initMask
            // 
            this.textBox_initMask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_initMask.Location = new System.Drawing.Point(216, 56);
            this.textBox_initMask.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_initMask.MaxLength = 16;
            this.textBox_initMask.Name = "textBox_initMask";
            this.textBox_initMask.Size = new System.Drawing.Size(232, 29);
            this.textBox_initMask.TabIndex = 30;
            this.textBox_initMask.Tag = "0";
            this.textBox_initMask.Text = "0000000000000000";
            this.textBox_initMask.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_initMask.Leave += new System.EventHandler(this.TextBox_initMask_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(135, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 24);
            this.label6.TabIndex = 29;
            this.label6.Text = "team#";
            // 
            // textBox_initTeamNum
            // 
            this.textBox_initTeamNum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_initTeamNum.Location = new System.Drawing.Point(216, 18);
            this.textBox_initTeamNum.MaxLength = 4;
            this.textBox_initTeamNum.Name = "textBox_initTeamNum";
            this.textBox_initTeamNum.Size = new System.Drawing.Size(232, 29);
            this.textBox_initTeamNum.TabIndex = 28;
            this.textBox_initTeamNum.Text = "0";
            this.textBox_initTeamNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_initTeamNum.Leave += new System.EventHandler(this.TextBox_initTeamNum_Leave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(135, 106);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(72, 24);
            this.label13.TabIndex = 27;
            this.label13.Text = "pages#";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(266, 160);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 24);
            this.label12.TabIndex = 26;
            this.label12.Text = "data";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(135, 214);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 24);
            this.label11.TabIndex = 25;
            this.label11.Text = "UID";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(135, 160);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 24);
            this.label10.TabIndex = 24;
            this.label10.Text = "page#";
            // 
            // textBox_uid
            // 
            this.textBox_uid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_uid.Location = new System.Drawing.Point(216, 211);
            this.textBox_uid.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_uid.MaxLength = 24;
            this.textBox_uid.Name = "textBox_uid";
            this.textBox_uid.Size = new System.Drawing.Size(232, 29);
            this.textBox_uid.TabIndex = 23;
            this.textBox_uid.Tag = "0";
            this.textBox_uid.Text = "00 00 00 00 00 00 00 00 ";
            this.textBox_uid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_uid.Leave += new System.EventHandler(this.TextBox_uid_Leave);
            // 
            // textBox_data
            // 
            this.textBox_data.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_data.Location = new System.Drawing.Point(320, 157);
            this.textBox_data.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_data.MaxLength = 12;
            this.textBox_data.Name = "textBox_data";
            this.textBox_data.Size = new System.Drawing.Size(128, 29);
            this.textBox_data.TabIndex = 22;
            this.textBox_data.Tag = "0";
            this.textBox_data.Text = "00 00 00 00";
            this.textBox_data.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_data.Leave += new System.EventHandler(this.TextBox_data_Leave);
            // 
            // textBox_readChipPage
            // 
            this.textBox_readChipPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_readChipPage.Location = new System.Drawing.Point(216, 103);
            this.textBox_readChipPage.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_readChipPage.MaxLength = 7;
            this.textBox_readChipPage.Name = "textBox_readChipPage";
            this.textBox_readChipPage.Size = new System.Drawing.Size(232, 29);
            this.textBox_readChipPage.TabIndex = 20;
            this.textBox_readChipPage.Tag = "0";
            this.textBox_readChipPage.Text = "0-40";
            this.textBox_readChipPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_readChipPage.Leave += new System.EventHandler(this.TextBox_readChipPage_Leave);
            // 
            // textBox_writeChipPage
            // 
            this.textBox_writeChipPage.Location = new System.Drawing.Point(216, 157);
            this.textBox_writeChipPage.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_writeChipPage.MaxLength = 3;
            this.textBox_writeChipPage.Name = "textBox_writeChipPage";
            this.textBox_writeChipPage.Size = new System.Drawing.Size(45, 29);
            this.textBox_writeChipPage.TabIndex = 21;
            this.textBox_writeChipPage.Tag = "0";
            this.textBox_writeChipPage.Text = "0";
            this.textBox_writeChipPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_writeChipPage.Leave += new System.EventHandler(this.TextBox_writeChipPage_Leave);
            // 
            // button_readChipPage
            // 
            this.button_readChipPage.Enabled = false;
            this.button_readChipPage.Location = new System.Drawing.Point(6, 97);
            this.button_readChipPage.Margin = new System.Windows.Forms.Padding(6);
            this.button_readChipPage.Name = "button_readChipPage";
            this.button_readChipPage.Size = new System.Drawing.Size(120, 42);
            this.button_readChipPage.TabIndex = 18;
            this.button_readChipPage.Text = "Read chip";
            this.button_readChipPage.UseVisualStyleBackColor = true;
            this.button_readChipPage.Click += new System.EventHandler(this.Button_readCardPage_Click);
            // 
            // button_eraseChip
            // 
            this.button_eraseChip.BackColor = System.Drawing.Color.Red;
            this.button_eraseChip.Enabled = false;
            this.button_eraseChip.Location = new System.Drawing.Point(6, 205);
            this.button_eraseChip.Margin = new System.Windows.Forms.Padding(6);
            this.button_eraseChip.Name = "button_eraseChip";
            this.button_eraseChip.Size = new System.Drawing.Size(120, 42);
            this.button_eraseChip.TabIndex = 19;
            this.button_eraseChip.Text = "Erase chip";
            this.button_eraseChip.UseVisualStyleBackColor = false;
            this.button_eraseChip.Click += new System.EventHandler(this.Button_eraseChip_Click);
            // 
            // button_writeChipPage
            // 
            this.button_writeChipPage.Enabled = false;
            this.button_writeChipPage.Location = new System.Drawing.Point(6, 151);
            this.button_writeChipPage.Margin = new System.Windows.Forms.Padding(6);
            this.button_writeChipPage.Name = "button_writeChipPage";
            this.button_writeChipPage.Size = new System.Drawing.Size(120, 42);
            this.button_writeChipPage.TabIndex = 19;
            this.button_writeChipPage.Text = "Write chip";
            this.button_writeChipPage.UseVisualStyleBackColor = true;
            this.button_writeChipPage.Click += new System.EventHandler(this.Button_writeCardPage_Click);
            // 
            // button_initChip
            // 
            this.button_initChip.Enabled = false;
            this.button_initChip.Location = new System.Drawing.Point(6, 12);
            this.button_initChip.Margin = new System.Windows.Forms.Padding(6);
            this.button_initChip.Name = "button_initChip";
            this.button_initChip.Size = new System.Drawing.Size(120, 73);
            this.button_initChip.TabIndex = 0;
            this.button_initChip.Text = "Init chip";
            this.button_initChip.UseVisualStyleBackColor = true;
            this.button_initChip.Click += new System.EventHandler(this.Button_initChip_Click);
            // 
            // tabPage_Flash
            // 
            this.tabPage_Flash.AutoScroll = true;
            this.tabPage_Flash.Controls.Add(this.label9);
            this.tabPage_Flash.Controls.Add(this.label21);
            this.tabPage_Flash.Controls.Add(this.label15);
            this.tabPage_Flash.Controls.Add(this.label14);
            this.tabPage_Flash.Controls.Add(this.textBox_flashData);
            this.tabPage_Flash.Controls.Add(this.textBox_readFlashLength);
            this.tabPage_Flash.Controls.Add(this.textBox_writeAddr);
            this.tabPage_Flash.Controls.Add(this.textBox_readFlashAddress);
            this.tabPage_Flash.Controls.Add(this.button_writeFlash);
            this.tabPage_Flash.Controls.Add(this.button_quickDump);
            this.tabPage_Flash.Controls.Add(this.button_readFlash);
            this.tabPage_Flash.Location = new System.Drawing.Point(4, 33);
            this.tabPage_Flash.Name = "tabPage_Flash";
            this.tabPage_Flash.Size = new System.Drawing.Size(453, 272);
            this.tabPage_Flash.TabIndex = 2;
            this.tabPage_Flash.Text = "Flash";
            this.tabPage_Flash.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(133, 106);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 24);
            this.label9.TabIndex = 23;
            this.label9.Text = "data";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(314, 15);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(62, 24);
            this.label21.TabIndex = 22;
            this.label21.Text = "length";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(133, 15);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(43, 24);
            this.label15.TabIndex = 22;
            this.label15.Text = "start";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(133, 67);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 24);
            this.label14.TabIndex = 22;
            this.label14.Text = "address";
            // 
            // textBox_flashData
            // 
            this.textBox_flashData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_flashData.Location = new System.Drawing.Point(219, 103);
            this.textBox_flashData.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_flashData.MaxLength = 8096;
            this.textBox_flashData.Multiline = true;
            this.textBox_flashData.Name = "textBox_flashData";
            this.textBox_flashData.Size = new System.Drawing.Size(229, 163);
            this.textBox_flashData.TabIndex = 21;
            this.textBox_flashData.Tag = "0";
            this.textBox_flashData.Text = "00 00 00 00";
            this.textBox_flashData.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_flashData.Leave += new System.EventHandler(this.TextBox_flashData_Leave);
            // 
            // textBox_readFlashLength
            // 
            this.textBox_readFlashLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_readFlashLength.Location = new System.Drawing.Point(387, 12);
            this.textBox_readFlashLength.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_readFlashLength.MaxLength = 5;
            this.textBox_readFlashLength.Name = "textBox_readFlashLength";
            this.textBox_readFlashLength.Size = new System.Drawing.Size(61, 29);
            this.textBox_readFlashLength.TabIndex = 19;
            this.textBox_readFlashLength.Tag = "0";
            this.textBox_readFlashLength.Text = "230";
            this.textBox_readFlashLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_readFlashLength.Leave += new System.EventHandler(this.TextBox_readFlashLength_Leave);
            // 
            // textBox_writeAddr
            // 
            this.textBox_writeAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_writeAddr.Location = new System.Drawing.Point(219, 64);
            this.textBox_writeAddr.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_writeAddr.MaxLength = 10;
            this.textBox_writeAddr.Name = "textBox_writeAddr";
            this.textBox_writeAddr.Size = new System.Drawing.Size(229, 29);
            this.textBox_writeAddr.TabIndex = 20;
            this.textBox_writeAddr.Tag = "0";
            this.textBox_writeAddr.Text = "0";
            this.textBox_writeAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_writeAddr.Leave += new System.EventHandler(this.TextBox_writeAddr_Leave);
            // 
            // textBox_readFlashAddress
            // 
            this.textBox_readFlashAddress.Location = new System.Drawing.Point(185, 12);
            this.textBox_readFlashAddress.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_readFlashAddress.MaxLength = 10;
            this.textBox_readFlashAddress.Name = "textBox_readFlashAddress";
            this.textBox_readFlashAddress.Size = new System.Drawing.Size(120, 29);
            this.textBox_readFlashAddress.TabIndex = 19;
            this.textBox_readFlashAddress.Tag = "0";
            this.textBox_readFlashAddress.Text = "0";
            this.textBox_readFlashAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_readFlashAddress.Leave += new System.EventHandler(this.TextBox_readFlash_Leave);
            // 
            // button_writeFlash
            // 
            this.button_writeFlash.Enabled = false;
            this.button_writeFlash.Location = new System.Drawing.Point(4, 58);
            this.button_writeFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_writeFlash.Name = "button_writeFlash";
            this.button_writeFlash.Size = new System.Drawing.Size(120, 74);
            this.button_writeFlash.TabIndex = 3;
            this.button_writeFlash.Text = "Write flash";
            this.button_writeFlash.UseVisualStyleBackColor = true;
            this.button_writeFlash.Click += new System.EventHandler(this.Button_writeFlash_Click);
            // 
            // button_quickDump
            // 
            this.button_quickDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_quickDump.Enabled = false;
            this.button_quickDump.Location = new System.Drawing.Point(5, 224);
            this.button_quickDump.Margin = new System.Windows.Forms.Padding(6);
            this.button_quickDump.Name = "button_quickDump";
            this.button_quickDump.Size = new System.Drawing.Size(171, 42);
            this.button_quickDump.TabIndex = 3;
            this.button_quickDump.Text = "Quick dump";
            this.button_quickDump.UseVisualStyleBackColor = true;
            this.button_quickDump.Click += new System.EventHandler(this.Button_quickDump_Click);
            // 
            // button_readFlash
            // 
            this.button_readFlash.Enabled = false;
            this.button_readFlash.Location = new System.Drawing.Point(4, 6);
            this.button_readFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_readFlash.Name = "button_readFlash";
            this.button_readFlash.Size = new System.Drawing.Size(120, 42);
            this.button_readFlash.TabIndex = 3;
            this.button_readFlash.Text = "Read flash";
            this.button_readFlash.UseVisualStyleBackColor = true;
            this.button_readFlash.Click += new System.EventHandler(this.Button_readFlash_Click);
            // 
            // tabPage_Config
            // 
            this.tabPage_Config.AutoScroll = true;
            this.tabPage_Config.Controls.Add(this.button_getConfig2);
            this.tabPage_Config.Controls.Add(this.groupBox2);
            this.tabPage_Config.Controls.Add(this.textBox_eraseBlock);
            this.tabPage_Config.Controls.Add(this.textBox_teamFlashSize);
            this.tabPage_Config.Controls.Add(this.button_setEraseBlock);
            this.tabPage_Config.Controls.Add(this.button_setTeamFlashSize);
            this.tabPage_Config.Controls.Add(this.button_setChipType);
            this.tabPage_Config.Controls.Add(this.button_setGain);
            this.tabPage_Config.Controls.Add(this.comboBox_setGain);
            this.tabPage_Config.Controls.Add(this.label20);
            this.tabPage_Config.Controls.Add(this.label24);
            this.tabPage_Config.Controls.Add(this.label18);
            this.tabPage_Config.Controls.Add(this.button_setAutoReport);
            this.tabPage_Config.Controls.Add(this.button_setTime);
            this.tabPage_Config.Controls.Add(this.checkBox_AutoReport);
            this.tabPage_Config.Controls.Add(this.textBox_setTime);
            this.tabPage_Config.Controls.Add(this.checkBox_autoTime);
            this.tabPage_Config.Controls.Add(this.comboBox_flashSize);
            this.tabPage_Config.Controls.Add(this.textBox_packetLength);
            this.tabPage_Config.Controls.Add(this.comboBox_chipType);
            this.tabPage_Config.Controls.Add(this.textBox_flashSize);
            this.tabPage_Config.Controls.Add(this.button_setBatteryLimit);
            this.tabPage_Config.Controls.Add(this.textBox_setBatteryLimit);
            this.tabPage_Config.Controls.Add(this.button_setKoeff);
            this.tabPage_Config.Controls.Add(this.textBox_koeff);
            this.tabPage_Config.Location = new System.Drawing.Point(4, 33);
            this.tabPage_Config.Name = "tabPage_Config";
            this.tabPage_Config.Size = new System.Drawing.Size(453, 272);
            this.tabPage_Config.TabIndex = 3;
            this.tabPage_Config.Text = "Config";
            this.tabPage_Config.UseVisualStyleBackColor = true;
            // 
            // button_getConfig2
            // 
            this.button_getConfig2.Enabled = false;
            this.button_getConfig2.Location = new System.Drawing.Point(6, 6);
            this.button_getConfig2.Margin = new System.Windows.Forms.Padding(6);
            this.button_getConfig2.Name = "button_getConfig2";
            this.button_getConfig2.Size = new System.Drawing.Size(147, 42);
            this.button_getConfig2.TabIndex = 26;
            this.button_getConfig2.Text = "Get config";
            this.button_getConfig2.UseVisualStyleBackColor = true;
            this.button_getConfig2.Click += new System.EventHandler(this.Button_getConfig_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox_sendBtCommand);
            this.groupBox2.Controls.Add(this.button_sendBtCommand);
            this.groupBox2.Controls.Add(this.button_SetBtName);
            this.groupBox2.Controls.Add(this.textBox_BtPin);
            this.groupBox2.Controls.Add(this.button_SetBtPin);
            this.groupBox2.Controls.Add(this.textBox_BtName);
            this.groupBox2.Location = new System.Drawing.Point(6, 573);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(425, 193);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bluetooth settings";
            // 
            // textBox_sendBtCommand
            // 
            this.textBox_sendBtCommand.Location = new System.Drawing.Point(268, 145);
            this.textBox_sendBtCommand.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_sendBtCommand.MaxLength = 16;
            this.textBox_sendBtCommand.Name = "textBox_sendBtCommand";
            this.textBox_sendBtCommand.Size = new System.Drawing.Size(148, 29);
            this.textBox_sendBtCommand.TabIndex = 26;
            this.textBox_sendBtCommand.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button_sendBtCommand
            // 
            this.button_sendBtCommand.Enabled = false;
            this.button_sendBtCommand.Location = new System.Drawing.Point(9, 139);
            this.button_sendBtCommand.Margin = new System.Windows.Forms.Padding(6);
            this.button_sendBtCommand.Name = "button_sendBtCommand";
            this.button_sendBtCommand.Size = new System.Drawing.Size(183, 42);
            this.button_sendBtCommand.TabIndex = 25;
            this.button_sendBtCommand.Text = "Send command";
            this.button_sendBtCommand.UseVisualStyleBackColor = true;
            this.button_sendBtCommand.Click += new System.EventHandler(this.Button_sendBtCommand_Click);
            // 
            // button_SetBtName
            // 
            this.button_SetBtName.Enabled = false;
            this.button_SetBtName.Location = new System.Drawing.Point(9, 31);
            this.button_SetBtName.Margin = new System.Windows.Forms.Padding(6);
            this.button_SetBtName.Name = "button_SetBtName";
            this.button_SetBtName.Size = new System.Drawing.Size(183, 42);
            this.button_SetBtName.TabIndex = 23;
            this.button_SetBtName.Text = "Set Bluetooth name";
            this.button_SetBtName.UseVisualStyleBackColor = true;
            this.button_SetBtName.Click += new System.EventHandler(this.Button_SetBtName_Click);
            // 
            // textBox_BtPin
            // 
            this.textBox_BtPin.Location = new System.Drawing.Point(268, 91);
            this.textBox_BtPin.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_BtPin.MaxLength = 16;
            this.textBox_BtPin.Name = "textBox_BtPin";
            this.textBox_BtPin.Size = new System.Drawing.Size(148, 29);
            this.textBox_BtPin.TabIndex = 24;
            this.textBox_BtPin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_BtPin.Leave += new System.EventHandler(this.TextBox_BtPin_Leave);
            // 
            // button_SetBtPin
            // 
            this.button_SetBtPin.Enabled = false;
            this.button_SetBtPin.Location = new System.Drawing.Point(9, 85);
            this.button_SetBtPin.Margin = new System.Windows.Forms.Padding(6);
            this.button_SetBtPin.Name = "button_SetBtPin";
            this.button_SetBtPin.Size = new System.Drawing.Size(183, 42);
            this.button_SetBtPin.TabIndex = 23;
            this.button_SetBtPin.Text = "Set Bluetooth pin";
            this.button_SetBtPin.UseVisualStyleBackColor = true;
            this.button_SetBtPin.Click += new System.EventHandler(this.Button_SetBtPin_Click);
            // 
            // textBox_BtName
            // 
            this.textBox_BtName.Location = new System.Drawing.Point(268, 37);
            this.textBox_BtName.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_BtName.MaxLength = 32;
            this.textBox_BtName.Name = "textBox_BtName";
            this.textBox_BtName.Size = new System.Drawing.Size(148, 29);
            this.textBox_BtName.TabIndex = 24;
            this.textBox_BtName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_BtName.Leave += new System.EventHandler(this.TextBox_BtName_Leave);
            // 
            // textBox_eraseBlock
            // 
            this.textBox_eraseBlock.Location = new System.Drawing.Point(272, 431);
            this.textBox_eraseBlock.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_eraseBlock.MaxLength = 10;
            this.textBox_eraseBlock.Name = "textBox_eraseBlock";
            this.textBox_eraseBlock.Size = new System.Drawing.Size(158, 29);
            this.textBox_eraseBlock.TabIndex = 24;
            this.textBox_eraseBlock.Text = "0";
            this.textBox_eraseBlock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_eraseBlock.Leave += new System.EventHandler(this.TextBox_eraseBlock_Leave);
            // 
            // textBox_teamFlashSize
            // 
            this.textBox_teamFlashSize.Location = new System.Drawing.Point(272, 377);
            this.textBox_teamFlashSize.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_teamFlashSize.MaxLength = 10;
            this.textBox_teamFlashSize.Name = "textBox_teamFlashSize";
            this.textBox_teamFlashSize.Size = new System.Drawing.Size(158, 29);
            this.textBox_teamFlashSize.TabIndex = 24;
            this.textBox_teamFlashSize.Text = "0";
            this.textBox_teamFlashSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_teamFlashSize.Leave += new System.EventHandler(this.TextBox_teamFlashSize_Leave);
            // 
            // button_setEraseBlock
            // 
            this.button_setEraseBlock.Enabled = false;
            this.button_setEraseBlock.Location = new System.Drawing.Point(6, 425);
            this.button_setEraseBlock.Margin = new System.Windows.Forms.Padding(6);
            this.button_setEraseBlock.Name = "button_setEraseBlock";
            this.button_setEraseBlock.Size = new System.Drawing.Size(196, 42);
            this.button_setEraseBlock.TabIndex = 23;
            this.button_setEraseBlock.Text = "Set erase block size";
            this.button_setEraseBlock.UseVisualStyleBackColor = true;
            this.button_setEraseBlock.Click += new System.EventHandler(this.Button_setEraseBlock_Click);
            // 
            // button_setTeamFlashSize
            // 
            this.button_setTeamFlashSize.Enabled = false;
            this.button_setTeamFlashSize.Location = new System.Drawing.Point(6, 371);
            this.button_setTeamFlashSize.Margin = new System.Windows.Forms.Padding(6);
            this.button_setTeamFlashSize.Name = "button_setTeamFlashSize";
            this.button_setTeamFlashSize.Size = new System.Drawing.Size(196, 42);
            this.button_setTeamFlashSize.TabIndex = 23;
            this.button_setTeamFlashSize.Text = "Set team block size";
            this.button_setTeamFlashSize.UseVisualStyleBackColor = true;
            this.button_setTeamFlashSize.Click += new System.EventHandler(this.Button_setTeamFlashSize_Click);
            // 
            // button_setChipType
            // 
            this.button_setChipType.Enabled = false;
            this.button_setChipType.Location = new System.Drawing.Point(6, 276);
            this.button_setChipType.Margin = new System.Windows.Forms.Padding(6);
            this.button_setChipType.Name = "button_setChipType";
            this.button_setChipType.Size = new System.Drawing.Size(196, 42);
            this.button_setChipType.TabIndex = 22;
            this.button_setChipType.Text = "Set chip type";
            this.button_setChipType.UseVisualStyleBackColor = true;
            this.button_setChipType.Click += new System.EventHandler(this.Button_setChipType_Click);
            // 
            // button_setGain
            // 
            this.button_setGain.Enabled = false;
            this.button_setGain.Location = new System.Drawing.Point(6, 222);
            this.button_setGain.Margin = new System.Windows.Forms.Padding(6);
            this.button_setGain.Name = "button_setGain";
            this.button_setGain.Size = new System.Drawing.Size(196, 42);
            this.button_setGain.TabIndex = 22;
            this.button_setGain.Text = "Set antenna gain";
            this.button_setGain.UseVisualStyleBackColor = true;
            this.button_setGain.Click += new System.EventHandler(this.Button_setGain_Click);
            // 
            // comboBox_setGain
            // 
            this.comboBox_setGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_setGain.FormattingEnabled = true;
            this.comboBox_setGain.Location = new System.Drawing.Point(272, 228);
            this.comboBox_setGain.Name = "comboBox_setGain";
            this.comboBox_setGain.Size = new System.Drawing.Size(158, 32);
            this.comboBox_setGain.TabIndex = 21;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(201, 333);
            this.label20.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(62, 24);
            this.label20.TabIndex = 20;
            this.label20.Text = "limit to";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(8, 538);
            this.label24.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(123, 24);
            this.label24.TabIndex = 19;
            this.label24.Text = "Packet length";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(8, 333);
            this.label18.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(94, 24);
            this.label18.TabIndex = 19;
            this.label18.Text = "Flash size";
            // 
            // button_setAutoReport
            // 
            this.button_setAutoReport.Enabled = false;
            this.button_setAutoReport.Location = new System.Drawing.Point(6, 479);
            this.button_setAutoReport.Margin = new System.Windows.Forms.Padding(6);
            this.button_setAutoReport.Name = "button_setAutoReport";
            this.button_setAutoReport.Size = new System.Drawing.Size(196, 42);
            this.button_setAutoReport.TabIndex = 0;
            this.button_setAutoReport.Text = "Set autoreport";
            this.button_setAutoReport.UseVisualStyleBackColor = true;
            this.button_setAutoReport.Click += new System.EventHandler(this.Button_setAutoReport_Click);
            // 
            // button_setTime
            // 
            this.button_setTime.Enabled = false;
            this.button_setTime.Location = new System.Drawing.Point(6, 60);
            this.button_setTime.Margin = new System.Windows.Forms.Padding(6);
            this.button_setTime.Name = "button_setTime";
            this.button_setTime.Size = new System.Drawing.Size(118, 42);
            this.button_setTime.TabIndex = 0;
            this.button_setTime.Text = "Set time";
            this.button_setTime.UseVisualStyleBackColor = true;
            this.button_setTime.Click += new System.EventHandler(this.Button_setTime_Click);
            // 
            // checkBox_AutoReport
            // 
            this.checkBox_AutoReport.AutoSize = true;
            this.checkBox_AutoReport.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox_AutoReport.Checked = true;
            this.checkBox_AutoReport.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoReport.Location = new System.Drawing.Point(272, 494);
            this.checkBox_AutoReport.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox_AutoReport.Name = "checkBox_AutoReport";
            this.checkBox_AutoReport.Size = new System.Drawing.Size(15, 14);
            this.checkBox_AutoReport.TabIndex = 7;
            this.checkBox_AutoReport.UseVisualStyleBackColor = true;
            this.checkBox_AutoReport.CheckedChanged += new System.EventHandler(this.CheckBox_AutoReport_CheckedChanged);
            // 
            // textBox_setTime
            // 
            this.textBox_setTime.Enabled = false;
            this.textBox_setTime.Location = new System.Drawing.Point(234, 66);
            this.textBox_setTime.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_setTime.MaxLength = 20;
            this.textBox_setTime.Name = "textBox_setTime";
            this.textBox_setTime.Size = new System.Drawing.Size(196, 29);
            this.textBox_setTime.TabIndex = 1;
            this.textBox_setTime.Text = "2000.01.01 00:00:00";
            this.textBox_setTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_setTime.Leave += new System.EventHandler(this.TextBox_setTime_Leave);
            // 
            // checkBox_autoTime
            // 
            this.checkBox_autoTime.AutoSize = true;
            this.checkBox_autoTime.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox_autoTime.Checked = true;
            this.checkBox_autoTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoTime.Location = new System.Drawing.Point(134, 68);
            this.checkBox_autoTime.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox_autoTime.Name = "checkBox_autoTime";
            this.checkBox_autoTime.Size = new System.Drawing.Size(88, 28);
            this.checkBox_autoTime.TabIndex = 7;
            this.checkBox_autoTime.Text = "current";
            this.checkBox_autoTime.UseVisualStyleBackColor = true;
            this.checkBox_autoTime.CheckedChanged += new System.EventHandler(this.CheckBox_autoTime_CheckedChanged);
            // 
            // comboBox_flashSize
            // 
            this.comboBox_flashSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_flashSize.FormattingEnabled = true;
            this.comboBox_flashSize.Location = new System.Drawing.Point(272, 327);
            this.comboBox_flashSize.Name = "comboBox_flashSize";
            this.comboBox_flashSize.Size = new System.Drawing.Size(158, 32);
            this.comboBox_flashSize.TabIndex = 18;
            this.comboBox_flashSize.SelectedIndexChanged += new System.EventHandler(this.ComboBox_flashSize_SelectedIndexChanged);
            // 
            // textBox_packetLength
            // 
            this.textBox_packetLength.Enabled = false;
            this.textBox_packetLength.Location = new System.Drawing.Point(272, 535);
            this.textBox_packetLength.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_packetLength.MaxLength = 4;
            this.textBox_packetLength.Name = "textBox_packetLength";
            this.textBox_packetLength.ReadOnly = true;
            this.textBox_packetLength.Size = new System.Drawing.Size(158, 29);
            this.textBox_packetLength.TabIndex = 2;
            this.textBox_packetLength.Text = "0";
            this.textBox_packetLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // comboBox_chipType
            // 
            this.comboBox_chipType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_chipType.FormattingEnabled = true;
            this.comboBox_chipType.Location = new System.Drawing.Point(272, 282);
            this.comboBox_chipType.Name = "comboBox_chipType";
            this.comboBox_chipType.Size = new System.Drawing.Size(158, 32);
            this.comboBox_chipType.TabIndex = 18;
            this.comboBox_chipType.SelectedIndexChanged += new System.EventHandler(this.ComboBox_chipType_SelectedIndexChanged);
            // 
            // textBox_flashSize
            // 
            this.textBox_flashSize.Enabled = false;
            this.textBox_flashSize.Location = new System.Drawing.Point(114, 330);
            this.textBox_flashSize.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_flashSize.MaxLength = 4;
            this.textBox_flashSize.Name = "textBox_flashSize";
            this.textBox_flashSize.Size = new System.Drawing.Size(75, 29);
            this.textBox_flashSize.TabIndex = 2;
            this.textBox_flashSize.Text = "0 Mb";
            this.textBox_flashSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button_setBatteryLimit
            // 
            this.button_setBatteryLimit.Enabled = false;
            this.button_setBatteryLimit.Location = new System.Drawing.Point(6, 168);
            this.button_setBatteryLimit.Margin = new System.Windows.Forms.Padding(6);
            this.button_setBatteryLimit.Name = "button_setBatteryLimit";
            this.button_setBatteryLimit.Size = new System.Drawing.Size(196, 42);
            this.button_setBatteryLimit.TabIndex = 0;
            this.button_setBatteryLimit.Text = "Set battery limit";
            this.button_setBatteryLimit.UseVisualStyleBackColor = true;
            this.button_setBatteryLimit.Click += new System.EventHandler(this.Button_setBatteryLimit_Click);
            // 
            // textBox_setBatteryLimit
            // 
            this.textBox_setBatteryLimit.Location = new System.Drawing.Point(272, 174);
            this.textBox_setBatteryLimit.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_setBatteryLimit.MaxLength = 10;
            this.textBox_setBatteryLimit.Name = "textBox_setBatteryLimit";
            this.textBox_setBatteryLimit.Size = new System.Drawing.Size(158, 29);
            this.textBox_setBatteryLimit.TabIndex = 2;
            this.textBox_setBatteryLimit.Text = "3,0";
            this.textBox_setBatteryLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_setBatteryLimit.Leave += new System.EventHandler(this.TextBox_setBatteryLimit_Leave);
            // 
            // button_setKoeff
            // 
            this.button_setKoeff.Enabled = false;
            this.button_setKoeff.Location = new System.Drawing.Point(6, 114);
            this.button_setKoeff.Margin = new System.Windows.Forms.Padding(6);
            this.button_setKoeff.Name = "button_setKoeff";
            this.button_setKoeff.Size = new System.Drawing.Size(196, 42);
            this.button_setKoeff.TabIndex = 0;
            this.button_setKoeff.Text = "Set V coeff.";
            this.button_setKoeff.UseVisualStyleBackColor = true;
            this.button_setKoeff.Click += new System.EventHandler(this.Button_setVCoeff_Click);
            // 
            // textBox_koeff
            // 
            this.textBox_koeff.Location = new System.Drawing.Point(272, 120);
            this.textBox_koeff.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_koeff.MaxLength = 10;
            this.textBox_koeff.Name = "textBox_koeff";
            this.textBox_koeff.Size = new System.Drawing.Size(158, 29);
            this.textBox_koeff.TabIndex = 2;
            this.textBox_koeff.Text = "0,00578";
            this.textBox_koeff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_koeff.Leave += new System.EventHandler(this.TextBox_koeff_Leave);
            // 
            // button_dumpFlash
            // 
            this.button_dumpFlash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_dumpFlash.Enabled = false;
            this.button_dumpFlash.Location = new System.Drawing.Point(118, 271);
            this.button_dumpFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_dumpFlash.Name = "button_dumpFlash";
            this.button_dumpFlash.Size = new System.Drawing.Size(121, 32);
            this.button_dumpFlash.TabIndex = 17;
            this.button_dumpFlash.Text = "Dump flash";
            this.button_dumpFlash.UseVisualStyleBackColor = true;
            this.button_dumpFlash.Click += new System.EventHandler(this.Button_dumpFlash_Click);
            // 
            // button_dumpChip
            // 
            this.button_dumpChip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_dumpChip.Enabled = false;
            this.button_dumpChip.Location = new System.Drawing.Point(118, 271);
            this.button_dumpChip.Margin = new System.Windows.Forms.Padding(6);
            this.button_dumpChip.Name = "button_dumpChip";
            this.button_dumpChip.Size = new System.Drawing.Size(121, 32);
            this.button_dumpChip.TabIndex = 3;
            this.button_dumpChip.Text = "Dump chip";
            this.button_dumpChip.UseVisualStyleBackColor = true;
            this.button_dumpChip.Click += new System.EventHandler(this.Button_dumpChip_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(201, 24);
            this.label2.TabIndex = 10;
            this.label2.Text = "Current station number";
            // 
            // textBox_stationNumber
            // 
            this.textBox_stationNumber.Location = new System.Drawing.Point(210, 4);
            this.textBox_stationNumber.MaxLength = 3;
            this.textBox_stationNumber.Name = "textBox_stationNumber";
            this.textBox_stationNumber.Size = new System.Drawing.Size(102, 29);
            this.textBox_stationNumber.TabIndex = 9;
            this.textBox_stationNumber.Text = "0";
            this.textBox_stationNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_stationNumber.Leave += new System.EventHandler(this.TextBox_stationNumber_Leave);
            // 
            // textBox_terminal
            // 
            this.textBox_terminal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_terminal.Font = new System.Drawing.Font("Courier New", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_terminal.HideSelection = false;
            this.textBox_terminal.Location = new System.Drawing.Point(3, 3);
            this.textBox_terminal.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_terminal.MaxLength = 3276700;
            this.textBox_terminal.Multiline = true;
            this.textBox_terminal.Name = "textBox_terminal";
            this.textBox_terminal.ReadOnly = true;
            this.textBox_terminal.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_terminal.Size = new System.Drawing.Size(453, 259);
            this.textBox_terminal.TabIndex = 1;
            this.textBox_terminal.TextChanged += new System.EventHandler(this.TextBox_terminal_TextChanged);
            // 
            // serialPort1
            // 
            this.serialPort1.BaudRate = 57600;
            this.serialPort1.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler(this.SerialPort1_ErrorReceived);
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPort1_DataReceived);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Title = "Save log to file...";
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog1_FileOk);
            // 
            // comboBox_portName
            // 
            this.comboBox_portName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_portName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_portName.FormattingEnabled = true;
            this.comboBox_portName.Location = new System.Drawing.Point(84, 363);
            this.comboBox_portName.Margin = new System.Windows.Forms.Padding(6);
            this.comboBox_portName.Name = "comboBox_portName";
            this.comboBox_portName.Size = new System.Drawing.Size(406, 32);
            this.comboBox_portName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 366);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "Port #";
            // 
            // button_openPort
            // 
            this.button_openPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_openPort.Location = new System.Drawing.Point(503, 357);
            this.button_openPort.Margin = new System.Windows.Forms.Padding(6);
            this.button_openPort.Name = "button_openPort";
            this.button_openPort.Size = new System.Drawing.Size(133, 42);
            this.button_openPort.TabIndex = 5;
            this.button_openPort.Text = "Open port";
            this.button_openPort.UseVisualStyleBackColor = true;
            this.button_openPort.Click += new System.EventHandler(this.Button_openPort_Click);
            // 
            // button_clearLog
            // 
            this.button_clearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_clearLog.Location = new System.Drawing.Point(6, 271);
            this.button_clearLog.Margin = new System.Windows.Forms.Padding(6);
            this.button_clearLog.Name = "button_clearLog";
            this.button_clearLog.Size = new System.Drawing.Size(100, 32);
            this.button_clearLog.TabIndex = 6;
            this.button_clearLog.Text = "Clear";
            this.button_clearLog.UseVisualStyleBackColor = true;
            this.button_clearLog.Click += new System.EventHandler(this.Button_clearLog_Click);
            // 
            // button_closePort
            // 
            this.button_closePort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_closePort.Enabled = false;
            this.button_closePort.Location = new System.Drawing.Point(786, 357);
            this.button_closePort.Margin = new System.Windows.Forms.Padding(6);
            this.button_closePort.Name = "button_closePort";
            this.button_closePort.Size = new System.Drawing.Size(133, 42);
            this.button_closePort.TabIndex = 4;
            this.button_closePort.Text = "Close port";
            this.button_closePort.UseVisualStyleBackColor = true;
            this.button_closePort.Click += new System.EventHandler(this.Button_closePort_Click);
            // 
            // button_saveLog
            // 
            this.button_saveLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveLog.Location = new System.Drawing.Point(350, 271);
            this.button_saveLog.Margin = new System.Windows.Forms.Padding(6);
            this.button_saveLog.Name = "button_saveLog";
            this.button_saveLog.Size = new System.Drawing.Size(100, 32);
            this.button_saveLog.TabIndex = 7;
            this.button_saveLog.Text = "Save";
            this.button_saveLog.UseVisualStyleBackColor = true;
            this.button_saveLog.Click += new System.EventHandler(this.Button_saveLog_Click);
            // 
            // checkBox_autoScroll
            // 
            this.checkBox_autoScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoScroll.AutoSize = true;
            this.checkBox_autoScroll.Checked = true;
            this.checkBox_autoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoScroll.Location = new System.Drawing.Point(118, 285);
            this.checkBox_autoScroll.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox_autoScroll.Name = "checkBox_autoScroll";
            this.checkBox_autoScroll.Size = new System.Drawing.Size(112, 28);
            this.checkBox_autoScroll.TabIndex = 8;
            this.checkBox_autoScroll.Text = "Autoscroll";
            this.checkBox_autoScroll.UseVisualStyleBackColor = true;
            // 
            // button_refresh
            // 
            this.button_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_refresh.Location = new System.Drawing.Point(645, 357);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(132, 42);
            this.button_refresh.TabIndex = 10;
            this.button_refresh.Text = "Refresh";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.Button_refresh_Click);
            // 
            // checkBox_portMon
            // 
            this.checkBox_portMon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_portMon.AutoSize = true;
            this.checkBox_portMon.Checked = true;
            this.checkBox_portMon.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_portMon.Location = new System.Drawing.Point(118, 262);
            this.checkBox_portMon.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox_portMon.Name = "checkBox_portMon";
            this.checkBox_portMon.Size = new System.Drawing.Size(130, 28);
            this.checkBox_portMon.TabIndex = 9;
            this.checkBox_portMon.Text = "Port monitor";
            this.checkBox_portMon.UseVisualStyleBackColor = true;
            // 
            // dataGridView_teams
            // 
            this.dataGridView_teams.AllowUserToAddRows = false;
            this.dataGridView_teams.AllowUserToDeleteRows = false;
            this.dataGridView_teams.AllowUserToResizeRows = false;
            this.dataGridView_teams.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_teams.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_teams.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView_teams.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_teams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_teams.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView_teams.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_teams.MultiSelect = false;
            this.dataGridView_teams.Name = "dataGridView_teams";
            this.dataGridView_teams.ReadOnly = true;
            this.dataGridView_teams.RowHeadersVisible = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dataGridView_teams.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_teams.Size = new System.Drawing.Size(456, 251);
            this.dataGridView_teams.TabIndex = 12;
            this.dataGridView_teams.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_teams_CellDoubleClick);
            // 
            // tabControl_teamData
            // 
            this.tabControl_teamData.Controls.Add(this.tabPage_terminal);
            this.tabControl_teamData.Controls.Add(this.tabPage_teams);
            this.tabControl_teamData.Controls.Add(this.tabPage_cardContent);
            this.tabControl_teamData.Controls.Add(this.tabPage_flashContent);
            this.tabControl_teamData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_teamData.Location = new System.Drawing.Point(0, 0);
            this.tabControl_teamData.Name = "tabControl_teamData";
            this.tabControl_teamData.SelectedIndex = 0;
            this.tabControl_teamData.Size = new System.Drawing.Size(464, 346);
            this.tabControl_teamData.TabIndex = 13;
            this.tabControl_teamData.SelectedIndexChanged += new System.EventHandler(this.TabControl_teamData_SelectedIndexChanged);
            // 
            // tabPage_terminal
            // 
            this.tabPage_terminal.Controls.Add(this.textBox_terminal);
            this.tabPage_terminal.Controls.Add(this.button_saveLog);
            this.tabPage_terminal.Controls.Add(this.checkBox_portMon);
            this.tabPage_terminal.Controls.Add(this.checkBox_autoScroll);
            this.tabPage_terminal.Controls.Add(this.button_clearLog);
            this.tabPage_terminal.Location = new System.Drawing.Point(4, 33);
            this.tabPage_terminal.Name = "tabPage_terminal";
            this.tabPage_terminal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_terminal.Size = new System.Drawing.Size(456, 309);
            this.tabPage_terminal.TabIndex = 0;
            this.tabPage_terminal.Text = "Terminal";
            this.tabPage_terminal.UseVisualStyleBackColor = true;
            // 
            // tabPage_teams
            // 
            this.tabPage_teams.AutoScroll = true;
            this.tabPage_teams.Controls.Add(this.button_dumpTeams);
            this.tabPage_teams.Controls.Add(this.button_clearTeams);
            this.tabPage_teams.Controls.Add(this.button_saveTeams);
            this.tabPage_teams.Controls.Add(this.dataGridView_teams);
            this.tabPage_teams.Location = new System.Drawing.Point(4, 33);
            this.tabPage_teams.Name = "tabPage_teams";
            this.tabPage_teams.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_teams.Size = new System.Drawing.Size(456, 309);
            this.tabPage_teams.TabIndex = 1;
            this.tabPage_teams.Text = "Teams";
            this.tabPage_teams.UseVisualStyleBackColor = true;
            // 
            // button_dumpTeams
            // 
            this.button_dumpTeams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_dumpTeams.Enabled = false;
            this.button_dumpTeams.Location = new System.Drawing.Point(118, 260);
            this.button_dumpTeams.Margin = new System.Windows.Forms.Padding(6);
            this.button_dumpTeams.Name = "button_dumpTeams";
            this.button_dumpTeams.Size = new System.Drawing.Size(128, 32);
            this.button_dumpTeams.TabIndex = 22;
            this.button_dumpTeams.Text = "Dump teams";
            this.button_dumpTeams.UseVisualStyleBackColor = true;
            this.button_dumpTeams.Click += new System.EventHandler(this.Button_dumpTeams_Click);
            // 
            // button_clearTeams
            // 
            this.button_clearTeams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_clearTeams.Location = new System.Drawing.Point(6, 260);
            this.button_clearTeams.Margin = new System.Windows.Forms.Padding(6);
            this.button_clearTeams.Name = "button_clearTeams";
            this.button_clearTeams.Size = new System.Drawing.Size(100, 32);
            this.button_clearTeams.TabIndex = 20;
            this.button_clearTeams.Text = "Clear";
            this.button_clearTeams.UseVisualStyleBackColor = true;
            this.button_clearTeams.Click += new System.EventHandler(this.Button_clearTeams_Click);
            // 
            // button_saveTeams
            // 
            this.button_saveTeams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveTeams.Location = new System.Drawing.Point(350, 260);
            this.button_saveTeams.Margin = new System.Windows.Forms.Padding(6);
            this.button_saveTeams.Name = "button_saveTeams";
            this.button_saveTeams.Size = new System.Drawing.Size(100, 32);
            this.button_saveTeams.TabIndex = 21;
            this.button_saveTeams.Text = "Save";
            this.button_saveTeams.UseVisualStyleBackColor = true;
            this.button_saveTeams.Click += new System.EventHandler(this.Button_saveTeams_Click);
            // 
            // tabPage_cardContent
            // 
            this.tabPage_cardContent.AutoScroll = true;
            this.tabPage_cardContent.Controls.Add(this.button_clearRfid);
            this.tabPage_cardContent.Controls.Add(this.button_loadRfid);
            this.tabPage_cardContent.Controls.Add(this.button_saveRfid);
            this.tabPage_cardContent.Controls.Add(this.dataGridView_chipRawData);
            this.tabPage_cardContent.Controls.Add(this.button_dumpChip);
            this.tabPage_cardContent.Location = new System.Drawing.Point(4, 22);
            this.tabPage_cardContent.Name = "tabPage_cardContent";
            this.tabPage_cardContent.Size = new System.Drawing.Size(456, 320);
            this.tabPage_cardContent.TabIndex = 2;
            this.tabPage_cardContent.Text = "RFID";
            this.tabPage_cardContent.UseVisualStyleBackColor = true;
            // 
            // button_clearRfid
            // 
            this.button_clearRfid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_clearRfid.Location = new System.Drawing.Point(6, 271);
            this.button_clearRfid.Margin = new System.Windows.Forms.Padding(6);
            this.button_clearRfid.Name = "button_clearRfid";
            this.button_clearRfid.Size = new System.Drawing.Size(100, 32);
            this.button_clearRfid.TabIndex = 19;
            this.button_clearRfid.Text = "Clear";
            this.button_clearRfid.UseVisualStyleBackColor = true;
            this.button_clearRfid.Click += new System.EventHandler(this.Button_clearRfid_Click);
            // 
            // button_loadRfid
            // 
            this.button_loadRfid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadRfid.Location = new System.Drawing.Point(251, 271);
            this.button_loadRfid.Margin = new System.Windows.Forms.Padding(6);
            this.button_loadRfid.Name = "button_loadRfid";
            this.button_loadRfid.Size = new System.Drawing.Size(87, 32);
            this.button_loadRfid.TabIndex = 19;
            this.button_loadRfid.Text = "Load";
            this.button_loadRfid.UseVisualStyleBackColor = true;
            this.button_loadRfid.Click += new System.EventHandler(this.Button_loadRfid_Click);
            // 
            // button_saveRfid
            // 
            this.button_saveRfid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveRfid.Location = new System.Drawing.Point(350, 271);
            this.button_saveRfid.Margin = new System.Windows.Forms.Padding(6);
            this.button_saveRfid.Name = "button_saveRfid";
            this.button_saveRfid.Size = new System.Drawing.Size(100, 32);
            this.button_saveRfid.TabIndex = 19;
            this.button_saveRfid.Text = "Save";
            this.button_saveRfid.UseVisualStyleBackColor = true;
            this.button_saveRfid.Click += new System.EventHandler(this.Button_saveRfid_Click);
            // 
            // dataGridView_chipRawData
            // 
            this.dataGridView_chipRawData.AllowUserToAddRows = false;
            this.dataGridView_chipRawData.AllowUserToDeleteRows = false;
            this.dataGridView_chipRawData.AllowUserToResizeRows = false;
            this.dataGridView_chipRawData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_chipRawData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_chipRawData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView_chipRawData.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_chipRawData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_chipRawData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView_chipRawData.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_chipRawData.MultiSelect = false;
            this.dataGridView_chipRawData.Name = "dataGridView_chipRawData";
            this.dataGridView_chipRawData.ReadOnly = true;
            this.dataGridView_chipRawData.RowHeadersVisible = false;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dataGridView_chipRawData.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_chipRawData.Size = new System.Drawing.Size(456, 262);
            this.dataGridView_chipRawData.TabIndex = 0;
            this.dataGridView_chipRawData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_chipRawData_CellDoubleClick);
            // 
            // tabPage_flashContent
            // 
            this.tabPage_flashContent.AutoScroll = true;
            this.tabPage_flashContent.Controls.Add(this.button_clearFlash);
            this.tabPage_flashContent.Controls.Add(this.button_loadFlash);
            this.tabPage_flashContent.Controls.Add(this.button_saveFlash);
            this.tabPage_flashContent.Controls.Add(this.button_dumpFlash);
            this.tabPage_flashContent.Controls.Add(this.dataGridView_flashRawData);
            this.tabPage_flashContent.Location = new System.Drawing.Point(4, 22);
            this.tabPage_flashContent.Name = "tabPage_flashContent";
            this.tabPage_flashContent.Size = new System.Drawing.Size(456, 320);
            this.tabPage_flashContent.TabIndex = 3;
            this.tabPage_flashContent.Text = "Flash";
            this.tabPage_flashContent.UseVisualStyleBackColor = true;
            // 
            // button_clearFlash
            // 
            this.button_clearFlash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_clearFlash.Location = new System.Drawing.Point(6, 271);
            this.button_clearFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_clearFlash.Name = "button_clearFlash";
            this.button_clearFlash.Size = new System.Drawing.Size(100, 32);
            this.button_clearFlash.TabIndex = 20;
            this.button_clearFlash.Text = "Clear";
            this.button_clearFlash.UseVisualStyleBackColor = true;
            this.button_clearFlash.Click += new System.EventHandler(this.Button_clearFlash_Click);
            // 
            // button_loadFlash
            // 
            this.button_loadFlash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadFlash.Location = new System.Drawing.Point(251, 271);
            this.button_loadFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_loadFlash.Name = "button_loadFlash";
            this.button_loadFlash.Size = new System.Drawing.Size(87, 32);
            this.button_loadFlash.TabIndex = 21;
            this.button_loadFlash.Text = "Load";
            this.button_loadFlash.UseVisualStyleBackColor = true;
            this.button_loadFlash.Click += new System.EventHandler(this.Button_loadFlash_Click);
            // 
            // button_saveFlash
            // 
            this.button_saveFlash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveFlash.Location = new System.Drawing.Point(350, 271);
            this.button_saveFlash.Margin = new System.Windows.Forms.Padding(6);
            this.button_saveFlash.Name = "button_saveFlash";
            this.button_saveFlash.Size = new System.Drawing.Size(100, 32);
            this.button_saveFlash.TabIndex = 21;
            this.button_saveFlash.Text = "Save";
            this.button_saveFlash.UseVisualStyleBackColor = true;
            this.button_saveFlash.Click += new System.EventHandler(this.Button_saveFlash_Click);
            // 
            // dataGridView_flashRawData
            // 
            this.dataGridView_flashRawData.AllowUserToAddRows = false;
            this.dataGridView_flashRawData.AllowUserToDeleteRows = false;
            this.dataGridView_flashRawData.AllowUserToResizeRows = false;
            this.dataGridView_flashRawData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_flashRawData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_flashRawData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView_flashRawData.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_flashRawData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_flashRawData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_flashRawData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView_flashRawData.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_flashRawData.MultiSelect = false;
            this.dataGridView_flashRawData.Name = "dataGridView_flashRawData";
            this.dataGridView_flashRawData.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_flashRawData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView_flashRawData.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_flashRawData.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView_flashRawData.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dataGridView_flashRawData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dataGridView_flashRawData.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_flashRawData.RowTemplate.ReadOnly = true;
            this.dataGridView_flashRawData.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_flashRawData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_flashRawData.Size = new System.Drawing.Size(456, 262);
            this.dataGridView_flashRawData.TabIndex = 1;
            this.dataGridView_flashRawData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_flashRawData_CellDoubleClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(1, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.label22);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.textBox_fwVersion);
            this.splitContainer1.Panel1.Controls.Add(this.textBox_stationNumber);
            this.splitContainer1.Panel1.Controls.Add(this.tabControl);
            this.splitContainer1.Panel1MinSize = 460;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.tabControl_teamData);
            this.splitContainer1.Panel2MinSize = 360;
            this.splitContainer1.Size = new System.Drawing.Size(932, 350);
            this.splitContainer1.SplitterDistance = 460;
            this.splitContainer1.TabIndex = 14;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(350, 7);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(33, 24);
            this.label22.TabIndex = 10;
            this.label22.Text = "fw.";
            // 
            // textBox_fwVersion
            // 
            this.textBox_fwVersion.Location = new System.Drawing.Point(389, 4);
            this.textBox_fwVersion.MaxLength = 10;
            this.textBox_fwVersion.Name = "textBox_fwVersion";
            this.textBox_fwVersion.ReadOnly = true;
            this.textBox_fwVersion.Size = new System.Drawing.Size(64, 29);
            this.textBox_fwVersion.TabIndex = 9;
            this.textBox_fwVersion.Text = "0";
            this.textBox_fwVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_fwVersion.WordWrap = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 412);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.button_refresh);
            this.Controls.Add(this.button_openPort);
            this.Controls.Add(this.button_closePort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_portName);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(950, 450);
            this.Name = "Form1";
            this.Text = "RFID Station control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPage_Station.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage_Team.ResumeLayout(false);
            this.tabPage_Team.PerformLayout();
            this.tabPage_Rfid.ResumeLayout(false);
            this.tabPage_Rfid.PerformLayout();
            this.tabPage_Flash.ResumeLayout(false);
            this.tabPage_Flash.PerformLayout();
            this.tabPage_Config.ResumeLayout(false);
            this.tabPage_Config.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_teams)).EndInit();
            this.tabControl_teamData.ResumeLayout(false);
            this.tabPage_terminal.ResumeLayout(false);
            this.tabPage_terminal.PerformLayout();
            this.tabPage_teams.ResumeLayout(false);
            this.tabPage_cardContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_chipRawData)).EndInit();
            this.tabPage_flashContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_flashRawData)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabControl tabControl;
        private TabPage tabPage_Station;
        private TextBox textBox_setTime;
        private Button button_setTime;
        private Button button_setMode;
        private TabPage tabPage_Rfid;
        private Button button_updTeamMask;
        private Button button_getTeamRecord;
        private Button button_initChip;
        private Button button_getStatus;
        private TextBox textBox_terminal;
        private SaveFileDialog saveFileDialog1;
        private ComboBox comboBox_portName;
        private Label label1;
        private Button button_openPort;
        private SerialPort serialPort1;
        private TextBox textBox_teamMask;
        private Button button_clearLog;
        private Button button_closePort;
        private Button button_saveLog;
        private Button button_resetStation;
        private CheckBox checkBox_autoTime;
        private CheckBox checkBox_autoScroll;
        private Button button_refresh;
        private ComboBox comboBox_mode;
        private Label label2;
        private TextBox textBox_stationNumber;
        private CheckBox checkBox_portMon;
        private Label label3;
        private Label label5;
        private Label label4;
        private TextBox textBox_lastCheck;
        private TextBox textBox_checkedChips;
        private TextBox textBox_newStationNumber;
        private TextBox textBox_issueTime;
        private Button button_getLastTeam;
        private Label label8;
        private Label label7;
        private TabPage tabPage_Flash;
        private GroupBox groupBox1;
        private DataGridView dataGridView_teams;
        private TabControl tabControl_teamData;
        private TabPage tabPage_terminal;
        private TabPage tabPage_teams;
        private SplitContainer splitContainer1;
        private ComboBox comboBox_chipType;
        private Button button_dumpChip;
        private TabPage tabPage_cardContent;
        private DataGridView dataGridView_chipRawData;
        private Button button_writeFlash;
        private Button button_readFlash;
        private Label label9;
        private Label label15;
        private Label label14;
        private TextBox textBox_flashData;
        private TextBox textBox_writeAddr;
        private TextBox textBox_readFlashAddress;
        private Button button_eraseTeamFlash;
        private Label label17;
        private Button button_dumpFlash;
        private TabPage tabPage_flashContent;
        private DataGridView dataGridView_flashRawData;
        private Button button_getConfig;
        private TextBox textBox_koeff;
        private Button button_setKoeff;
        private TextBox textBox_flashSize;
        private Label label13;
        private Label label12;
        private Label label11;
        private Label label10;
        private TextBox textBox_uid;
        private TextBox textBox_data;
        private TextBox textBox_readChipPage;
        private TextBox textBox_writeChipPage;
        private Button button_readChipPage;
        private Button button_writeChipPage;
        private TabPage tabPage_Config;
        private Button button_clearTeams;
        private Button button_saveTeams;
        private Button button_clearRfid;
        private Button button_saveRfid;
        private Button button_clearFlash;
        private Button button_saveFlash;
        private Button button_dumpTeams;
        private Label label18;
        private ComboBox comboBox_flashSize;
        private Label label20;
        private ComboBox comboBox_setGain;
        private Button button_setGain;
        private Button button_setChipType;
        private Button button_eraseChip;
        private TextBox textBox_teamFlashSize;
        private Button button_setTeamFlashSize;
        private TabPage tabPage_Team;
        private Label label16;
        private TextBox textBox_initMask;
        private Label label6;
        private TextBox textBox_initTeamNum;
        private Label label19;
        private TextBox textBox_eraseBlock;
        private Button button_setEraseBlock;
        private Label label21;
        private TextBox textBox_readFlashLength;
        private TextBox textBox_BtPin;
        private TextBox textBox_BtName;
        private Button button_SetBtPin;
        private Button button_SetBtName;
        private Label label22;
        private TextBox textBox_fwVersion;
        private GroupBox groupBox2;
        private Button button_setBatteryLimit;
        private TextBox textBox_setBatteryLimit;
        private Button button_loadRfid;
        private Button button_loadFlash;
        private OpenFileDialog openFileDialog1;
        private TextBox textBox_sendBtCommand;
        private Button button_sendBtCommand;
        private Label label23;
        private TextBox textBox_TeamNumber;
        private Button button_getTeamsList;
        private Button button_quickDump;
        private Button button_getLastErrors;
        private Button button_getConfig2;
        private Button button_setAutoReport;
        private CheckBox checkBox_AutoReport;
        private Label label24;
        private TextBox textBox_packetLength;
    }
}

