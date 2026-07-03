using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MickeySpeedrunTool.Core;

namespace MickeySpeedrunTool.UI
{
    internal enum AppTab
    {
        Dashboard,
        Saves,
        UE4SS,
        Consistency
    }

    internal sealed class UIManager
    {
        private readonly Panel _sidebarButtonsContainer;
        private readonly Panel _mainPanel;
        private readonly Panel _statusPanel;
        private readonly Label _statusLabel;
        private readonly Label _gameLabel;
        private readonly Label _ue4ssLabel;
        private readonly Dictionary<AppTab, Button> _tabButtons = new();
        private readonly Dictionary<AppTab, Panel> _contentPanels = new();
        private readonly GameManager _gameManager = new();
        private readonly Color _sidebarColor = ColorTranslator.FromHtml("#2A2A2A");
        private readonly Color _backgroundColor = ColorTranslator.FromHtml("#1E1E1E");
        private readonly Color _accentColor = Color.Gold;
        private readonly Color _textColor = Color.WhiteSmoke;

        public UIManager(Panel sidebarButtonsContainer, Panel mainPanel, Panel statusPanel, Label statusLabel, Label gameLabel, Label ue4ssLabel)
        {
            _sidebarButtonsContainer = sidebarButtonsContainer;
            _mainPanel = mainPanel;
            _statusPanel = statusPanel;
            _statusLabel = statusLabel;
            _gameLabel = gameLabel;
            _ue4ssLabel = ue4ssLabel;
        }

        public void Initialize()
        {
            BuildSidebar();
            BuildContentPanels();
            RefreshStatus();
            SelectTab(AppTab.Dashboard);
        }

        private string GetUe4ssTargetPath()
        {
            if (!string.IsNullOrWhiteSpace(SettingsStore.Current.Ue4ssTargetPath))
            {
                return SettingsStore.Current.Ue4ssTargetPath;
            }

            return UE4SSInstaller.GetRecommendedTargetPath(SettingsStore.Current.GameExePath);
        }

        private void BuildSidebar()
        {
            _sidebarButtonsContainer.BackColor = _sidebarColor;
            _sidebarButtonsContainer.Padding = new Padding(0, 8, 0, 0);
            _sidebarButtonsContainer.Controls.Clear();
            AddSidebarButton("Dashboard", AppTab.Dashboard);
            AddSidebarButton("Saves", AppTab.Saves);
            AddSidebarButton("UE4SS", AppTab.UE4SS);
            AddSidebarButton("Consistency", AppTab.Consistency);
        }

        private void AddSidebarButton(string text, AppTab tab)
        {
            var button = new Button
            {
                Text = text,
                ForeColor = _textColor,
                BackColor = _sidebarColor,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Height = 44,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 0, 0, 0),
            };
            button.Click += (_, _) => SelectTab(tab);
            _sidebarButtonsContainer.Controls.Add(button);
            _sidebarButtonsContainer.Controls.SetChildIndex(button, 0);
            _tabButtons[tab] = button;
        }

        private void SelectTab(AppTab tab)
        {
            if (_contentPanels.Count == 0 || !_tabButtons.ContainsKey(tab))
                return;
            foreach (var pair in _tabButtons)
            {
                pair.Value.BackColor = pair.Key == tab ? Color.FromArgb(50, _accentColor) : _sidebarColor;
                pair.Value.ForeColor = pair.Key == tab ? Color.White : _textColor;
            }
            foreach (var content in _contentPanels.Values)
                content.Visible = false;
            _contentPanels[tab].Visible = true;
            _contentPanels[tab].BringToFront();
        }

        private void BuildContentPanels()
        {
            _mainPanel.BackColor = _backgroundColor;
            _mainPanel.Controls.Clear();
            _contentPanels.Clear();
            AddPagePanel(AppTab.Dashboard, CreateDashboardPanel());
            AddPagePanel(AppTab.Saves, CreateSavesPanel());
            AddPagePanel(AppTab.UE4SS, CreateUe4ssPanel());
            AddPagePanel(AppTab.Consistency, CreateConsistencyPanel());
        }

        private void AddPagePanel(AppTab tab, Panel panel)
        {
            panel.Dock = DockStyle.Fill;
            panel.Visible = false;
            _mainPanel.Controls.Add(panel);
            _contentPanels[tab] = panel;
        }

        private Panel CreateDashboardPanel()
        {
            var panel = new Panel
            {
                BackColor = _backgroundColor,
                Padding = new Padding(20),
                AutoScroll = true,
            };

            var header = CreateSectionHeader("Dashboard");
            header.Dock = DockStyle.Top;
            header.Height = 42;

            var actionCard = new Panel
            {
                BackColor = _sidebarColor,
                Padding = new Padding(16),
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            var actionLabel = new Label
            {
                Text = "Actions",
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(0, 0, 0, 6),
            };

            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 0),
            };
            actionPanel.Controls.Add(CreateActionButton("Install UE4SS", OnInstallUe4ss));
            actionPanel.Controls.Add(CreateActionButton("Locate Game", OnLocateGame));
            actionPanel.Controls.Add(CreateActionButton("Launch Game", OnLaunchGame));

            actionCard.Controls.Add(actionPanel);
            actionCard.Controls.Add(actionLabel);

            panel.Controls.Add(actionCard);
            panel.Controls.Add(header);
            return panel;
        }

        private Panel CreateSavesPanel()
        {
            var panel = new Panel
            {
                BackColor = _backgroundColor,
                Padding = new Padding(20),
                AutoScroll = true,
            };

            var header = CreateSectionHeader("Saves");

            var savePathTextbox = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(SaveSwapper.GameSaveFolder) ? "Not configured" : SaveSwapper.GameSaveFolder,
                ReadOnly = true,
                BackColor = _backgroundColor,
                ForeColor = _textColor,
                Width = 540,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var browseSaveFolderButton = CreateActionButton("Choose Save Folder", (_, _) =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select your game's SaveGames folder",
                    ShowNewFolderButton = false,
                    SelectedPath = string.IsNullOrWhiteSpace(SaveSwapper.GameSaveFolder)
                        ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                        : SaveSwapper.GameSaveFolder,
                };
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;
                if (SaveSwapper.TryConfigureManualSavePath(dialog.SelectedPath))
                {
                    SettingsStore.Current.GameSavePath = dialog.SelectedPath;
                    SettingsStore.Save();
                    savePathTextbox.Text = dialog.SelectedPath;
                    RefreshStatus();
                }
                else
                {
                    MessageBox.Show("Selected folder does not contain valid .sav files. Please choose the game SaveGames folder.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });

            var saveFolderRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 16),
            };

            saveFolderRow.Controls.Add(savePathTextbox);
            saveFolderRow.Controls.Add(browseSaveFolderButton);

            var categoryRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 12),
            };

            categoryRow.Controls.Add(new Label
            {
                Text = "Category:",
                ForeColor = _textColor,
                AutoSize = true,
                Padding = new Padding(0, 7, 8, 0),
            });

            var categoryCombo = new ComboBox
            {
                Width = 220,
                BackColor = _backgroundColor,
                ForeColor = _textColor,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            var categories = GetSaveFileCategories().ToList();
            if (categories.Count == 0)
            {
                categoryCombo.Items.Add("No save categories found");
                categoryCombo.SelectedIndex = 0;
                categoryCombo.Enabled = false;
            }
            else
            {
                foreach (var category in categories)
                {
                    categoryCombo.Items.Add(category);
                }

                categoryCombo.SelectedIndex = 0;
            }

            categoryRow.Controls.Add(categoryCombo);

            var areaRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 12),
                Visible = false,
            };

            areaRow.Controls.Add(new Label
            {
                Text = "Area:",
                ForeColor = _textColor,
                AutoSize = true,
                Padding = new Padding(0, 7, 8, 0),
            });

            var areaCombo = new ComboBox
            {
                Width = 320,
                BackColor = _backgroundColor,
                ForeColor = _textColor,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            areaRow.Controls.Add(areaCombo);

            var browserHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 4, 0, 0),
                BackColor = Color.Transparent,
            };

            void RenderBrowser()
            {
                browserHost.Controls.Clear();
                var categoryName = categoryCombo.SelectedItem?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(categoryName) || categoryName == "No save categories found")
                {
                    browserHost.Controls.Add(CreateEmptyStateLabel("No bundled save categories were detected. Add save folders under 'Save Files'."));
                    return;
                }

                if (string.Equals(categoryName, "IL's", StringComparison.OrdinalIgnoreCase))
                {
                    browserHost.Controls.Add(BuildSaveBrowserForCategory(categoryName));
                    return;
                }

                var selectedArea = areaCombo.SelectedItem?.ToString();
                browserHost.Controls.Add(BuildSaveBrowserForCategory(categoryName, selectedArea));
            }

            void RenderCategory()
            {
                browserHost.Controls.Clear();
                var categoryName = categoryCombo.SelectedItem?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(categoryName) || categoryName == "No save categories found")
                {
                    areaRow.Visible = false;
                    browserHost.Controls.Add(CreateEmptyStateLabel("No bundled save categories were detected. Add save folders under 'Save Files'."));
                    return;
                }

                if (string.Equals(categoryName, "IL's", StringComparison.OrdinalIgnoreCase))
                {
                    areaRow.Visible = false;
                    browserHost.Controls.Add(BuildSaveBrowserForCategory(categoryName));
                    return;
                }

                var areas = GetCategoryAreaItems(categoryName).ToList();
                if (areas.Count == 0)
                {
                    areaRow.Visible = false;
                    browserHost.Controls.Add(CreateEmptyStateLabel("No area folders found for this category."));
                    return;
                }

                areaCombo.Items.Clear();
                foreach (var area in areas)
                {
                    areaCombo.Items.Add(area.Name);
                }

                areaCombo.SelectedIndex = 0;
                areaRow.Visible = true;
                RenderBrowser();
            }

            categoryCombo.SelectedIndexChanged += (_, _) => RenderCategory();
            areaCombo.SelectedIndexChanged += (_, _) => RenderBrowser();
            RenderCategory();

            panel.Controls.Add(browserHost);
            panel.Controls.Add(areaRow);
            panel.Controls.Add(categoryRow);
            panel.Controls.Add(saveFolderRow);
            panel.Controls.Add(header);
            return panel;
        }

        private Panel BuildSaveBrowserForCategory(string categoryName, string? selectedArea = null)
        {
            var browserPanel = new Panel
            {
                BackColor = _sidebarColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                AutoScroll = true,
            };

            var rootPath = GetCategoryRootPath(categoryName);
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                browserPanel.Controls.Add(CreateEmptyStateLabel("No save files found for this category."));
                return browserPanel;
            }

            if (string.Equals(categoryName, "IL's", StringComparison.OrdinalIgnoreCase))
            {
                var ilContent = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoScroll = true,
                    Padding = new Padding(0, 4, 0, 0),
                };

                var ilSubfolders = Directory.GetDirectories(rootPath)
                    .Where(dir => Directory.GetFiles(dir, "*.sav", SearchOption.AllDirectories).Length > 0)
                    .OrderBy(Path.GetFileName)
                    .ToList();

                foreach (var ilFolder in ilSubfolders)
                {
                    ilContent.Controls.Add(CreateLeafSaveRow(Path.GetFileName(ilFolder), ilFolder));
                }

                if (ilContent.Controls.Count == 0)
                {
                    ilContent.Controls.Add(CreateEmptyStateLabel("No IL save files found."));
                }

                browserPanel.Controls.Add(ilContent);
                return browserPanel;
            }

            var areaItems = GetCategoryAreaItems(categoryName).ToList();
            if (areaItems.Count == 0)
            {
                browserPanel.Controls.Add(CreateEmptyStateLabel("No area folders found for this category."));
                return browserPanel;
            }

            var selectedAreaEntry = areaItems.FirstOrDefault(a => a.Name == selectedArea);
            var selectedAreaPath = !string.IsNullOrWhiteSpace(selectedAreaEntry.Name)
                ? selectedAreaEntry.Path
                : areaItems[0].Path;
            var areaBrowser = BuildSaveBrowserForArea(selectedAreaPath);
            browserPanel.Controls.Add(areaBrowser);
            return browserPanel;
        }

        private static (string Name, string Path)[] GetCategoryAreaItems(string categoryName)
        {
            var saveRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Save Files");
            var categoryPath = Path.Combine(saveRoot, categoryName);
            if (!Directory.Exists(categoryPath))
            {
                return Array.Empty<(string, string)>();
            }

            var candidates = Directory.GetDirectories(categoryPath, "*", SearchOption.TopDirectoryOnly)
                .Where(dir => Directory.GetFiles(dir, "*.sav", SearchOption.AllDirectories).Length > 0)
                .Select(dir => (Name: Path.GetFileName(dir) ?? string.Empty, Path: dir))
                .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                .OrderBy(item => item.Name)
                .ToArray();

            if (candidates.Length == 0 && Directory.GetFiles(categoryPath, "*.sav", SearchOption.TopDirectoryOnly).Length > 0)
            {
                return new[] { (Name: "Root Save Files", Path: categoryPath) };
            }

            return candidates;
        }

        private Control BuildSaveBrowserForArea(string areaPath)
        {
            var areaPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoScroll = true,
                Padding = new Padding(0, 4, 0, 0),
            };

            var saveFolders = Directory.GetDirectories(areaPath, "*", SearchOption.AllDirectories)
                .Where(dir => Directory.GetFiles(dir, "*.sav", SearchOption.AllDirectories).Length > 0)
                .OrderBy(Path.GetFileName)
                .ToList();

            if (Directory.GetFiles(areaPath, "*.sav", SearchOption.TopDirectoryOnly).Length > 0)
            {
                saveFolders.Insert(0, areaPath);
            }

            foreach (var saveFolder in saveFolders)
            {
                var displayName = saveFolder.Equals(areaPath, StringComparison.OrdinalIgnoreCase)
                    ? Path.GetFileName(areaPath)
                    : Path.GetRelativePath(areaPath, saveFolder);

                areaPanel.Controls.Add(CreateLeafSaveRow(displayName, saveFolder, 1));
            }

            if (areaPanel.Controls.Count == 0)
            {
                areaPanel.Controls.Add(CreateEmptyStateLabel("No save files found for this area."));
            }

            return areaPanel;
        }

        private Control CreateExpandableAreaNode(DirectoryInfo directory, int depth)
        {
            var hasChildDirectories = directory.GetDirectories().Length > 0;
            var hasSaveFiles = directory.GetFiles("*.sav").Length > 0;

            if (!hasChildDirectories)
            {
                return CreateLeafSaveRow(directory.Name, directory.FullName, depth);
            }

            var nodePanel = new Panel
            {
                BackColor = Color.FromArgb(40, _backgroundColor),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 8),
                Margin = new Padding(0, 0, 0, 8),
            };

            var headerRow = new Panel
            {
                BackColor = Color.FromArgb(34, _sidebarColor),
                Height = 42,
                Dock = DockStyle.Top,
                Padding = new Padding(8 + (depth * 18), 6, 8, 6),
            };

            var expandedByDefault = depth == 0;

            var toggleButton = new Button
            {
                Text = expandedByDefault ? "-" : "+",
                Width = 30,
                Dock = DockStyle.Left,
                FlatStyle = FlatStyle.Flat,
                ForeColor = _textColor,
                BackColor = _sidebarColor,
                Margin = new Padding(0),
            };
            toggleButton.FlatAppearance.BorderSize = 0;

            var titleLabel = new Label
            {
                Text = hasSaveFiles ? directory.Name : $"{directory.Name} (empty)",
                ForeColor = _textColor,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 6, 0, 0),
            };

            headerRow.Controls.Add(titleLabel);
            headerRow.Controls.Add(toggleButton);

            var childrenHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoScroll = true,
                Visible = expandedByDefault,
                Padding = new Padding(0, 8, 0, 0),
            };

            foreach (var childDirectory in directory.GetDirectories().OrderBy(d => d.Name))
            {
                childrenHost.Controls.Add(CreateExpandableAreaNode(childDirectory, depth + 1));
            }

            if (hasSaveFiles)
            {
                childrenHost.Controls.Add(CreateLeafSaveRow(directory.Name, directory.FullName, depth + 1));
            }

            toggleButton.Click += (_, _) =>
            {
                childrenHost.Visible = !childrenHost.Visible;
                toggleButton.Text = childrenHost.Visible ? "-" : "+";
            };

            nodePanel.Controls.Add(childrenHost);
            nodePanel.Controls.Add(headerRow);
            return nodePanel;
        }

        private Control CreateLeafSaveRow(string displayName, string folderPath, int depth = 0)
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(30, _backgroundColor),
                Padding = new Padding(16 + (depth * 18), 8, 16, 8),
                Margin = new Padding(0, 0, 0, 6),
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

            var title = new Label
            {
                Text = displayName,
                ForeColor = _textColor,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var loadButton = CreateAccentButton("Load", (_, _) =>
            {
                if (SaveSwapper.LoadPresetFromFolder(folderPath, out var message))
                {
                    _statusLabel.Text = $"Status: {message}";
                }
                else
                {
                    MessageBox.Show(message, "Load Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
            loadButton.Width = 90;
            loadButton.Margin = new Padding(0, 0, 0, 0);

            row.Controls.Add(title, 0, 0);
            row.Controls.Add(loadButton, 1, 0);
            return row;
        }

        private Label CreateEmptyStateLabel(string message)
        {
            return new Label
            {
                Text = message,
                ForeColor = _textColor,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(8, 6, 0, 0),
            };
        }

        private string GetCategoryRootPath(string categoryName)
        {
            var saveRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Save Files");
            var categoryPath = Path.Combine(saveRoot, categoryName);
            if (Directory.Exists(categoryPath))
            {
                return categoryPath;
            }

            return string.Empty;
        }

        private IEnumerable<string> GetSaveFileCategories()
        {
            var saveRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Save Files");
            if (!Directory.Exists(saveRoot))
            {
                return Array.Empty<string>();
            }

            return Directory.GetDirectories(saveRoot)
                .Select(path => Path.GetFileName(path) ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name)
                .ToList();
        }

        private static string FindFolderWithSaveFiles(string rootPath)
        {
            if (Directory.GetFiles(rootPath, "*.sav").Length > 0)
            {
                return rootPath;
            }

            foreach (var directory in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFiles(directory, "*.sav").Length > 0)
                {
                    return directory;
                }
            }

            return rootPath;
        }

        private Panel CreateUe4ssPanel()
        {
            var panel = new Panel { BackColor = _backgroundColor, Padding = new Padding(20) };
            var header = CreateSectionHeader("UE4SS Installer");
            
            var installerPanel = new Panel
            {
                BackColor = _sidebarColor,
                Dock = DockStyle.Top,
                Padding = new Padding(16),
                Height = 160,
            };

            var pathLabel = new Label
            {
                Text = "Target Path:",
                ForeColor = _textColor,
                Dock = DockStyle.Top,
                Height = 20,
            };

            var pathRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0),
            };

            var pathTextbox = new TextBox
            {
                Width = 540,
                BackColor = _backgroundColor,
                ForeColor = _textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Text = GetUe4ssTargetPath(),
            };

            pathRow.Controls.Add(pathTextbox);
            pathRow.Controls.Add(CreateActionButton("Browse", (_, _) =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select the UE4SS target installation folder.",
                    ShowNewFolderButton = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTextbox.Text = dialog.SelectedPath;
                    SettingsStore.Current.Ue4ssTargetPath = dialog.SelectedPath;
                    SettingsStore.Save();
                    RefreshStatus();
                }
            }));

            var installPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 0),
            };

            var installStatusLabel = new Label
            {
                Text = UE4SSInstaller.IsInstalled(pathTextbox.Text) ? "Status: Installed" : "Status: Not Installed",
                ForeColor = _textColor,
                Dock = DockStyle.Top,
                Height = 24,
                Padding = new Padding(0, 12, 0, 0),
            };

            installPanel.Controls.Add(CreateAccentButton("Install UE4SS", (_, _) =>
            {
                var targetPath = pathTextbox.Text.Trim();
                var sourcePath = UE4SSInstaller.SourcePath;

                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    MessageBox.Show("Please select a valid installation target folder.", "UE4SS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!UE4SSInstaller.SourceExists)
                {
                    MessageBox.Show("UE4SS source files not found in application folder.", "UE4SS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var success = TryPerformUe4ssAction(() =>
                {
                    var installed = UE4SSInstaller.Install(targetPath, sourcePath, out var installMessage);
                    return (installed, installMessage);
                }, out var message);

                if (success)
                {
                    SettingsStore.Current.Ue4ssTargetPath = targetPath;
                    SettingsStore.Save();
                    RefreshStatus();
                    installStatusLabel.Text = "Status: Installed";
                }
                else
                {
                    installStatusLabel.Text = "Status: Installation failed";
                }

                MessageBox.Show(message, "UE4SS", success ? MessageBoxButtons.OK : MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }));

            installPanel.Controls.Add(CreateDangerButton("Uninstall UE4SS", (_, _) =>
            {
                var targetPath = pathTextbox.Text.Trim();

                if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
                {
                    MessageBox.Show("Please select a valid installation target folder.", "UE4SS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var success = TryPerformUe4ssAction(() =>
                {
                    var uninstalled = UE4SSInstaller.Uninstall(targetPath, out var uninstallMessage);
                    return (uninstalled, uninstallMessage);
                }, out var message);

                if (success)
                {
                    RefreshStatus();
                    installStatusLabel.Text = "Status: Not Installed";
                }
                else
                {
                    installStatusLabel.Text = "Status: Uninstall failed";
                }

                MessageBox.Show(message, "UE4SS", success ? MessageBoxButtons.OK : MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }));

            var panelContainer = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
            };

            panelContainer.Controls.Add(installStatusLabel);
            panelContainer.Controls.Add(pathRow);
            installPanel.Dock = DockStyle.Top;
            panel.Controls.Add(installPanel);
            panel.Controls.Add(panelContainer);
            panel.Controls.Add(header);
            return panel;
        }

        private Panel CreateDebugPanel()
        {
            var panel = new Panel { BackColor = _backgroundColor, Padding = new Padding(20) };
            var header = CreateSectionHeader("Debug");

            var debugGroup = new GroupBox
            {
                Text = "Debug",
                ForeColor = _textColor,
                BackColor = _sidebarColor,
                Dock = DockStyle.Top,
                Padding = new Padding(12),
                Height = 220,
            };

            var message = new Label
            {
                Text = "Debug options and info appear here.",
                ForeColor = _textColor,
                Dock = DockStyle.Top,
                Height = 24,
                Padding = new Padding(0, 6, 0, 0),
            };

            debugGroup.Controls.Add(message);
            panel.Controls.Add(debugGroup);
            panel.Controls.Add(header);
            return panel;
        }

        private Label CreateSectionHeader(string title)
        {
            return new Label
            {
                Text = title,
                ForeColor = _accentColor,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                Dock = DockStyle.Top,
                Height = 42,
            };
        }

        private CheckBox CreateOptionCheckBox(string text)
        {
            return new CheckBox
            {
                Text = text,
                ForeColor = _textColor,
                BackColor = _sidebarColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                AutoSize = true,
                Padding = new Padding(4),
            };
        }

        private Panel CreateSaveStateRow(string name)
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 2, 0, 2),
            };

            var nameLabel = new Label
            {
                Text = name,
                ForeColor = _textColor,
                Width = 240,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 6, 0, 0),
            };

            var saveButton = CreateActionButton("Save", (_, _) =>
            {
                if (SaveSwapper.SaveState(name, out var message))
                {
                    _statusLabel.Text = $"Status: {message}";
                }
                else
                {
                    MessageBox.Show(message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });

            var loadButton = CreateActionButton("Load", (_, _) =>
            {
                bool success = SaveSwapper.LoadState(name, out var message);
                if (success)
                {
                    _statusLabel.Text = $"Status: {message}";
                }
                else
                {
                    MessageBox.Show(message, "Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });

            row.Controls.Add(nameLabel);
            row.Controls.Add(saveButton);
            row.Controls.Add(loadButton);
            return row;
        }

        private Button CreateActionButton(string text, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                BackColor = _sidebarColor,
                ForeColor = _textColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(0, 0, 12, 0),
                AutoSize = true,
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(80, Color.White);
            button.Click += onClick;
            return button;
        }

        private Button CreateAccentButton(string text, EventHandler onClick)
        {
            var button = CreateActionButton(text, onClick);
            button.BackColor = _accentColor;
            button.ForeColor = Color.Black;
            return button;
        }

        private Button CreateDangerButton(string text, EventHandler onClick)
        {
            var button = CreateActionButton(text, onClick);
            button.BackColor = Color.Red;
            button.ForeColor = Color.White;
            return button;
        }

        private Panel CreatePlaceholderPanel(string title)
        {
            var panel = new Panel { BackColor = _backgroundColor, Padding = new Padding(20) };

            var titleLabel = new Label
            {
                Text = title,
                ForeColor = _accentColor,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                Dock = DockStyle.Top,
                Height = 42,
            };

            var messageLabel = new Label
            {
                Text = $"{title} content will appear here.",
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 12, 0, 0),
            };

            panel.Controls.Add(messageLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        private void UpdateStatus(string status, string game, string ue4ss)
        {
            _statusLabel.Text = $"Status: {status}";
            _gameLabel.Text = $"Game: {game}";
            _ue4ssLabel.Text = $"UE4SS: {ue4ss}";
        }

        private void RefreshStatus()
        {
            bool gameDetected = _gameManager.DetectGame();
            string gameState = gameDetected ? "Detected" : "Not Detected";
            string ue4ssState = UE4SSInstaller.IsInstalled(GetUe4ssTargetPath())
                ? "Installed"
                : "Not Installed";
            string saveState = string.IsNullOrWhiteSpace(SaveSwapper.GameSaveFolder)
                ? "Save folder not configured"
                : "Save folder configured";
            UpdateStatus(saveState, gameState, ue4ssState);
        }

        private Label CreateStatusLabel(string title, string value)
        {
            return new Label
            {
                Text = $"{title}: {value}",
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4),
            };
        }

        private void OnInstallUe4ss(object? sender, EventArgs e)
        {
            var targetPath = GetUe4ssTargetPath();
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UE4SS");

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("UE4SS source files not found in application folder.", "UE4SS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UE4SSInstaller.Install(targetPath, sourcePath))
            {
                SettingsStore.Current.Ue4ssTargetPath = targetPath;
                SettingsStore.Save();
                RefreshStatus();
            }
            else
            {
                MessageBox.Show("UE4SS installation failed.", "UE4SS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnLocateGame(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Select Game Executable",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            SettingsStore.Current.GameExePath = dialog.FileName;
            SettingsStore.Save();
            RefreshStatus();
        }

        private void OnLaunchGame(object? sender, EventArgs e)
        {
            string? exePath = SettingsStore.Current.GameExePath;
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show("Please locate the game executable first.", "Launch Game", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _gameManager.LaunchGame(exePath, out var message);
                RefreshStatus();
                _statusLabel.Text = $"Status: {message}";
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Launch Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryPerformUe4ssAction(Func<(bool success, string message)> action, out string message)
        {
            var wasRunning = _gameManager.IsGameRunning;
            var gameExePath = _gameManager.GamePath;
            if (wasRunning && string.IsNullOrWhiteSpace(gameExePath))
            {
                gameExePath = SettingsStore.Current.GameExePath;
            }

            if (wasRunning)
            {
                if (!_gameManager.CloseGame(out var closeMessage))
                {
                    message = $"Could not close the game before UE4SS action: {closeMessage}";
                    return false;
                }
            }

            var (success, actionMessage) = action();
            message = actionMessage;

            if (wasRunning && !string.IsNullOrWhiteSpace(gameExePath))
            {
                if (!_gameManager.LaunchGame(gameExePath, out var launchMessage))
                {
                    message = success
                        ? $"{actionMessage} Game action completed, but failed to relaunch the game: {launchMessage}."
                        : $"{actionMessage} Additionally, failed to relaunch the game: {launchMessage}.";
                    return false;
                }

                message = success
                    ? $"{actionMessage} Game was relaunched successfully."
                    : $"{actionMessage} Game was relaunched after failure.";
            }

            return success;
        }

        private void OnOpenDebug(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = true
            });
        }

        private Panel CreateConsistencyPanel()
        {
            var consistency = new Consistency();
            return consistency.CreateConsistencyPanel();
        }
    }
}