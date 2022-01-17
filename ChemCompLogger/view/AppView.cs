using ChemCompLogger.model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemCompLogger.view
{
    internal class AppView
    {
        // view-model objects/references
        internal AppConsole appConsole;
        private Button[] buttons;
        private Grid buttonGrid;

        /* to avoid magic number usage, but cannot be changed
         * without making modification to MainWindow.xaml */
        private const int buttonCount = 8;

        private MainWindow mainWindow;

        private AppState appState;

        private double initialWinWidth;
        private double initialWinHeight;

        private double initialTop;
        private double initialLeft;

        internal bool AppSnappedToSide
        {
            get; private set;
        }

        internal bool LogPanelShown
        {
            get; private set;
        }

        internal bool ShiftHeld
        {
            get; private set;
        }

        public AppView(MainWindow mw, AppState appState, AppConsole appConsole, Grid buttonGrid)
        {
            mainWindow = mw;
            mainWindow.Width = initialWinWidth = SystemParameters.WorkArea.Width * (3.0D/4.0D);
            mainWindow.Height = initialWinHeight = SystemParameters.WorkArea.Height * (3.0D/4.0D);
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AppSnappedToSide = false;
            LogPanelShown = true;

            this.appState = appState;
            this.appConsole = appConsole;

            mainWindow.SizeChanged += new SizeChangedEventHandler(UpdateTableSizing);
            
            appState.GetDbState().WordBank.Update_WordBank_Latest_Version();

            // retrieve word bank from database
            mw.wordBankDataGrid.ItemsSource = appState.GetDbState().WordBank.Query_Subjective_Analyses();

            this.buttonGrid = buttonGrid;

            // initialize buttons
            buttons = new Button[buttonCount];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new Button();
                buttons[i].Name = $"b{i}";
                buttons[i].Click += ButtonClicked;

                // add button to buttonGrid
                this.buttonGrid.Children.Add(buttons[i]);

                // layout buttons in 2 column layout
                if (i % 2 == 1)
                {
                    Grid.SetColumn(buttons[i], 1);
                }
                if (i > 1)
                {
                    Grid.SetRow(buttons[i], i / 2);
                }
            }

            ToggleLogPanel();
            loadMenu();

            mw.KeyDown -= MainWindow_KeyDown;
            mw.KeyDown += MainWindow_KeyDown;
            mw.KeyUp -= MainWindow_KeyUp;
            mw.KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.IsUp && (e.Key == Key.LeftShift || e.Key == Key.RightShift))
            {
                ShiftHeld = false;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && (e.Key == Key.LeftShift || e.Key == Key.RightShift))
            {
                ShiftHeld = true;
            }
        }

        internal void CloseOnMainMenu()
        {
            if (appState.GetMenuState().GetCurrentPage() == "main_menu" ||
                appState.GetMenuState().GetCurrentPage() == "view_active_doses")
            {
                mainWindow.Close();
            }
        }

        internal void PosOrSizeChanged()
        {
            var windowCloseToHalfWidth = Math.Abs(mainWindow.Width - SystemParameters.WorkArea.Width / 2) < 16;
            var windowCloseToFullHeight = Math.Abs(mainWindow.Height - SystemParameters.WorkArea.Height) < 16;

            if (mainWindow.Left <= 0 && mainWindow.Top < 8 && windowCloseToHalfWidth && windowCloseToFullHeight)
            {
                if (LogPanelShown)
                {
                    ToggleLogPanel();
                }
                AppSnappedToSide = true;
            }
            else if (mainWindow.Left > SystemParameters.WorkArea.Width / 2 - 16 &&
                mainWindow.Top < 8 && windowCloseToHalfWidth && windowCloseToFullHeight)
            {
                if (LogPanelShown)
                {
                    ToggleLogPanel();
                }
                AppSnappedToSide = true;
            }
            else
            {
                AppSnappedToSide = false;
            }

            mainWindow.RepositionPuzzleTitleEditingElements();
        }

        internal void RecordInitialWindowValues()
        {
            initialTop = mainWindow.Top;
            initialLeft = mainWindow.Left;
        }

        internal void PositionInitial()
        {
            mainWindow.Left = initialLeft;
            mainWindow.Top = initialTop;
            mainWindow.Width = initialWinWidth;
            mainWindow.Height = initialWinHeight;
            mainWindow.WindowState = WindowState.Normal;
            if (!LogPanelShown)
            {
                ToggleLogPanel();
            }
        }

        internal void PositionLeft()
        {
            mainWindow.Left = 0;
            mainWindow.Top = 0;
            mainWindow.Width = SystemParameters.WorkArea.Width / 2;
            mainWindow.Height = SystemParameters.WorkArea.Height;
            mainWindow.WindowState = WindowState.Normal;
        }

        internal void PositionRight()
        {
            mainWindow.Left = SystemParameters.WorkArea.Width / 2;
            mainWindow.Top = 0;
            mainWindow.Width = SystemParameters.WorkArea.Width / 2;
            mainWindow.Height = SystemParameters.WorkArea.Height;
            mainWindow.WindowState = WindowState.Normal;
        }

        internal void PositionMax()
        {
            mainWindow.WindowState = WindowState.Maximized;
        }

        internal void ToggleLogPanel()
        {
            if (mainWindow.infoLogPanel.Visibility == Visibility.Visible)
            {
                mainWindow.infoLogPanel.Visibility = Visibility.Hidden;
                mainWindow.MasterGrid.ColumnDefinitions.RemoveAt(1);
                LogPanelShown = false;
            }
            else if (mainWindow.infoLogPanel.Visibility == Visibility.Hidden)
            {
                mainWindow.infoLogPanel.Visibility = Visibility.Visible;
                mainWindow.MasterGrid.ColumnDefinitions.Add(new ColumnDefinition());
                LogPanelShown = true;
            }
            mainWindow.RepositionPuzzleTitleEditingElements();
        }

        private void UpdateTableHeights(double tableHeight)
        {
            mainWindow.wordBankDataGrid.MaxHeight = tableHeight;
            mainWindow.wordBankDataGrid.UpdateLayout();
        }

        private void UpdateTableSizing(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                UpdateTableHeights((e.NewSize.Height - 16.0) / 4.0D * 3.0D - 64);
            }
            e.Handled = true;
        }

        private void loadMenu()
        {
            // for now, write the opening text of first page after loading story
            // TODO: title page format perhaps or special field in metadata object
            appState.GetMenuState().ShowOpeningText();
            UpdateButtons();
        }

        // if a left-panel button is clicked, handle the event
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            // determine Button which triggered the event
            Button button = (Button)sender;
            int buttonNumber = int.Parse(button.Name.Substring(1));

            // if an action is associated w/ the button, this will return 0
            // otw, the page will be changed by this function and the new page
            // number will be returned
            var pageRedirectedTo = appState.GetMenuState().ButtonClicked(buttonNumber);
            if (!string.IsNullOrWhiteSpace(pageRedirectedTo))
            {
                PageChanged(pageRedirectedTo);
                return;
            }

            string action = appState.GetMenuState().ButtonAction(buttonNumber);
            switch (action)
            {
                case "add_word":
                    appState.GetDbState().WordBank.AddNew_WordBank_TableItem();
                    break;

                case "exit_word_bank":
                    var analysesModified = appState.GetDbState().WordBank.CheckModified_WordBank();
                    if (analysesModified)
                    {
                        ChangePage("wordbank_save_discard");
                    }
                    else
                    {
                        ChangePage("main_menu");
                    }
                    break;

                case "save_wordbank":
                    appState.GetDbState().WordBank.WriteChanges_WordBank();
                    ChangePage("main_menu");
                    break;

                case "discard_wordbank":
                    mainWindow.wordBankDataGrid.ItemsSource = appState.GetDbState().WordBank.Query_Subjective_Analyses();
                    ChangePage("main_menu");
                    break;

                case "create_puzzle":
                    appState.GetDbState().WordBank.ClearEmptyWords();
                    if (appState.GetDbState().WordBank.ValidWordCount < 1)
                    {
                        MessageBox.Show("Please enter at least 1 word for the puzzle!");
                    }
                    else
                    {
                        appState.GetDbState().CreatePuzzle(appState, mainWindow.GetPuzzleTitleFromUser());
                    }
                    break;

                case "add_puzzles":
                    if (ShiftHeld)
                    {
                        if (appState.UniquePuzzles == 1)
                        {
                            appState.UniquePuzzles = 10;
                        }
                        else
                        {
                            appState.UniquePuzzles += 10;
                        }
                    }
                    else
                    {
                        appState.UniquePuzzles += 1;
                    }
                    var mb = appState.GetMenuState().GetButton(2);
                    mb.Text = "Add Unique Puzzles\n" + "{" + (appState.UniquePuzzles) + "}";
                    UpdateButtons();
                    break;

                case "remove_puzzles":
                    var mb2 = appState.GetMenuState().GetButton(2);
                    mb2.Text = "Add Unique Puzzles";
                    UpdateButtons();
                    appState.UniquePuzzles = 1;
                    break;

                case "toggle_reverse_words":
                    var mb3 = appState.GetMenuState().GetButton(4);
                    appState.AllowReverseWords = !appState.AllowReverseWords;
                    if (appState.AllowReverseWords)
                    {
                        mb3.Text = "Allow Reverse Words\n(On)";
                    }
                    else
                    {
                        mb3.Text = "Allow Reverse Words\n(Off)";
                    }
                    UpdateButtons();
                    break;

                case "toggle_overlap_percentage":
                    var mb4 = appState.GetMenuState().GetButton(5);
                    appState.OverlapPercentage += 10;
                    if (appState.OverlapPercentage == 110)
                    {
                        appState.OverlapPercentage = 50;
                    }
                    if (appState.OverlapPercentage != 100)
                    {
                        mb4.Text = "Overlap Percentage\n(~" + appState.OverlapPercentage +
                        "%)";
                    }
                    else
                    {
                        mb4.Text = "Overlap Percentage\n(MAX)";
                    }
                    UpdateButtons();
                    break;

                case "toggle_dimensions":
                    if (ShiftHeld)
                    {
                        appState.TogglePuzzleDimensions(true);
                    }
                    else
                    {
                        appState.TogglePuzzleDimensions(false);
                    }
                    var mb5 = appState.GetMenuState().GetButton(6);
                    mb5.Text = "Puzzle Dimensions\n(" +
                        appState.PuzzleDimensions.Item1 + "x" +
                        appState.PuzzleDimensions.Item2 + ")";
                    UpdateButtons();
                    break;

                case "toggle_log_panel":
                    ToggleLogPanel();
                    break;
            }
        }

        private void ChangePage(string page)
        {
            appState.GetMenuState().SetCurrentPage(page);
            PageChanged(page);
        }

        private void PageChanged(string pageOpened)
        {
            switch (pageOpened)
            {
                // hide tableHeader on following pages
                case "main_menu":
                    mainWindow.tableHeader.Visibility = Visibility.Hidden;
                    mainWindow.SetVisibilityPuzzleTitleEditingElements(Visibility.Visible);
                    mainWindow.RepositionPuzzleTitleEditingElements();
                    break;
                default:
                    mainWindow.tableHeader.Visibility = Visibility.Visible;
                    mainWindow.tableHeader.FontSize = 18;
                    break;
            }

            mainWindow.wordBankDataGrid.Visibility = Visibility.Hidden;

            switch (pageOpened)
            {
                case "word_bank":
                    mainWindow.wordBankDataGrid.ItemsSource = appState.GetDbState().WordBank.Words;
                    mainWindow.wordBankDataGrid.Visibility = Visibility.Visible;
                    mainWindow.SetVisibilityPuzzleTitleEditingElements(Visibility.Hidden);
                    mainWindow.tableHeader.Content = "Word Bank";

                    // reset sorting
                    foreach (var col in mainWindow.wordBankDataGrid.Columns)
                    {
                        col.SortDirection = null;
                    }
                    mainWindow.wordBankDataGrid.Columns[0].SortDirection = System.ComponentModel.ListSortDirection.Ascending;

                    mainWindow.wordBankDataGrid.Items.SortDescriptions.Clear();
                    mainWindow.wordBankDataGrid.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription("Word", System.ComponentModel.ListSortDirection.Ascending));
                    break;
            }

            UpdateButtons();
        }

        /* called when a story is first loaded, and whenever a button
        * is clicked while the StoryState is on a given page */
        private void UpdateButtons()
        {
            var displays = appState.GetMenuState().GetButtonDisplays();
            for (int i = 0; i < buttonCount; i++)
            {
                var text = displays[i];
                if (text == null)
                {
                    buttons[i].Visibility = Visibility.Hidden;
                } else
                {
                    string buttonText = text.Trim();
                    string formattedButtonText = "";
                    char[] buttonTextChars = buttonText.ToCharArray();
                    // format buttonText to replace newlines w/ special escape string for XAML line break
                    for (int ci = 0; ci < buttonTextChars.Length; ci++)
                    {
                        if (buttonTextChars[ci] == '\n')
                        {
                            formattedButtonText += Environment.NewLine;
                        }
                        else
                        {
                            formattedButtonText += buttonTextChars[ci];
                        }
                    }
                    buttons[i].Content = formattedButtonText;
                    buttons[i].Visibility = Visibility.Visible;
                }
            }
        }

        internal void AppClosing()
        {
            
        }
    }
}
