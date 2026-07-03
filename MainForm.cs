using System;
using System.Drawing;
using System.Windows.Forms;
using MickeySpeedrunTool.Core;
using MickeySpeedrunTool.UI;

namespace MickeySpeedrunTool
{
    public class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Text = "Mickey Speedrun Tool";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1080, 720);
            MinimumSize = new Size(980, 680);
            BackColor = ColorTranslator.FromHtml("#1E1E1E");
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            var sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = ColorTranslator.FromHtml("#2A2A2A"),
            };

            var sidebarTitle = new Label
            {
                Text = "Mickey Speedrun Tool",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                Height = 56,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var sidebarButtonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
            };

            sidebarPanel.Controls.Add(sidebarButtonsPanel);
            sidebarPanel.Controls.Add(sidebarTitle);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#1E1E1E"),
            };

            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#2A2A2A"),
                Padding = new Padding(16, 0, 16, 0),
            };

            var statusLabels = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Fill,
            };

            statusLabels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            statusLabels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            statusLabels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

            var statusLabel = new Label
            {
                Text = "Status: Ready",
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var gameLabel = new Label
            {
                Text = "Game: Not Detected",
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var ue4ssLabel = new Label
            {
                Text = "UE4SS: Not Installed",
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
            };

            statusLabels.Controls.Add(statusLabel, 0, 0);
            statusLabels.Controls.Add(gameLabel, 1, 0);
            statusLabels.Controls.Add(ue4ssLabel, 2, 0);

            statusPanel.Controls.Add(statusLabels);

            Controls.Add(mainPanel);
            Controls.Add(sidebarPanel);
            Controls.Add(statusPanel);

            SettingsStore.Initialize();
            SaveSwapper.Initialize();

            var uiManager = new UIManager(
                sidebarButtonsPanel,
                mainPanel,
                statusPanel,
                statusLabel,
                gameLabel,
                ue4ssLabel
            );

            uiManager.Initialize();

            BackImages.ApplyRandomBackground(this);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            SuspendLayout();
            // 
            // MainForm
            // 
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(1264, 681);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Epic Mickey Speedrunning Tool";
            ResumeLayout(false);
        }
    }
}