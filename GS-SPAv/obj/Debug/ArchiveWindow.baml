   M S B A M L   `   `   `  ����C  ?GS-SPAv, Version=1.13.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:GS_SPAvGS_SPAv  5   	       GS_SPAv.ArchiveWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:GS_SPAv     5   	   R NOxyPlot.Wpf, Version=2.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  5   	   -   5          � Title$  
Архив��5	   	   $	��728��6   $
��1366��6$    � WindowStartupLocation$ CenterScreen=�5
   	   .��5      �  � RowDefinitions 5      �� 5      $��20 �6   �� 5      .+�5      � .+�5      o� $
��Left=�6   ��6(   Z� .@�5      m� -   5        $��Сохранить��5      6(   m� 5      -   5        $%��Сохранить график��5      6/   m� 5   
   -   5        $��Экспорт в Excel��5      6,   m� 5   
   -   5        $#��Выгрузка файлов��5      6,   5   	   5      �� 5      $��1q�6   
�������$��1��64   ._�5      �  � ColumnDefinitions 5   
   �� 5      $	��300 �6   �� 5      .+�5!   
   �� 
�������$��0 0 1 0q�6$   ._�5"      �  5#      �� 5$      �� 5%      $��26 �6   �� 5&      $��26 �6   �� 5'      �� 5(      $��26 �6   �� 5)      �� 5*      $��26 �6   �� 5+      $��26 �6   .+�5-      �� -   5         ��Name$ 
lvArchives��5/      $��0��5.       �� SelectionMode$ Single=�50       �� View 53      �� 54      .P�55      ��  �� Width$	 150��6N    �� DisplayMemberBinding 6#   �� 	 �� Path$
	 Well�.��6Z   Скважина6d   �� 56      $	 150��6R    6#   �� $	 StartEnd�.��6^   Дата/время6j   57      59      � 5:      $��1��6    5;      �� 5<      $��60 �6#   �� 5=      .+�5?      �� $��Выбран:��6   �� 5@      -   5        
 ��Name$
 lSelectedArchive��5@   &   $��1��6   $
��Bold"�6@   5A      �� 5B      -   5         ��Name$ btnReadArchive��5C      $��2��5B      $	��0 2q�5D      $#��Прочитать архив��5E      5F      �� 5G      -	   5         ��Name$ lbParams��5H      $��3��5I      $ Single=�5J      ��5K      �� 5L      .R�5M      �� ��6   �� *Title+ d� 	IsChecked 67   �� *	IsChecked+5N      5P      � 5Q      $��4��6    5R      �� 5S      �� 5T      .+�5V      �� -
   5        $��Выделить все��5V      $
��10 2q�6.   6<   �� 5W      -   5        $#��Снять выделения��5W      $��1��61   $
��10 2q�6A   6O   5X      �� 5Y      -   5        $ lbStages��5Y      $��5��5Z      ��5[      �� 5\      .R�5]      �� ��5^      �� *Text+ 5_      �� *	IsChecked+5a      ��5c      �� 5d       �� 
TargetType" ��6    �� Triggers 5e      �� 5f       �� Binding 6"   �� *
IsSecond+ �� Value True6?   .a�5g      ��  �� Property � y� �� Value$ 	Collapsed=�65   5h      5j      5l      �� 5m      -   5        $ btnShowGraphStage��5o      $��6��5n      $	��0 2q�5p      {�.  $7��1Построить график по этапам��5r      5s      �� 5t      -   5        $ btnShowGraph��5v      $��7��5u      $	��0 2q�5w      {�.  $B��<Построить график по всей закачке��5y      5z      5|      5}      � 5~   
   $��1��6    5      �� 5�      .+�5�      � $��1��6    5�      �� 5�      $��36 �6   �� 5�      .+�5�      ��  �� Orientation$ 
Horizontal=�6   .+�5�      �� $��1q�5�      
������� �� CornerRadius$ 3��5�      $��3q�5�      ._�5�      �� -   5        $ btnAddStage��5�      $��Добавить этап��5�       � Cursor$
 Hand��5�      $��5q�5�      5�      �� 5�      $��1q�5�      
�������$ 3��5�      $��3q�5�      ._�5�      �� -   5        $ btnDelStage��5�      $&�� Удалить один этап��5�      $��5q�5�      $
 Hand��5�      5�      �� 5�      $��1q�5�      
�������$ 3��5�      $��3q�5�      ._�5�      �� -   5        $ btnDelAllStage��5�      $&�� Удалить все этапы��5�      $��5q�5�      $
 Hand��5�      5�      5�        OxyPlot.Wpf.PlotView  5�      -   5          Name$ plotter��5�   (   $��1��6     OxyPlot.Wpf.PlotBase   DefaultTrackerTemplate 5�      �� 5�      .R�5�         OxyPlot.Wpf.TrackerControl  
���   �$��1q�5�   ;      HorizontalLineVisibility$ Hidden=�5�   '      Position 5�   '   �� *
Position+   LineExtents 5�   '   �� *PlotModel.PlotArea+��5�      � 5�      $��4,0,4,7q�6    5�      �� 5�      �� 5�      .+�5�      �� $��0��6$   $	��4,2q�6Q   ��61   �� *ValueString[0]+6Q   5�      5�      5�      5�      5�      5�      5�   	   5�      5�      5�      