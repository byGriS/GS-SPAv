   M S B A M L   `   `   `  ����C  ?GS-SPAv, Version=1.13.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:GS_SPAvGS_SPAv  5   	        GS_SPAv.SettingAlarmWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:GS_SPAv     5   	   -   5          � Title$-  'Настройки параметров��5   	   $	��600��5	   	   $	��700��6    � WindowStartupLocation$ CenterScreen=�5   	   .��5      �  � RowDefinitions 5      �� 5      �� 5      $��36 �6   �� 5      $��36 �6   .+�5      &   System.Windows.Controls.DataGrid  -   5          Name$ dgParamSetting��5      $��2��5         AutoGenerateColumns .     CanUserDeleteRows .     CanUserAddRows .     SelectionMode$ Single=�5         SelectionUnit$ FullRow=�5      	   RowHeaderWidth$	 20Q�5      
   CanUserSortColumns
 .     Columns 5"      0  *System.Windows.Controls.DataGridTextColumn  5#   
   ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .    Header$ Параметр��5%         Width5  /System.Windows.Controls.DataGridLengthConverter$ 3* 5&      1  +System.Windows.Controls.DataGridBoundColumn   Binding 5'      ��  �� Path$ Title� �� Mode$ TwoWay=�4  .System.Windows.Controls.DataGridTemplateColumn  5(   
   $ Ед.изм��6!   $
 1.3* 61      CellTemplate 5)      �� 5*      .R�5+      ��  �� SelectedValuePath$ Title��5/       �� DisplayMemberPath$ Title��50      {�5,      �� $ IsBlock���5-      �� $ 	ListUnits� �� SelectedItem 5.      �� $
 Unit�$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=���51      �� 52      .R�53      �� ��6    �� 54      56      57      59        5:   
   $ Цвет��5;      $ 1* 5<       5=      �� 5>      .R�5?      �� -   5        ��5?      �� $ 	ColorLine�6=   5@      5B        5C   
   $
 1.0* 6    6*   �� $ Accuracy� 5D      �� 5E      $��Точность��6    � ToolTip$2 ,Кол-во знаков после нуля��6(      ElementStyle 5G      �� 5H      .�5I      ��  �� Property �@��TextAlignment �� Value$ Center=�6:   5J      5L        5M   
   $ AlarmL��6   $
 1.0* 6-    6:   �� $ AlarmMin� 5N      �� 5O      .�5P      ��  �@��TextAlignment$ Center=�6:   5Q      5S        5T   
   $ AlarmH��6   $
 1.0* 6-    6:   �� $ AlarmMax� 5U      �� 5V      .�5W      ��  �@��TextAlignment$ Center=�6:   5X      5Z      4  .System.Windows.Controls.DataGridCheckBoxColumn  5[   
   $ Сигн.цв.��5\      $
 1.0* 5]       5^      �� $ IsAlarmColor�  5_   
   $ Сигн.зв.��5`      $
 1.0* 5a       5b      �� $ IsAlarmSound�5d      � 5e      $��1��6   .+�5f      ��  �� Orientation$ 
Horizontal=�6   $��Center=�6,   .+�5g   
   �� $D��>Расчет концентрации по расходу от��6   �� 5h   
   -   5         ��Name$ cbSelectFlow��5i      $��2��5h      $��Center=�5j      $��3q�5k      $	��170��5l      5m      5n   	   5o      � 5p      $��2��6   $��2��6   .+�5q      �� $ 
Horizontal=�6   $��Right=�6,   $��5q�6H   .+�5r   
   �� -   5         ��Name$ btnOk��5r      $��Применить��6    $	��5 0q�64   $
��10 0q�6A   6P   �� 5s   
   -   5        $��Закрыть��5s      $	��5 0q�6#   $
��10 0q�60   6?   5t   	   5u      5v      5w      