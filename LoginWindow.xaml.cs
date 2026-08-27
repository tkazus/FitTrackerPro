using FitTrackerPro.Data;
using FitTrackerPro.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FitTrackerPro
{
    public partial class LoginWindow : Window
    {
        private readonly FitTrackerContext db = new FitTrackerContext();
        private bool isLoginMode = true;

        public LoginWindow()
        {
            InitializeComponent();

            // Проверяем подключение к БД при запуске
            CheckDatabaseConnection();
        }

        // Проверка подключения к БД
        private void CheckDatabaseConnection()
        {
            try
            {
                var count = db.Users.Count();
                System.Diagnostics.Debug.WriteLine($"Подключение к БД успешно. Пользователей в БД: {count}");

                // Выводим всех пользователей
                var users = db.Users.ToList();
                foreach (var user in users)
                {
                    System.Diagnostics.Debug.WriteLine($"   - {user.Id}: {user.Username} ({user.Email})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка подключения к БД: {ex.Message}");
                MessageBox.Show($"Ошибка подключения к БД:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Переключение вкладок
        private void LoginTab_Click(object sender, RoutedEventArgs e)
        {
            isLoginMode = true;
            LoginTab.Background = System.Windows.Media.Brushes.LightGreen;
            LoginTab.Foreground = System.Windows.Media.Brushes.White;
            RegisterTab.Background = System.Windows.Media.Brushes.Gray;
            RegisterTab.Foreground = System.Windows.Media.Brushes.LightGray;

            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginMessage.Text = "";
            RegisterMessage.Text = "";
        }

        private void RegisterTab_Click(object sender, RoutedEventArgs e)
        {
            isLoginMode = false;
            RegisterTab.Background = System.Windows.Media.Brushes.LightGreen;
            RegisterTab.Foreground = System.Windows.Media.Brushes.White;
            LoginTab.Background = System.Windows.Media.Brushes.Gray;
            LoginTab.Foreground = System.Windows.Media.Brushes.LightGray;

            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            LoginMessage.Text = "";
            RegisterMessage.Text = "";
        }

        // Вход
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = LoginUsername.Text.Trim();
                string password = LoginPassword.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    LoginMessage.Text = "Заполните все поля!";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Попытка входа: {username}");

                var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Вход выполнен: {user.Username} (ID: {user.Id})");
                    AppState.CurrentUser = user;
                    user.LastLoginDate = DateTime.Now;
                    db.SaveChanges();

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    var existingUser = db.Users.FirstOrDefault(u => u.Username == username);
                    if (existingUser != null)
                    {
                        LoginMessage.Text = "Неверный пароль!";
                        System.Diagnostics.Debug.WriteLine($"❌ Неверный пароль для: {username}");
                    }
                    else
                    {
                        LoginMessage.Text = "Пользователь не найден!";
                        System.Diagnostics.Debug.WriteLine($"Пользователь не найден: {username}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoginMessage.Text = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка входа: {ex.Message}");
            }
        }

        private void LoginPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        // Регистрация
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = RegisterUsername.Text.Trim();
                string email = RegisterEmail.Text.Trim();
                string password = RegisterPassword.Password;
                string confirmPassword = RegisterConfirmPassword.Password;

                System.Diagnostics.Debug.WriteLine($"Попытка регистрации: {username}");

                // Валидация
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    RegisterMessage.Text = "Заполните все обязательные поля!";
                    return;
                }

                if (username.Length < 3)
                {
                    RegisterMessage.Text = "Имя пользователя должно содержать минимум 3 символа!";
                    return;
                }

                if (password.Length < 4)
                {
                    RegisterMessage.Text = "Пароль должен содержать минимум 4 символа!";
                    return;
                }

                if (password != confirmPassword)
                {
                    RegisterMessage.Text = "Пароли не совпадают!";
                    return;
                }

                // Проверка на существующего пользователя
                if (db.Users.Any(u => u.Username == username))
                {
                    RegisterMessage.Text = "Пользователь с таким именем уже существует!";
                    System.Diagnostics.Debug.WriteLine($"Пользователь уже существует: {username}");
                    return;
                }

                // Создаем нового пользователя
                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    CreatedDate = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Добавление пользователя: {newUser.Username}");
                db.Users.Add(newUser);

                // Показываем, что добавили
                System.Diagnostics.Debug.WriteLine($"Пользователь добавлен в контекст. Сохраняем...");

                int result = db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges вернул: {result}");

                // Проверяем, что пользователь сохранился
                if (result > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Пользователь '{username}' успешно зарегистрирован! ID: {newUser.Id}");

                    // Проверяем, что пользователь появился в БД
                    var savedUser = db.Users.FirstOrDefault(u => u.Username == username);
                    if (savedUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Пользователь найден в БД: {savedUser.Id} - {savedUser.Username}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Пользователь НЕ найден в БД после сохранения!");
                    }

                    RegisterMessage.Text = "Регистрация успешна! Войдите в систему.";
                    RegisterMessage.Foreground = System.Windows.Media.Brushes.LightGreen;

                    // Переключаем на вкладку входа
                    LoginTab_Click(null, null);
                    LoginUsername.Text = username;
                    LoginPassword.Password = "";
                }
                else
                {
                    RegisterMessage.Text = "Ошибка при сохранении пользователя!";
                    System.Diagnostics.Debug.WriteLine($"❌ SaveChanges вернул 0 - пользователь не сохранен!");
                }
            }
            catch (Exception ex)
            {
                RegisterMessage.Text = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка регистрации: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Inner Exception: {ex.InnerException?.Message}");

                if (ex.InnerException != null)
                {
                    RegisterMessage.Text += $"\n{ex.InnerException.Message}";
                }
            }
        }

        // Управление окном
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}