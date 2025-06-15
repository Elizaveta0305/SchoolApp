using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.Services;
using SchoolApplication.ViewModels;
using SchoolApplication.Views.UserControls.TeacherUC;
using SchoolApplication.Views.Windows;
using System.Configuration;
using System.Windows;

namespace SchoolApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["SchoolDbConnection"].ConnectionString;

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(connectionString);
                    });

                    services.AddSingleton<IAuthService, AuthService>();


                    // Основные ViewModel, которые MainViewModel использует напрямую
                    services.AddSingleton<LoginViewModel>();
                    services.AddSingleton<HomeVm>();        // студент
                    services.AddSingleton<HomeAdminVm>();
                    services.AddSingleton<HomeTeacherVm>();

                    // ViewModel'ы для админской части
                    services.AddSingleton<ClassroomsAdminVm>();
                    services.AddSingleton<DiaryAdminVm>();
                    services.AddSingleton<GroupsAdminVm>();
                    services.AddSingleton<SubjectAdminVm>();
                    services.AddSingleton<UsersAdminVm>();
                    services.AddSingleton<LessonAdminVm>();

                    // ViewModel'ы для студенческой части
                    services.AddSingleton<GradeVm>();
                    services.AddSingleton<LessonsVm>();
                    // Если у вас есть Chart.xaml и ChartVm.cs:
                    // services.AddSingleton<ChartVm>();

                    // ViewModel'ы для учительской части
                    services.AddSingleton<DiaryTeacherVm>();
                    services.AddSingleton<LessonTeacherVm>();
                                                              
                                                              // services.AddSingleton<LiveChartTeacherUsersVm>();

                    // ViewModel'ы для навигационных панелей
                    services.AddSingleton<NavigationAdminVm>();
                    services.AddSingleton<NavigationVm>();       // студент
                    services.AddSingleton<TeacherNavigationVm>();

                    // MainViewModel регистрируется ПОСЛЕ ВСЕХ своих зависимостей
                    services.AddSingleton<MainViewModel>();

                    services.AddSingleton<LoginView>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();

                if (!await dbContext.Users.AnyAsync())
                {
                    var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Администратор");
                    if (adminRole == null)
                    {
                        adminRole = new Role { RoleName = "Администратор" };
                        dbContext.Roles.Add(adminRole);
                    }

                    var teacherRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Преподаватель");
                    if (teacherRole == null)
                    {
                        teacherRole = new Role { RoleName = "Преподаватель" };
                        dbContext.Roles.Add(teacherRole);
                    }

                    var studentRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Ученик");
                    if (studentRole == null)
                    {
                        studentRole = new Role { RoleName = "Ученик" };
                        dbContext.Roles.Add(studentRole);
                    }

                    await dbContext.SaveChangesAsync();

                    dbContext.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = "adminpass",
                        RoleID = adminRole.RoleID,
                        Role = adminRole
                    });
                    dbContext.Users.Add(new User
                    {
                        Username = "teacher",
                        PasswordHash = "teacherpass",
                        RoleID = teacherRole.RoleID,
                        Role = teacherRole
                    });
                    dbContext.Users.Add(new User
                    {
                        Username = "student",
                        PasswordHash = "studentpass",
                        RoleID = studentRole.RoleID,
                        Role = studentRole
                    });

                    await dbContext.SaveChangesAsync();
                    Console.WriteLine("Тестовые пользователи добавлены (пароли не хэшированы).");
                }
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}