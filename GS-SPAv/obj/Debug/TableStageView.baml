   M S B A M L   `   `   `  ����C  ?GS-SPAv, Version=1.13.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:GS_SPAvGS_SPAv  5          GS_SPAv.TableStageView   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      'localclr-namespace:GS_SPAv     5      .��5	      �� $��1q�6   
�������._�5
      �   � RowDefinitions  5      �� 5   
   $��30 �6   �� 5   
   .+�5      � $��0��6   .+�5   
   (� .��5      �� $��
Этапы��6   $
��Bold"�6"   5      5   	   &   System.Windows.Controls.DataGrid  5      $��1��5   	      AutoGenerateColumns .     CanUserDeleteRows .     CanUserAddRows .     
IsReadOnly .    SelectionMode$ Single=�5   	      SelectionUnit$ FullRow=�5   	      RowHeaderWidth$ 20Q�5   	   $��16(�5   	      CanUserSortColumns .  ��5   	   �� *.+	 �� IsAsync	 . 
   Columns
 5    
   0  *System.Windows.Controls.DataGridTextColumn  5!      ,  &System.Windows.Controls.DataGridColumn   Header$ Название��5"         Width5  /System.Windows.Controls.DataGridLengthConverter$ 4* 5#      1  +System.Windows.Controls.DataGridBoundColumn   Binding 5$      ��  �� Path$
 Text�  5%      $ V(э/м)��5&      $
 1.5* 5'       5(      ��  �� StringFormat$ 0.###��$ 	Capacity2�  5)      $ V(турб.)��5*      $
 1.5* 5+       5,      �� $ 0.###��$ 	Capacity1�  5-      $
 Pmax��5.      $
 1.5* 5/       50      �� $ 0.###��$ MaxPress�  51      $ Начало��52      $
 1.5* 53       54      �� $ 0.###��$ Start�  55      $ Окончан.��56      $
 1.5* 57       58      �� $ 0.###��$	 End�5:   	   5;      5<      5=      