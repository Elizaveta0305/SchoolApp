   M S B A M L   `   `   `  ����L  HSchoolApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:SchoolApplication.ViewModelsSchoolApplication.ViewModels  5   	   Q-clr-namespace:SchoolApplication.Views.WindowsSchoolApplication.Views.Windows  5   	   k:clr-namespace:SchoolApplication.Views.UserControls.AdminUC,SchoolApplication.Views.UserControls.AdminUC  5
   	   o<clr-namespace:SchoolApplication.Views.UserControls.StudentUC.SchoolApplication.Views.UserControls.StudentUC  5   	   o<clr-namespace:SchoolApplication.Views.UserControls.TeacherUC.SchoolApplication.Views.UserControls.TeacherUC  5   	   "    SchoolApplication.MainWindow   5   	   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9vm*clr-namespace:SchoolApplication.ViewModels     5   	   ?views-clr-namespace:SchoolApplication.Views.Windows     5   	   NadminUC:clr-namespace:SchoolApplication.Views.UserControls.AdminUC     5
   	   R	studentUC<clr-namespace:SchoolApplication.Views.UserControls.StudentUC     5   	   R	teacherUC<clr-namespace:SchoolApplication.Views.UserControls.TeacherUC     5   	     � Title$	  App��5   	   $	��800��5   	   $
��1440��5   	    � AllowsTransparency$
 True��5   	    � WindowStyle$
 None=�5   	    � WindowStartupLocation$ CenterScreen=�5   	   
������  � 	Resources 5       �� DataType1   +SchoolApplication.ViewModels.LoginViewModel/   )SchoolApplication.Views.Windows.LoginView.   (SchoolApplication.ViewModels.HomeAdminVm@   :SchoolApplication.Views.UserControls.AdminUC.HomeAdminView4   .SchoolApplication.ViewModels.ClassroomsAdminVmF   @SchoolApplication.Views.UserControls.AdminUC.ClassroomsAdminView/   )SchoolApplication.ViewModels.DiaryAdminVmA   ;SchoolApplication.Views.UserControls.AdminUC.DiaryAdminView0	   *SchoolApplication.ViewModels.GroupsAdminVmB
   <SchoolApplication.Views.UserControls.AdminUC.GroupsAdminView0   *SchoolApplication.ViewModels.LessonAdminVmC   =SchoolApplication.Views.UserControls.AdminUC.LessonsAdminView1   +SchoolApplication.ViewModels.SubjectAdminVmC   =SchoolApplication.Views.UserControls.AdminUC.SubjectAdminView/   )SchoolApplication.ViewModels.UsersAdminVmA   ;SchoolApplication.Views.UserControls.AdminUC.UsersAdminView4   .SchoolApplication.ViewModels.NavigationAdminVmB   <SchoolApplication.Views.UserControls.AdminUC.NavBarAdminView)   #SchoolApplication.ViewModels.HomeVm9   3SchoolApplication.Views.UserControls.StudentUC.Home*   $SchoolApplication.ViewModels.GradeVm:   4SchoolApplication.Views.UserControls.StudentUC.Grade,   &SchoolApplication.ViewModels.LessonsVm<   6SchoolApplication.Views.UserControls.StudentUC.Lessons/   )SchoolApplication.ViewModels.NavigationVm?   9SchoolApplication.Views.UserControls.StudentUC.NavBarView0   *SchoolApplication.ViewModels.HomeTeacherVmD   >SchoolApplication.Views.UserControls.TeacherUC.HomeTeacherView1   +SchoolApplication.ViewModels.DiaryTeacherVmE   ?SchoolApplication.Views.UserControls.TeacherUC.DiaryTeacherView2   ,SchoolApplication.ViewModels.LessonTeacherVmF    @SchoolApplication.Views.UserControls.TeacherUC.LessonTeacherView6!   0SchoolApplication.ViewModels.TeacherNavigationVmF"   @SchoolApplication.Views.UserControls.TeacherUC.NavBarTeacherView��%�  (��       5      *, +)(�� 2     5      *, +)(�� d     5       *, +)(�� �     5#      *, +)(�� �     5&      *,	 +)(�� �     5)      *, +)(�� ,    5,      *, +)(�� ^    5/      *, +)(�� �    52      *, +)(�� �    56      *, +)(�� �    59      *, +)(�� &    5<      *, +)(�� X    5?      *, +)(�� �    5B      *, +)(�� �    5E      *, +)(�� �    5H      *, +)(��      5K      *,! +)�� 5   
   "  6   .R�5        5      �� 5   
   "  6   .R�5        5      �� 5    
   "  6   .R�5!        5"      �� 5#   
   "  6   .R�5$        5%      �� 5&   
   " 	 6   .R�5'      
  5(      �� 5)   
   "  6   .R�5*        5+      �� 5,   
   "  6   .R�5-        5.      �� 5/   
   "  6   .R�50        51      �� 52   
   "  6   .R�53        54      �� 56   
   "  6   .R�57        58      �� 59   
   "  6   .R�5:        5;      �� 5<   
   "  6   .R�5=        5>      �� 5?   
   "  6   .R�5@        5A      �� 5B   
   "  6   .R�5C        5D      �� 5E   
   "  6   .R�5F        5G      �� 5H   
   "  6   .R�5I         5J      �� 5K   
   " ! 6   .R�5L      "  5M      5N      .��5P      �  � ColumnDefinitions 5Q   
   �� 5R      $	��300 �6   �� 5S      $��* �6   .+�5V   
   �� $��0��6   ��6)   �� *CurrentNavigationViewModel+�� 5X   
   $��1��6   
���������6)   �� *CurrentMainContentViewModel+��5Y      �� 5Z       �� 
TargetType" ��6"   .R�5[      �� #��z��6   ._�5\      �� #��z��6+   z�5]      T� 5^   "    T� X$ 0Q�65   	 T� Y$	 0Q�6;   
 � Triggers
 5`      :� 5a   "    :� RoutedEvent$ ContentPresenter.Loaded��6/   .[�5b   &   �� .��5c   *   �� .��5d   .   W�  �� TargetProperty$ Opacity�6>    W� From 05e       	 W� To 16)    g� Duration$ 0:0:0.8D�60   W� 5f   .   $ RenderTransform.Y�6>    205g        06*   $ 0:0:0.7D�61   5h   +   5i   '   5j   #   5l      5m      5n      5p      5q      5r      