using ChemCompLogger.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ChemCompLogger.model.DbState_WordBank;

namespace ChemCompLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private AppState appState;

        view.AppConsole appConsole;
        view.AppView appView;

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "Word Search Maker";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            appConsole = new view.AppConsole(ref infoLogPanel);
            appState = new AppState(appConsole);
            appView = new view.AppView(this, appState, appConsole, ButtonGrid);

            // add listener for key presses
            this.PreviewKeyDown += new KeyEventHandler(HandleKeys);

            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;

            // https://stackoverflow.com/questions/1127647/convert-system-drawing-icon-to-system-media-imagesource
            Bitmap bitmap = Properties.Resources.nootracker_icon_small_shadow.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource nootrackerIconBitMap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            this.Icon = nootrackerIconBitMap;

            wordBankDataGrid.PreparingCellForEdit -= WordBankDataGrid_PreparingCellForEdit;
            wordBankDataGrid.PreparingCellForEdit += WordBankDataGrid_PreparingCellForEdit;
        }

        private void WordBankDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var fe = wordBankDataGrid.Columns[0].GetCellContent(e.Row);
            var word = (Word)fe.DataContext;
            word.ResetText();
        }

        protected override void OnRender(DrawingContext obj)
        {
            base.OnRender(obj);
            appView.PosOrSizeChanged();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            appView.PosOrSizeChanged();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            appView.PosOrSizeChanged();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            appView.RecordInitialWindowValues();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            appView.AppClosing();
        }

        // KEYPRESSES
        private void HandleKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                appView.CloseOnMainMenu();
            }
            else if (e.Key == Key.F1)
            {
                appView.PositionInitial();
            }
            else if (e.Key == Key.F2)
            {
                appView.PositionLeft();
            }
            else if (e.Key == Key.F3)
            {
                appView.PositionRight();
            }
            else if (e.Key == Key.F4)
            {
                appView.PositionMax();
            }
        }

        private void WordBank_Delete(object sender, RoutedEventArgs e)
        {
            var menuItemClicked = sender as MenuItem;
            if (menuItemClicked == null) return;

            var contextMenu = menuItemClicked.Parent as ContextMenu;
            if (contextMenu == null) return;

            var clickedDataGrid = contextMenu.PlacementTarget as DataGrid;
            clickedDataGrid.CancelEdit();

            if (MessageBox.Show("Delete Word?", "Alert", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                appState.GetDbState().WordBank.DeleteWord(clickedDataGrid.SelectedItems);
            }

            wordBankDataGrid.UpdateLayout();
        }

        public void RepositionPuzzleTitleEditingElements()
        {
            // hard-coded button grid offset in master grid
            var buttonGrid = this.MasterGrid.Children[0];
            buttonGrid.UpdateLayout();

            var marginOfPuzzleTitleTextBox = PuzzleTitleTextBox.Margin;
            marginOfPuzzleTitleTextBox.Top = buttonGrid.RenderSize.Height + 8;
            PuzzleTitleTextBox.Margin = marginOfPuzzleTitleTextBox;
            var widthForPuzzleTitleTextBox = buttonGrid.RenderSize.Width - marginOfPuzzleTitleTextBox.Left;
            if (widthForPuzzleTitleTextBox > 24)
            {
                PuzzleTitleTextBox.Width = widthForPuzzleTitleTextBox;
                PuzzleTitleTextBox.UpdateLayout();
                PuzzleTitleTextBox.InvalidateVisual();
            }

            var marginOfPuzzleTitleLabel = PuzzleTitleLabel.Margin;
            marginOfPuzzleTitleLabel.Top = buttonGrid.RenderSize.Height + 8;
            PuzzleTitleLabel.Margin = marginOfPuzzleTitleLabel;
            PuzzleTitleLabel.UpdateLayout();
            PuzzleTitleLabel.InvalidateVisual();
        }

        public void SetVisibilityPuzzleTitleEditingElements(Visibility visibility)
        {
            var buttonGrid = (FrameworkElement)this.MasterGrid.Children[0];
            var buttonGridMargin = buttonGrid.Margin;
            if (visibility == Visibility.Hidden)
            {
                buttonGridMargin.Bottom = 8;
                buttonGrid.Margin = buttonGridMargin;
            }
            else
            {
                buttonGridMargin.Bottom = 32;
                buttonGrid.Margin = buttonGridMargin;
            }
            PuzzleTitleTextBox.Visibility = visibility;
            PuzzleTitleLabel.Visibility = visibility;
            PuzzleTitleTextBox.UpdateLayout();
            PuzzleTitleTextBox.InvalidateVisual();
            PuzzleTitleLabel.UpdateLayout();
            PuzzleTitleLabel.InvalidateVisual();
        }

        public String GetPuzzleTitleFromUser()
        {
            return PuzzleTitleTextBox.Text;
        }
    }
}
