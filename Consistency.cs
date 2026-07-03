using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MickeySpeedrunTool.Core;

namespace MickeySpeedrunTool
{
    public class Consistency
    {
        private readonly Color _backgroundColor = ColorTranslator.FromHtml("#1E1E1E");
        private readonly Color _sidebarColor = ColorTranslator.FromHtml("#2A2A2A");
        private readonly Color _textColor = Color.WhiteSmoke;
        private readonly Color _accentColor = Color.Gold;

        public Panel CreateConsistencyPanel()
        {
            var panel = new Panel
            {
                BackColor = _backgroundColor,
                Padding = new Padding(20),
                Dock = DockStyle.Fill,
                AutoScroll = true,
            };

            var header = new Label
            {
                Text = "Consistency Logging",
                ForeColor = _accentColor,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 16),
            };

            var cardPanel = new Panel
            {
                BackColor = _sidebarColor,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(18),
                Margin = new Padding(0, 0, 0, 16),
            };

            var cardHeader = new Label
            {
                Text = "Track Trick Consistency",
                ForeColor = _accentColor,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12),
            };

            var cardLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            var trickDropdown = new ComboBox
            {
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor = _backgroundColor,
                ForeColor = _textColor,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 10),
            };

            var successButton = new Button
            {
                Text = "Succeeded",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Width = 120,
                Margin = new Padding(0, 0, 8, 0),
            };

            var failedButton = new Button
            {
                Text = "Failed",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Width = 120,
                Margin = new Padding(0),
            };

            var successRateLabel = new Label
            {
                Text = "Success Rate: 0%",
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point),
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 0),
            };

            var trickStats = SettingsStore.Current.TrickHistory;

            foreach (var trickName in trickStats.Keys.OrderBy(x => x))
            {
                trickDropdown.Items.Add(trickName);
            }

            void RefreshDropdownSelection(string trickName)
            {
                if (!string.IsNullOrEmpty(trickName) && !trickDropdown.Items.Contains(trickName))
                {
                    trickDropdown.Items.Add(trickName);
                }

                trickDropdown.Text = trickName;
            }

            void UpdateSuccessRate()
            {
                var trickName = trickDropdown.Text.Trim();
                if (string.IsNullOrEmpty(trickName) || !trickStats.ContainsKey(trickName))
                {
                    successRateLabel.Text = "Success Rate: 0%";
                    return;
                }

                var stats = trickStats[trickName];
                int total = stats.SuccessCount + stats.FailCount;
                if (total == 0)
                {
                    successRateLabel.Text = "Success Rate: 0%";
                    return;
                }

                double successPercent = (double)stats.SuccessCount / total * 100;
                successRateLabel.Text = $"Success Rate: {successPercent:F1}%";
            }

            successButton.Click += (_, _) =>
            {
                var trickName = trickDropdown.Text.Trim();
                if (string.IsNullOrEmpty(trickName))
                    return;

                if (!trickStats.ContainsKey(trickName))
                    trickStats[trickName] = new TrickStats();

                var current = trickStats[trickName];
                current.SuccessCount++;
                trickStats[trickName] = current;
                RefreshDropdownSelection(trickName);
                SettingsStore.Save();
                UpdateSuccessRate();
            };

            failedButton.Click += (_, _) =>
            {
                var trickName = trickDropdown.Text.Trim();
                if (string.IsNullOrEmpty(trickName))
                    return;

                if (!trickStats.ContainsKey(trickName))
                    trickStats[trickName] = new TrickStats();

                var current = trickStats[trickName];
                current.FailCount++;
                trickStats[trickName] = current;
                RefreshDropdownSelection(trickName);
                SettingsStore.Save();
                UpdateSuccessRate();
            };

            trickDropdown.SelectedIndexChanged += (_, _) => UpdateSuccessRate();
            trickDropdown.TextChanged += (_, _) => UpdateSuccessRate();

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
            };
            buttonPanel.Controls.Add(successButton);
            buttonPanel.Controls.Add(failedButton);

            cardLayout.Controls.Add(cardHeader);
            cardLayout.Controls.Add(trickDropdown);
            cardLayout.Controls.Add(buttonPanel);
            cardLayout.Controls.Add(successRateLabel);

            cardPanel.Controls.Add(cardLayout);

            panel.Controls.Add(header);
            panel.Controls.Add(cardPanel);

            return panel;
        }
    }
}