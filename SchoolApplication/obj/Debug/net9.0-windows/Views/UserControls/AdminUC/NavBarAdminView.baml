   M S B A M L   `   `   `  ����L  HSchoolApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:SchoolApplication.Views.UserControls.AdminUC,SchoolApplication.Views.UserControls.AdminUC  5      K*clr-namespace:SchoolApplication.ViewModelsSchoolApplication.ViewModels  5      I)clr-namespace:SchoolApplication.UtilitiesSchoolApplication.Utilities  5	      B    <SchoolApplication.Views.UserControls.AdminUC.NavBarAdminView   5      W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Lviews:clr-namespace:SchoolApplication.Views.UserControls.AdminUC     5      c _MaterialDesignThemes.MahApps, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a_ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aHmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes   5      5vm*clr-namespace:SchoolApplication.ViewModels   5      6Menu)clr-namespace:SchoolApplication.Utilities   5	      $	��350��5      .��5      � .+�5   
   � $
��Left=�6   $	��300��6*     � RowDefinitions  5      �� 5      $	��20* �6    �� 5      $
��220* �6    .+�5      �� ���#272B2F �� CornerRadius$ 	20,0,0,20��6*   $��1��6C   �� 5      ���#272B2F$ 	20,0,0,20��6*   $��2��6C   ._�5      �� $��Center=�6   $��0 20 0 0q�6:   .+�5      � .+�5      �� $��/Images/Logo.png��6    $��50��6:   $��50��6F    �� BitmapScalingMode$ HighQuality=�6Q   �� 5      $��ШПЭ��6$   
�������$��22(�6B   $��Center=�6P   $��15 0 0 0q�6k   5      5      5      �� 5      $	��600��6   $	��300��6&   $��1��62   .+�5!      %   SchoolApplication.Utilities.Btn     BtnStyle#��[  6    d� 	IsChecked$
 TrueY�5#      ��5"      �� *HomeAdminCommand+.��5$      � .+�5%      � $��Center=�6$   $��Center=�6A   .+�5&      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$
 Home=�66   
�������$��Center=�6U   $��24��6p   $��24��6|   �� 5'      $��Главная��6(   $��10 0 0 0q�67   
�������$��18(�6\   5(      5)      5*        5,      #��[  6   $��Center=�5.      ��5-      �� *UsersAdminCommand+.��5/      � .+�50      � .+�51        $
 User=�66   
�������$��Center=�6U   $��24��6p   $��24��6|   �� 52      $��Пользователи��6(   
�������$��10 0 0 0q�6O   $��18(�6a   53      54      55        57      #��[  6   ��58      �� *LessonsAdminCommand+.��59      � .+�5:      � .+�5;        $ School=�66   
�������$��Center=�6W   $��24��6r   $��24��6~   �� 5<      $��Занятия��6(   
�������$��10 0 0 0q�6J   $��18(�6\   5=      5>      5?        5A      #��[  6   ��5B      �� *DiaryAdminCommand+.��5C      � .+�5D      � .+�5E        $ BookEducation=�66   
�������$��Center=�6^   $��24��6y   $��24��6�   �� 5F      $��Журнал��6(   
�������$��10 0 0 0q�6I   $��18(�6[   5G      5H      5I        5K      #��[  6   ��5L      �� *SubjectAdminCommand+.��5M      � .+�5N      � .+�5O        $
 Book=�66   
�������$��Center=�6U   $��24��6p   $��24��6|   �� 5P      $��Предметы��6(   
�������$��10 0 0 0q�6K   $��18(�6]   5Q      5R      5S        5U      #��[  6   ��5V      �� *ClassroomsAdminCommand+.��5W      � .+�5X      � .+�5Y        $
 Room=�66   
�������$��Center=�6U   $��24��6p   $��24��6|   �� 5Z      $��Кабинеты��6(   
�������$��10 0 0 0q�6K   $��18(�6]   5[      5\      5]        5_      #��[  6   ��5`      �� *GroupsAdminCommand+.��5a      � .+�5b      � .+�5c        $ People=�66   
�������$��Center=�6W   $��24��6r   $��24��6~   �� 5d      $��Группы��6(   
�������$��10 0 0 0q�6I   $��18(�6[   5e      5f      5g        5i      #��[  6   .��5k      � .+�5l      � .+�5m        $ 	ExitToApp=�66   
�������$��Center=�6Z   $��24��6u   $��24��6�   �� 5n      $��
Выход��6(   
�������$��20 0 0 0q�6H   $��18(�6Z   5o      5p      5q      5s      5u      5w      5x      