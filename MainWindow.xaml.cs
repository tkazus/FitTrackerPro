using FitTrackerPro.Data;
using FitTrackerPro.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitTrackerPro
{
    public partial class MainWindow : Window
    {
        private readonly FitTrackerContext db = new FitTrackerContext();
        private Exercise selectedExercise;
        private DateTime workoutStartTime;
        private bool isWorkoutActive = false;
        private List<WorkoutItem> currentWorkoutItems = new List<WorkoutItem>();

        public MainWindow()
        {
            InitializeComponent();

            if (AppState.CurrentUser != null)
            {
                UserInfoText.Text = AppState.CurrentUser.Username;
            }

            LoadExercises();

            ExercisesListBox.SelectionChanged += ExercisesListBox_SelectionChanged;
            SearchTextBox.TextChanged += SearchTextBox_Changed;

            StartWorkoutButton.Visibility = Visibility.Visible;
            FinishWorkoutButton.Visibility = Visibility.Collapsed;
            AddExerciseButton.IsEnabled = false;
            DeleteExerciseButton.IsEnabled = false;
        }

        private void LoadExercises()
        {
            try
            {
                ExercisesListBox.ItemsSource = db.Exercises.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки упражнений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_Changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                string text = SearchTextBox.Text.ToLower();

                var filtered = db.Exercises
                    .Where(x => x.Name.ToLower().Contains(text))
                    .ToList();

                ExercisesListBox.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ExerciseVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoStatusText.Text = "Воспроизведение завершено";
            VideoStatusText.Foreground = System.Windows.Media.Brushes.LightGray;
            System.Diagnostics.Debug.WriteLine("Видео завершено");
        }

        private void ExercisesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedExercise = ExercisesListBox.SelectedItem as Exercise;

            if (selectedExercise == null)
                return;

            NameText.Text = selectedExercise.Name;
            DescriptionText.Text = FormatText(selectedExercise.Description ?? "Описание отсутствует");
            TechniqueText.Text = FormatText(selectedExercise.Technique ?? "Техника выполнения отсутствует");

            // Останавливаем видео и сбрасываем статус
            ExerciseVideo.Stop();
            ExerciseVideo.Source = null;
            VideoStatusText.Text = "Выберите упражнение для просмотра видео";
            VideoStatusText.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private string FormatText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("\\n", "\n").Replace("\n", Environment.NewLine);

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var formattedLines = new List<string>();

            foreach (var line in lines)
            {
                string trimmedLine = line.TrimStart();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    formattedLines.Add(trimmedLine);
                }
            }

            return string.Join(Environment.NewLine, formattedLines);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedExercise == null)
            {
                MessageBox.Show("Выберите упражнение.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedExercise.VideoPath))
            {
                MessageBox.Show("Для этого упражнения видео отсутствует.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    selectedExercise.VideoPath);

                // Проверяем, существует ли файл
                if (!System.IO.File.Exists(path))
                {
                    MessageBox.Show($"Видеофайл не найден: {path}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    VideoStatusText.Text = "Видеофайл не найден";
                    VideoStatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                ExerciseVideo.Stop();
                ExerciseVideo.Source = new Uri(path, UriKind.Absolute);
                ExerciseVideo.Play();

                // Обновляем статус
                VideoStatusText.Text = $"Воспроизводится: {selectedExercise.Name}";
                VideoStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;

                System.Diagnostics.Debug.WriteLine($"Воспроизведение видео: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при воспроизведении видео: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                VideoStatusText.Text = "Ошибка воспроизведения";
                VideoStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        /// <summary>
        /// Остановка воспроизведения видео
        /// </summary>
        private void StopVideoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExerciseVideo.Source != null)
                {
                    ExerciseVideo.Stop();
                    VideoStatusText.Text = "Воспроизведение остановлено";
                    VideoStatusText.Foreground = System.Windows.Media.Brushes.LightGray;

                    System.Diagnostics.Debug.WriteLine("Видео остановлено пользователем");
                }
                else
                {
                    VideoStatusText.Text = "Видео не загружено";
                    VideoStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке видео: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentWorkoutItems.Any())
            {
                var result = MessageBox.Show("У вас есть незавершенная тренировка. Начать новую?",
                    "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                currentWorkoutItems.Clear();
                WorkoutDataGrid.ItemsSource = null;
            }

            workoutStartTime = DateTime.Now;
            isWorkoutActive = true;

            StartWorkoutButton.Visibility = Visibility.Collapsed;
            FinishWorkoutButton.Visibility = Visibility.Visible;
            AddExerciseButton.IsEnabled = true;
            DeleteExerciseButton.IsEnabled = true;

            MessageBox.Show("Тренировка начата! Добавляйте упражнения.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isWorkoutActive)
            {
                MessageBox.Show("Сначала начните тренировку.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedExercise == null)
            {
                MessageBox.Show("Выберите упражнение из списка.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(
                WeightTextBox.Text.Replace(".", ","),
                NumberStyles.Any,
                CultureInfo.CurrentCulture, out decimal weight))
            {
                MessageBox.Show("Введите корректный вес (например: 50,5).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (weight <= 0)
            {
                MessageBox.Show("Вес должен быть больше 0.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(SetsTextBox.Text, out int sets) || sets <= 0)
            {
                MessageBox.Show("Введите корректное количество подходов (целое число > 0).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(RepsTextBox.Text, out int reps) || reps <= 0)
            {
                MessageBox.Show("Введите корректное количество повторений (целое число > 0).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentWorkoutItems.Add(new WorkoutItem
            {
                ExerciseId = selectedExercise.Id,
                ExerciseName = selectedExercise.Name,
                Weight = weight,
                Sets = sets,
                Reps = reps
            });

            WorkoutDataGrid.ItemsSource = null;
            WorkoutDataGrid.ItemsSource = currentWorkoutItems;

            WeightTextBox.Clear();
            SetsTextBox.Clear();
            RepsTextBox.Clear();

            UpdateWorkoutStats();
        }

        private void DeleteExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkoutDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите упражнение в таблице для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedItem = WorkoutDataGrid.SelectedItem as WorkoutItem;
            if (selectedItem != null)
            {
                currentWorkoutItems.Remove(selectedItem);
                WorkoutDataGrid.ItemsSource = null;
                WorkoutDataGrid.ItemsSource = currentWorkoutItems;
                UpdateWorkoutStats();
            }
        }

        private void FinishWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!currentWorkoutItems.Any())
            {
                MessageBox.Show("Вы не добавили ни одного упражнения.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Завершить тренировку? Добавлено упражнений: {currentWorkoutItems.Count}",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                SaveWorkoutToDatabase();

                currentWorkoutItems.Clear();
                WorkoutDataGrid.ItemsSource = null;
                isWorkoutActive = false;

                StartWorkoutButton.Visibility = Visibility.Visible;
                FinishWorkoutButton.Visibility = Visibility.Collapsed;
                AddExerciseButton.IsEnabled = false;
                DeleteExerciseButton.IsEnabled = false;

                MessageBox.Show("Тренировка успешно завершена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении тренировки:\n\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveWorkoutToDatabase()
        {
            var session = new WorkoutSession
            {
                UserId = AppState.CurrentUser.Id,
                StartTime = workoutStartTime,
                EndTime = DateTime.Now,
                WorkoutDate = DateTime.Now.Date,
                TotalExercises = currentWorkoutItems.Count,
                TotalWeight = currentWorkoutItems.Sum(x => x.Weight * x.Sets * x.Reps)
            };

            foreach (var item in currentWorkoutItems)
            {
                session.Exercises.Add(new WorkoutExercise
                {
                    ExerciseId = item.ExerciseId,
                    Weight = item.Weight,
                    Sets = item.Sets,
                    Reps = item.Reps
                });
            }

            db.WorkoutSessions.Add(session);
            db.SaveChanges();
        }

        private void UpdateWorkoutStats()
        {
            if (currentWorkoutItems.Any())
            {
                var totalWeight = currentWorkoutItems.Sum(x => x.Weight * x.Sets * x.Reps);
                FinishWorkoutButton.Content = $"Завершить ({currentWorkoutItems.Count} упр., {totalWeight:F0} кг)";
            }
            else
            {
                FinishWorkoutButton.Content = "Завершить тренировку";
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AppState.CurrentUser = null;
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
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