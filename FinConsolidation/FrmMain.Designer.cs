namespace FinConsolidation
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            PnlLeft = new Panel();
            PnlEnv = new Panel();
            RbEnvUat = new RadioButton();
            RbEnvLive = new RadioButton();
            PnlVersion = new Panel();
            LblVersion = new Label();
            TblFinChoiceBtn = new TableLayoutPanel();
            LblSelInd1 = new Label();
            LblSelInd2 = new Label();
            BtnFinChoiceAP = new Button();
            BtnFinChoiceAR = new Button();
            LblVertSep = new Label();
            PicAppLogo = new PictureBox();
            statusStrip1 = new StatusStrip();
            LblUser_Lbl = new ToolStripStatusLabel();
            LblUser = new ToolStripStatusLabel();
            LblStatus = new ToolStripStatusLabel();
            ProgBar = new ToolStripProgressBar();
            PnlTop = new Panel();
            LblHorSep1 = new Label();
            PnlTotals = new Panel();
            LblVertSep2 = new Label();
            TblTotals = new TableLayoutPanel();
            LblCalcAmt = new Label();
            LblCalcAmt_Lbl = new Label();
            LblAmtAfter_Lbl = new Label();
            LblEntity_Lbl = new Label();
            LblAmtBefore_Lbl = new Label();
            LblEntity = new Label();
            LblAmtBefore = new Label();
            LblAmtAfter = new Label();
            LblConPass_Lbl = new Label();
            LblConPass = new Label();
            PnlModRates = new Panel();
            LblVertSep1 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            BtnRefreshRates = new Button();
            BtnApplyCon = new Button();
            PnlItemSearch = new Panel();
            TblTripSearchControls = new TableLayoutPanel();
            LblItemSearch_Lbl = new Label();
            TxtItemSearch = new TextBox();
            BtnItemSearch = new Button();
            LblAppTitle = new Label();
            WebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            toolStripContainer1 = new ToolStripContainer();
            PnlRegion = new Panel();
            RbRegionNz = new RadioButton();
            RbRegionAu = new RadioButton();
            PnlLeft.SuspendLayout();
            PnlEnv.SuspendLayout();
            PnlVersion.SuspendLayout();
            TblFinChoiceBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicAppLogo).BeginInit();
            statusStrip1.SuspendLayout();
            PnlTop.SuspendLayout();
            PnlTotals.SuspendLayout();
            TblTotals.SuspendLayout();
            PnlModRates.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            PnlItemSearch.SuspendLayout();
            TblTripSearchControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)WebView).BeginInit();
            toolStripContainer1.SuspendLayout();
            PnlRegion.SuspendLayout();
            SuspendLayout();
            // 
            // PnlLeft
            // 
            PnlLeft.Controls.Add(PnlRegion);
            PnlLeft.Controls.Add(PnlEnv);
            PnlLeft.Controls.Add(PnlVersion);
            PnlLeft.Controls.Add(TblFinChoiceBtn);
            PnlLeft.Controls.Add(LblVertSep);
            PnlLeft.Controls.Add(PicAppLogo);
            PnlLeft.Dock = DockStyle.Left;
            PnlLeft.Location = new Point(0, 0);
            PnlLeft.Name = "PnlLeft";
            PnlLeft.Size = new Size(150, 881);
            PnlLeft.TabIndex = 0;
            // 
            // PnlEnv
            // 
            PnlEnv.Controls.Add(RbEnvUat);
            PnlEnv.Controls.Add(RbEnvLive);
            PnlEnv.Dock = DockStyle.Bottom;
            PnlEnv.Location = new Point(0, 766);
            PnlEnv.Margin = new Padding(3, 2, 3, 2);
            PnlEnv.Name = "PnlEnv";
            PnlEnv.Size = new Size(147, 75);
            PnlEnv.TabIndex = 5;
            // 
            // RbEnvUat
            // 
            RbEnvUat.AutoSize = true;
            RbEnvUat.Location = new Point(50, 42);
            RbEnvUat.Margin = new Padding(3, 2, 3, 2);
            RbEnvUat.Name = "RbEnvUat";
            RbEnvUat.Size = new Size(47, 19);
            RbEnvUat.TabIndex = 1;
            RbEnvUat.TabStop = true;
            RbEnvUat.Text = "UAT";
            RbEnvUat.UseVisualStyleBackColor = true;
            // 
            // RbEnvLive
            // 
            RbEnvLive.AutoSize = true;
            RbEnvLive.Location = new Point(50, 20);
            RbEnvLive.Margin = new Padding(3, 2, 3, 2);
            RbEnvLive.Name = "RbEnvLive";
            RbEnvLive.Size = new Size(46, 19);
            RbEnvLive.TabIndex = 0;
            RbEnvLive.TabStop = true;
            RbEnvLive.Text = "Live";
            RbEnvLive.UseVisualStyleBackColor = true;
            // 
            // PnlVersion
            // 
            PnlVersion.Controls.Add(LblVersion);
            PnlVersion.Dock = DockStyle.Bottom;
            PnlVersion.Location = new Point(0, 841);
            PnlVersion.Name = "PnlVersion";
            PnlVersion.Size = new Size(147, 40);
            PnlVersion.TabIndex = 4;
            // 
            // LblVersion
            // 
            LblVersion.AutoSize = true;
            LblVersion.Location = new Point(22, 14);
            LblVersion.Name = "LblVersion";
            LblVersion.Size = new Size(17, 15);
            LblVersion.TabIndex = 2;
            LblVersion.Text = "--";
            // 
            // TblFinChoiceBtn
            // 
            TblFinChoiceBtn.ColumnCount = 2;
            TblFinChoiceBtn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
            TblFinChoiceBtn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TblFinChoiceBtn.Controls.Add(LblSelInd1, 0, 0);
            TblFinChoiceBtn.Controls.Add(LblSelInd2, 0, 1);
            TblFinChoiceBtn.Controls.Add(BtnFinChoiceAP, 1, 0);
            TblFinChoiceBtn.Controls.Add(BtnFinChoiceAR, 1, 1);
            TblFinChoiceBtn.Location = new Point(22, 147);
            TblFinChoiceBtn.Name = "TblFinChoiceBtn";
            TblFinChoiceBtn.RowCount = 2;
            TblFinChoiceBtn.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblFinChoiceBtn.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblFinChoiceBtn.Size = new Size(106, 63);
            TblFinChoiceBtn.TabIndex = 2;
            // 
            // LblSelInd1
            // 
            LblSelInd1.Location = new Point(3, 6);
            LblSelInd1.Margin = new Padding(3, 6, 3, 0);
            LblSelInd1.Name = "LblSelInd1";
            LblSelInd1.Size = new Size(4, 18);
            LblSelInd1.TabIndex = 0;
            LblSelInd1.Text = " ";
            // 
            // LblSelInd2
            // 
            LblSelInd2.Location = new Point(3, 37);
            LblSelInd2.Margin = new Padding(3, 6, 3, 0);
            LblSelInd2.Name = "LblSelInd2";
            LblSelInd2.Size = new Size(4, 18);
            LblSelInd2.TabIndex = 1;
            LblSelInd2.Text = " ";
            // 
            // BtnFinChoiceAP
            // 
            BtnFinChoiceAP.Location = new Point(13, 3);
            BtnFinChoiceAP.Name = "BtnFinChoiceAP";
            BtnFinChoiceAP.Size = new Size(75, 25);
            BtnFinChoiceAP.TabIndex = 2;
            BtnFinChoiceAP.Text = "AP";
            BtnFinChoiceAP.UseVisualStyleBackColor = true;
            // 
            // BtnFinChoiceAR
            // 
            BtnFinChoiceAR.Location = new Point(13, 34);
            BtnFinChoiceAR.Name = "BtnFinChoiceAR";
            BtnFinChoiceAR.Size = new Size(75, 23);
            BtnFinChoiceAR.TabIndex = 3;
            BtnFinChoiceAR.Text = "AR";
            BtnFinChoiceAR.UseVisualStyleBackColor = true;
            // 
            // LblVertSep
            // 
            LblVertSep.BorderStyle = BorderStyle.Fixed3D;
            LblVertSep.Dock = DockStyle.Right;
            LblVertSep.Location = new Point(147, 0);
            LblVertSep.Name = "LblVertSep";
            LblVertSep.Size = new Size(3, 881);
            LblVertSep.TabIndex = 1;
            LblVertSep.Text = "label1";
            // 
            // PicAppLogo
            // 
            PicAppLogo.BackgroundImage = Properties.Resources.fin_con;
            PicAppLogo.Image = Properties.Resources.fin_con;
            PicAppLogo.Location = new Point(15, 5);
            PicAppLogo.Name = "PicAppLogo";
            PicAppLogo.Size = new Size(120, 114);
            PicAppLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            PicAppLogo.TabIndex = 0;
            PicAppLogo.TabStop = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { LblUser_Lbl, LblUser, LblStatus, ProgBar });
            statusStrip1.Location = new Point(150, 854);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1534, 27);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // LblUser_Lbl
            // 
            LblUser_Lbl.Name = "LblUser_Lbl";
            LblUser_Lbl.Size = new Size(36, 22);
            LblUser_Lbl.Text = "User :";
            // 
            // LblUser
            // 
            LblUser.AutoSize = false;
            LblUser.Name = "LblUser";
            LblUser.Size = new Size(200, 22);
            LblUser.Text = "--";
            LblUser.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LblStatus
            // 
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(17, 22);
            LblStatus.Text = "--";
            LblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ProgBar
            // 
            ProgBar.Name = "ProgBar";
            ProgBar.Size = new Size(200, 21);
            // 
            // PnlTop
            // 
            PnlTop.Controls.Add(LblHorSep1);
            PnlTop.Controls.Add(PnlTotals);
            PnlTop.Controls.Add(PnlModRates);
            PnlTop.Controls.Add(PnlItemSearch);
            PnlTop.Controls.Add(LblAppTitle);
            PnlTop.Dock = DockStyle.Top;
            PnlTop.Location = new Point(150, 0);
            PnlTop.Name = "PnlTop";
            PnlTop.Size = new Size(1534, 101);
            PnlTop.TabIndex = 2;
            // 
            // LblHorSep1
            // 
            LblHorSep1.BorderStyle = BorderStyle.Fixed3D;
            LblHorSep1.Dock = DockStyle.Bottom;
            LblHorSep1.Location = new Point(0, 98);
            LblHorSep1.Name = "LblHorSep1";
            LblHorSep1.Size = new Size(1534, 3);
            LblHorSep1.TabIndex = 4;
            LblHorSep1.Text = "label2";
            // 
            // PnlTotals
            // 
            PnlTotals.Controls.Add(LblVertSep2);
            PnlTotals.Controls.Add(TblTotals);
            PnlTotals.Location = new Point(776, 7);
            PnlTotals.Name = "PnlTotals";
            PnlTotals.Size = new Size(902, 87);
            PnlTotals.TabIndex = 3;
            // 
            // LblVertSep2
            // 
            LblVertSep2.BorderStyle = BorderStyle.Fixed3D;
            LblVertSep2.Dock = DockStyle.Left;
            LblVertSep2.Location = new Point(0, 0);
            LblVertSep2.Name = "LblVertSep2";
            LblVertSep2.Size = new Size(3, 87);
            LblVertSep2.TabIndex = 2;
            LblVertSep2.Text = "label1";
            // 
            // TblTotals
            // 
            TblTotals.ColumnCount = 5;
            TblTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.0514565F));
            TblTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.0524826F));
            TblTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.0524826F));
            TblTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.0524826F));
            TblTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15.7910929F));
            TblTotals.Controls.Add(LblCalcAmt, 3, 1);
            TblTotals.Controls.Add(LblCalcAmt_Lbl, 3, 0);
            TblTotals.Controls.Add(LblAmtAfter_Lbl, 2, 0);
            TblTotals.Controls.Add(LblEntity_Lbl, 0, 0);
            TblTotals.Controls.Add(LblAmtBefore_Lbl, 1, 0);
            TblTotals.Controls.Add(LblEntity, 0, 1);
            TblTotals.Controls.Add(LblAmtBefore, 1, 1);
            TblTotals.Controls.Add(LblAmtAfter, 2, 1);
            TblTotals.Controls.Add(LblConPass_Lbl, 4, 0);
            TblTotals.Controls.Add(LblConPass, 4, 1);
            TblTotals.Location = new Point(21, 15);
            TblTotals.Name = "TblTotals";
            TblTotals.RowCount = 2;
            TblTotals.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblTotals.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblTotals.Size = new Size(656, 56);
            TblTotals.TabIndex = 0;
            // 
            // LblCalcAmt
            // 
            LblCalcAmt.AutoSize = true;
            LblCalcAmt.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            LblCalcAmt.Location = new Point(417, 28);
            LblCalcAmt.Name = "LblCalcAmt";
            LblCalcAmt.Size = new Size(22, 21);
            LblCalcAmt.TabIndex = 9;
            LblCalcAmt.Text = "--";
            LblCalcAmt.Visible = false;
            // 
            // LblCalcAmt_Lbl
            // 
            LblCalcAmt_Lbl.AutoSize = true;
            LblCalcAmt_Lbl.Font = new Font("Segoe UI", 12F);
            LblCalcAmt_Lbl.Location = new Point(417, 0);
            LblCalcAmt_Lbl.Name = "LblCalcAmt_Lbl";
            LblCalcAmt_Lbl.Size = new Size(72, 21);
            LblCalcAmt_Lbl.TabIndex = 8;
            LblCalcAmt_Lbl.Text = "Calc Amt";
            LblCalcAmt_Lbl.Visible = false;
            // 
            // LblAmtAfter_Lbl
            // 
            LblAmtAfter_Lbl.AutoSize = true;
            LblAmtAfter_Lbl.Font = new Font("Segoe UI", 12F);
            LblAmtAfter_Lbl.Location = new Point(279, 0);
            LblAmtAfter_Lbl.Name = "LblAmtAfter_Lbl";
            LblAmtAfter_Lbl.Size = new Size(77, 21);
            LblAmtAfter_Lbl.TabIndex = 2;
            LblAmtAfter_Lbl.Text = "Amt After";
            // 
            // LblEntity_Lbl
            // 
            LblEntity_Lbl.AutoSize = true;
            LblEntity_Lbl.Font = new Font("Segoe UI", 12F);
            LblEntity_Lbl.Location = new Point(3, 0);
            LblEntity_Lbl.Name = "LblEntity_Lbl";
            LblEntity_Lbl.Size = new Size(22, 21);
            LblEntity_Lbl.TabIndex = 0;
            LblEntity_Lbl.Text = "--";
            // 
            // LblAmtBefore_Lbl
            // 
            LblAmtBefore_Lbl.AutoSize = true;
            LblAmtBefore_Lbl.Font = new Font("Segoe UI", 12F);
            LblAmtBefore_Lbl.Location = new Point(141, 0);
            LblAmtBefore_Lbl.Name = "LblAmtBefore_Lbl";
            LblAmtBefore_Lbl.Size = new Size(76, 21);
            LblAmtBefore_Lbl.TabIndex = 1;
            LblAmtBefore_Lbl.Text = "Amt Now";
            // 
            // LblEntity
            // 
            LblEntity.AutoSize = true;
            LblEntity.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            LblEntity.Location = new Point(3, 28);
            LblEntity.Name = "LblEntity";
            LblEntity.Size = new Size(22, 21);
            LblEntity.TabIndex = 4;
            LblEntity.Text = "--";
            // 
            // LblAmtBefore
            // 
            LblAmtBefore.AutoSize = true;
            LblAmtBefore.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            LblAmtBefore.Location = new Point(141, 28);
            LblAmtBefore.Name = "LblAmtBefore";
            LblAmtBefore.Size = new Size(22, 21);
            LblAmtBefore.TabIndex = 5;
            LblAmtBefore.Text = "--";
            // 
            // LblAmtAfter
            // 
            LblAmtAfter.AutoSize = true;
            LblAmtAfter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            LblAmtAfter.Location = new Point(279, 28);
            LblAmtAfter.Name = "LblAmtAfter";
            LblAmtAfter.Size = new Size(22, 21);
            LblAmtAfter.TabIndex = 6;
            LblAmtAfter.Text = "--";
            // 
            // LblConPass_Lbl
            // 
            LblConPass_Lbl.AutoSize = true;
            LblConPass_Lbl.Font = new Font("Segoe UI", 12F);
            LblConPass_Lbl.Location = new Point(555, 0);
            LblConPass_Lbl.Name = "LblConPass_Lbl";
            LblConPass_Lbl.Size = new Size(72, 21);
            LblConPass_Lbl.TabIndex = 3;
            LblConPass_Lbl.Text = "Con Pass";
            LblConPass_Lbl.Visible = false;
            // 
            // LblConPass
            // 
            LblConPass.AutoSize = true;
            LblConPass.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            LblConPass.Location = new Point(555, 28);
            LblConPass.Name = "LblConPass";
            LblConPass.Size = new Size(22, 21);
            LblConPass.TabIndex = 7;
            LblConPass.Text = "--";
            LblConPass.Visible = false;
            // 
            // PnlModRates
            // 
            PnlModRates.Controls.Add(LblVertSep1);
            PnlModRates.Controls.Add(tableLayoutPanel1);
            PnlModRates.Location = new Point(615, 7);
            PnlModRates.Name = "PnlModRates";
            PnlModRates.Size = new Size(161, 87);
            PnlModRates.TabIndex = 2;
            // 
            // LblVertSep1
            // 
            LblVertSep1.BorderStyle = BorderStyle.Fixed3D;
            LblVertSep1.Dock = DockStyle.Left;
            LblVertSep1.Location = new Point(0, 0);
            LblVertSep1.Name = "LblVertSep1";
            LblVertSep1.Size = new Size(3, 87);
            LblVertSep1.TabIndex = 1;
            LblVertSep1.Text = "label1";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(BtnRefreshRates, 0, 0);
            tableLayoutPanel1.Controls.Add(BtnApplyCon, 0, 1);
            tableLayoutPanel1.Location = new Point(26, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(0, 3, 0, 0);
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(105, 79);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // BtnRefreshRates
            // 
            BtnRefreshRates.Enabled = false;
            BtnRefreshRates.Location = new Point(3, 6);
            BtnRefreshRates.Name = "BtnRefreshRates";
            BtnRefreshRates.Size = new Size(96, 30);
            BtnRefreshRates.TabIndex = 0;
            BtnRefreshRates.Text = "Refresh Rates";
            BtnRefreshRates.UseVisualStyleBackColor = true;
            // 
            // BtnApplyCon
            // 
            BtnApplyCon.Enabled = false;
            BtnApplyCon.Location = new Point(3, 44);
            BtnApplyCon.Name = "BtnApplyCon";
            BtnApplyCon.Size = new Size(96, 30);
            BtnApplyCon.TabIndex = 1;
            BtnApplyCon.Text = "Apply Con";
            BtnApplyCon.UseVisualStyleBackColor = true;
            // 
            // PnlItemSearch
            // 
            PnlItemSearch.BackColor = SystemColors.Control;
            PnlItemSearch.Controls.Add(TblTripSearchControls);
            PnlItemSearch.Location = new Point(362, 7);
            PnlItemSearch.Name = "PnlItemSearch";
            PnlItemSearch.Size = new Size(253, 87);
            PnlItemSearch.TabIndex = 1;
            PnlItemSearch.Visible = false;
            // 
            // TblTripSearchControls
            // 
            TblTripSearchControls.ColumnCount = 2;
            TblTripSearchControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67.51055F));
            TblTripSearchControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.4894524F));
            TblTripSearchControls.Controls.Add(LblItemSearch_Lbl, 0, 0);
            TblTripSearchControls.Controls.Add(TxtItemSearch, 0, 1);
            TblTripSearchControls.Controls.Add(BtnItemSearch, 1, 1);
            TblTripSearchControls.Location = new Point(10, 12);
            TblTripSearchControls.Name = "TblTripSearchControls";
            TblTripSearchControls.RowCount = 2;
            TblTripSearchControls.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblTripSearchControls.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TblTripSearchControls.Size = new Size(237, 62);
            TblTripSearchControls.TabIndex = 0;
            // 
            // LblItemSearch_Lbl
            // 
            LblItemSearch_Lbl.AutoSize = true;
            LblItemSearch_Lbl.Location = new Point(3, 12);
            LblItemSearch_Lbl.Margin = new Padding(3, 12, 3, 0);
            LblItemSearch_Lbl.Name = "LblItemSearch_Lbl";
            LblItemSearch_Lbl.Size = new Size(17, 15);
            LblItemSearch_Lbl.TabIndex = 0;
            LblItemSearch_Lbl.Text = "--";
            // 
            // TxtItemSearch
            // 
            TxtItemSearch.Location = new Point(3, 34);
            TxtItemSearch.Name = "TxtItemSearch";
            TxtItemSearch.Size = new Size(154, 23);
            TxtItemSearch.TabIndex = 1;
            // 
            // BtnItemSearch
            // 
            BtnItemSearch.Location = new Point(163, 33);
            BtnItemSearch.Margin = new Padding(3, 2, 3, 3);
            BtnItemSearch.Name = "BtnItemSearch";
            BtnItemSearch.Size = new Size(71, 25);
            BtnItemSearch.TabIndex = 3;
            BtnItemSearch.Text = "Search";
            BtnItemSearch.UseVisualStyleBackColor = true;
            // 
            // LblAppTitle
            // 
            LblAppTitle.Font = new Font("Segoe UI", 24F);
            LblAppTitle.Location = new Point(16, 21);
            LblAppTitle.Name = "LblAppTitle";
            LblAppTitle.Size = new Size(350, 50);
            LblAppTitle.TabIndex = 0;
            LblAppTitle.Text = "Fin.Consolidation - AP";
            // 
            // WebView
            // 
            WebView.AllowExternalDrop = true;
            WebView.BackColor = SystemColors.ButtonHighlight;
            WebView.CreationProperties = null;
            WebView.DefaultBackgroundColor = Color.White;
            WebView.Dock = DockStyle.Fill;
            WebView.Location = new Point(150, 101);
            WebView.Name = "WebView";
            WebView.Size = new Size(1534, 753);
            WebView.TabIndex = 3;
            WebView.ZoomFactor = 1D;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Size = new Size(1684, 856);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(1684, 881);
            toolStripContainer1.TabIndex = 5;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // PnlRegion
            // 
            PnlRegion.Controls.Add(RbRegionAu);
            PnlRegion.Controls.Add(RbRegionNz);
            PnlRegion.Location = new Point(6, 216);
            PnlRegion.Name = "PnlRegion";
            PnlRegion.Size = new Size(138, 85);
            PnlRegion.TabIndex = 6;
            // 
            // RbRegionNz
            // 
            RbRegionNz.AutoSize = true;
            RbRegionNz.Location = new Point(49, 21);
            RbRegionNz.Name = "RbRegionNz";
            RbRegionNz.Size = new Size(41, 19);
            RbRegionNz.TabIndex = 0;
            RbRegionNz.TabStop = true;
            RbRegionNz.Text = "NZ";
            RbRegionNz.UseVisualStyleBackColor = true;
            // 
            // RbRegionAu
            // 
            RbRegionAu.AutoSize = true;
            RbRegionAu.Location = new Point(49, 46);
            RbRegionAu.Name = "RbRegionAu";
            RbRegionAu.Size = new Size(41, 19);
            RbRegionAu.TabIndex = 7;
            RbRegionAu.TabStop = true;
            RbRegionAu.Text = "AU";
            RbRegionAu.UseVisualStyleBackColor = true;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1684, 881);
            Controls.Add(WebView);
            Controls.Add(PnlTop);
            Controls.Add(statusStrip1);
            Controls.Add(PnlLeft);
            Controls.Add(toolStripContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Fin.Consolidation";
            WindowState = FormWindowState.Maximized;
            PnlLeft.ResumeLayout(false);
            PnlEnv.ResumeLayout(false);
            PnlEnv.PerformLayout();
            PnlVersion.ResumeLayout(false);
            PnlVersion.PerformLayout();
            TblFinChoiceBtn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicAppLogo).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            PnlTop.ResumeLayout(false);
            PnlTotals.ResumeLayout(false);
            TblTotals.ResumeLayout(false);
            TblTotals.PerformLayout();
            PnlModRates.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            PnlItemSearch.ResumeLayout(false);
            TblTripSearchControls.ResumeLayout(false);
            TblTripSearchControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)WebView).EndInit();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            PnlRegion.ResumeLayout(false);
            PnlRegion.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel PnlLeft;
        private PictureBox PicAppLogo;
        private Label LblVertSep;
        private StatusStrip statusStrip1;
        private Panel PnlTop;
        private Label LblAppTitle;
        private TableLayoutPanel TblFinChoiceBtn;
        private Label LblSelInd2;
        private Label LblSelInd1;
        private Button BtnFinChoiceAP;
        private Button BtnFinChoiceAR;
        private Panel PnlItemSearch;
        private TableLayoutPanel TblTripSearchControls;
        private Label LblItemSearch_Lbl;
        private TextBox TxtItemSearch;
        private Button BtnItemSearch;
        private ToolStripStatusLabel LblUser_Lbl;
        private ToolStripStatusLabel LblUser;
        private ToolStripStatusLabel LblStatus;
        private ToolStripProgressBar ProgBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 WebView;
        private Panel PnlModRates;
        private TableLayoutPanel tableLayoutPanel1;
        private Button BtnRefreshRates;
        private Button BtnApplyCon;
        private Label LblVertSep1;
        private Panel PnlTotals;
        private Label LblVertSep2;
        private TableLayoutPanel TblTotals;
        private Label LblEntity_Lbl;
        private Label LblAmtBefore_Lbl;
        private Label LblConPass_Lbl;
        private Label LblAmtAfter_Lbl;
        private Label LblEntity;
        private Label LblAmtBefore;
        private Label LblAmtAfter;
        private Label LblConPass;
        private Label LblHorSep1;
        private ToolStripContainer toolStripContainer1;
        private Label LblCalcAmt;
        private Label LblCalcAmt_Lbl;
        private Panel PnlVersion;
        private Label LblVersion;
        private Panel PnlEnv;
        private RadioButton RbEnvUat;
        private RadioButton RbEnvLive;
        private Panel PnlRegion;
        private RadioButton RbRegionNz;
        private RadioButton RbRegionAu;
    }
}
