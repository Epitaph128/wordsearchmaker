   M S B A M L   `   `   `  ����   ChemCompLogger    ChemCompLogger.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Title$  
MainWindow��$	��300��$	��400��$	��300��$	��400��.��� -   $��
WindowGrid�����#051A38 � RowDefinitions �� $
��Auto ��� $��1* �.+�� -   $��
MasterGrid��$��1�� � ColumnDefinitions �� $��1* ��� $��1* �.+�� -   $��
ButtonGrid��$��0��$��8 8 8 32q� K� ZIndex$ 8�� �� $��1* ��� $��1* � �� $��1* ��� $��1* ��� $��1* ��� $��1* ��� -   $��tableHeader��$��8,0,8,8q�$��0��$
��Left=�$y�Hidden=�
�������$��24(�&   System.Windows.Controls.DataGrid  -   $��wordBankDataGrid��$��8,32,8,8q�$��0��   ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$ * $   HorizontalScrollBarVisibility$ Hidden=�$
��Left=�$y�Hidden=�$	��Top=�   CanUserAddRows .     CanUserResizeRows .  $ 8��   AutoGenerateColumns .  	   RowHeaderWidth$	 16Q����� 
 �� RelativeSource
 �� *FindAncestor+ �� AncestorType" � �� Path$$ RowDefinitions[0].ActualHeight�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Words��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $
 Text� � ContextMenu �� .@�m� -   $��Delete���� -    ��Name$ infoLogPanel��$
��Auto��" �� VerticalScrollBarVisibility$ Hidden=�$��1�� �� TextWrapping$
 Wrap=� �� AcceptsReturn .  �� 
IsReadOnly . $��16(�$��Consolas)����#051A38���#A6B5CB$��0q� �� MaxLines$	 200��.���� -    ��Name$ PuzzleTitleLabel��$��Puzzle Title:��$
��Left=�$��
10,242,0,0q�$	��Top=� J� RenderTransformOrigin$ 
0.503,0.47&�$��95��#��� ��$ -1��$��14(��� -	   $ PuzzleTitleTextBox��$
��Left=�$��27��$��94,247,0,-3.6q�$
 Wrap=�$	��Top=�$	��256��$��14(�$��2��$��Word Search Puzzle��