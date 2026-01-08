using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace WallpaperChanger
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer mainTimer;
        private DispatcherTimer processMonitorTimer;
        private List<string> imageFiles;
        private int currentImageIndex = 0;
        private Random random = new Random();
        private ObservableCollection<ProgramMapping> programMappings;
        private string lastActiveProcess = "";

        public MainWindow()
        {
            InitializeComponent();
            imageFiles = new List<string>();
            programMappings = new ObservableCollection<ProgramMapping>();
            ProgramMappingsListView.ItemsSource = programMappings;

            mainTimer = new DispatcherTimer();
            mainTimer.Tick += MainTimer_Tick;

            processMonitorTimer = new DispatcherTimer();
            processMonitorTimer.Interval = TimeSpan.FromSeconds(2);
            processMonitorTimer.Tick += ProcessMonitorTimer_Tick;

            LoadSettings();
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку с изображениями";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPathTextBox.Text = dialog.SelectedPath;
                LoadImages(dialog.SelectedPath);
            }
        }

        private void LoadImages(string folderPath)
        {
            try
            {
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                imageFiles = Directory.GetFiles(folderPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                ImageCountText.Text = $"Найдено изображений: {imageFiles.Count}";
                StatusText.Text = $"Загружено {imageFiles.Count} изображений из папки";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображений: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FrequencyTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FrequencyTypeCombo.SelectedIndex == 6) // "В определенное время"
            {
                IntervalPanel.Visibility = Visibility.Collapsed;
                SpecificTimePanel.Visibility = Visibility.Visible;
            }
            else
            {
                IntervalPanel.Visibility = Visibility.Visible;
                SpecificTimePanel.Visibility = Visibility.Collapsed;
                UpdateIntervalSlider();
            }
        }

        private void UpdateIntervalSlider()
        {
            switch (FrequencyTypeCombo.SelectedIndex)
            {
                case 0: // Секунды
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 60;
                    IntervalSlider.Value = 5;
                    break;
                case 1: // Минуты
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 120;
                    IntervalSlider.Value = 5;
                    break;
                case 2: // Часы
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 24;
                    IntervalSlider.Value = 1;
                    break;
                case 3: // Дни
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 30;
                    IntervalSlider.Value = 1;
                    break;
                case 4: // Недели
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 52;
                    IntervalSlider.Value = 1;
                    break;
                case 5: // Месяцы
                    IntervalSlider.Minimum = 1;
                    IntervalSlider.Maximum = 12;
                    IntervalSlider.Value = 1;
                    break;
            }
        }

        private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IntervalValueText != null)
            {
                IntervalValueText.Text = ((int)IntervalSlider.Value).ToString();
            }
        }

        private void EnableDesktopChange_Checked(object sender, RoutedEventArgs e)
        {
            FrequencyTypeCombo.IsEnabled = true;
            IntervalSlider.IsEnabled = true;
            ShuffleCheckBox.IsEnabled = true;
        }

        private void EnableDesktopChange_Unchecked(object sender, RoutedEventArgs e)
        {
            FrequencyTypeCombo.IsEnabled = false;
            IntervalSlider.IsEnabled = false;
            ShuffleCheckBox.IsEnabled = false;
        }

        private void EnableProgramBasedLockScreen_Checked(object sender, RoutedEventArgs e)
        {
            ProgramMappingPanel.Visibility = Visibility.Visible;
        }

        private void EnableProgramBasedLockScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            ProgramMappingPanel.Visibility = Visibility.Collapsed;
        }

        private void BrowseProgramImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            dialog.Title = "Выберите изображение";

            if (dialog.ShowDialog() == true)
            {
                ProgramImagePathTextBox.Text = dialog.FileName;
            }
        }

        private void AddProgramMappingButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProgramNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ProgramImagePathTextBox.Text))
            {
                MessageBox.Show("Заполните оба поля перед добавлением привязки",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            programMappings.Add(new ProgramMapping
            {
                ProgramName = ProgramNameTextBox.Text.Trim().ToLower(),
                ImagePath = ProgramImagePathTextBox.Text
            });

            ProgramNameTextBox.Clear();
            ProgramImagePathTextBox.Clear();
            StatusText.Text = "Привязка программы добавлена";
        }

        private void RemoveProgramMappingButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramMappingsListView.SelectedItem != null)
            {
                programMappings.Remove((ProgramMapping)ProgramMappingsListView.SelectedItem);
                StatusText.Text = "Привязка программы удалена";
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageFiles.Count == 0)
            {
                MessageBox.Show("Сначала выберите папку с изображениями",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EnableDesktopChange.IsChecked == true)
            {
                SetupTimer();
                mainTimer.Start();
            }

            if (EnableProgramBasedLockScreen.IsChecked == true)
            {
                processMonitorTimer.Start();
            }

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StatusText.Text = "Автоматическая смена обоев запущена";
            
            // Сменить обои сразу при запуске
            ChangeWallpaper();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mainTimer.Stop();
            processMonitorTimer.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusText.Text = "Автоматическая смена обоев остановлена";
        }

        private void ChangeNowButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageFiles.Count == 0)
            {
                MessageBox.Show("Сначала выберите папку с изображениями",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChangeWallpaper();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Настройки сохранены!", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetupTimer()
        {
            double interval = 0;

            switch (FrequencyTypeCombo.SelectedIndex)
            {
                case 0: // Секунды
                    interval = IntervalSlider.Value * 1000;
                    break;
                case 1: // Минуты
                    interval = IntervalSlider.Value * 60 * 1000;
                    break;
                case 2: // Часы
                    interval = IntervalSlider.Value * 60 * 60 * 1000;
                    break;
                case 3: // Дни
                    interval = IntervalSlider.Value * 24 * 60 * 60 * 1000;
                    break;
                case 4: // Недели
                    interval = IntervalSlider.Value * 7 * 24 * 60 * 60 * 1000;
                    break;
                case 5: // Месяцы
                    interval = IntervalSlider.Value * 30 * 24 * 60 * 60 * 1000;
                    break;
                case 6: // В определенное время
                    SetupScheduledTimer();
                    return;
            }

            mainTimer.Interval = TimeSpan.FromMilliseconds(interval);
        }

        private void SetupScheduledTimer()
        {
            // Проверяем каждую минуту, не пришло ли время
            mainTimer.Interval = TimeSpan.FromMinutes(1);
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (FrequencyTypeCombo.SelectedIndex == 6) // Определенное время
            {
                if (IsScheduledTime())
                {
                    ChangeWallpaper();
                }
            }
            else
            {
                ChangeWallpaper();
            }
        }

        private bool IsScheduledTime()
        {
            try
            {
                string[] times = SpecificTimesTextBox.Text.Split(';');
                TimeSpan currentTime = DateTime.Now.TimeOfDay;

                foreach (string timeStr in times)
                {
                    if (TimeSpan.TryParse(timeStr.Trim(), out TimeSpan scheduledTime))
                    {
                        // Проверяем, что текущее время в пределах минуты от запланированного
                        if (Math.Abs((currentTime - scheduledTime).TotalMinutes) < 1)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private void ProcessMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                string currentProcess = "";

                // Получаем процесс активного окна
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd != IntPtr.Zero)
                {
                    uint processId;
                    GetWindowThreadProcessId(hwnd, out processId);
                    
                    Process activeProcess = processes.FirstOrDefault(p => p.Id == processId);
                    if (activeProcess != null)
                    {
                        currentProcess = activeProcess.ProcessName.ToLower() + ".exe";
                    }
                }

                // Если процесс изменился, проверяем привязки
                if (currentProcess != lastActiveProcess && !string.IsNullOrEmpty(currentProcess))
                {
                    lastActiveProcess = currentProcess;
                    
                    var mapping = programMappings.FirstOrDefault(m => 
                        m.ProgramName.ToLower() == currentProcess);
                    
                    if (mapping != null && File.Exists(mapping.ImagePath))
                    {
                        SetLockScreen(mapping.ImagePath);
                        StatusText.Text = $"Экран блокировки изменен для: {currentProcess}";
                    }
                }
            }
            catch { }
        }

        private void ChangeWallpaper()
        {
            if (imageFiles.Count == 0) return;

            try
            {
                string imagePath;

                if (ShuffleCheckBox.IsChecked == true)
                {
                    imagePath = imageFiles[random.Next(imageFiles.Count)];
                }
                else
                {
                    imagePath = imageFiles[currentImageIndex];
                    currentImageIndex = (currentImageIndex + 1) % imageFiles.Count;
                }

                SetWallpaper(imagePath);

                if (EnableLockScreenChange.IsChecked == true)
                {
                    SetLockScreen(imagePath);
                }

                StatusText.Text = $"Обои изменены: {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка при смене обоев: {ex.Message}";
            }
        }

        // Windows API для смены обоев
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, 
            string lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private void SetWallpaper(string imagePath)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, 
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private void SetLockScreen(string imagePath)
        {
            try
            {
                // Для Windows 10/11 нужно скопировать файл в специальную папку
                string lockScreenPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets"
                );

                // Также устанавливаем через реестр
                string personalizedPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";
                
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(personalizedPath))
                {
                    if (key != null)
                    {
                        key.SetValue("LockScreenImagePath", imagePath);
                        key.SetValue("LockScreenImageUrl", imagePath);
                        key.SetValue("LockScreenImageStatus", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при установке экрана блокировки: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new Dictionary<string, string>
                {
                    ["FolderPath"] = FolderPathTextBox.Text,
                    ["FrequencyType"] = FrequencyTypeCombo.SelectedIndex.ToString(),
                    ["Interval"] = IntervalSlider.Value.ToString(),
                    ["SpecificTimes"] = SpecificTimesTextBox.Text,
                    ["Shuffle"] = ShuffleCheckBox.IsChecked.ToString(),
                    ["EnableDesktop"] = EnableDesktopChange.IsChecked.ToString(),
                    ["EnableLockScreen"] = EnableLockScreenChange.IsChecked.ToString(),
                    ["EnableProgramBased"] = EnableProgramBasedLockScreen.IsChecked.ToString()
                };

                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WallpaperChanger"
                );

                Directory.CreateDirectory(settingsPath);
                string settingsFile = Path.Combine(settingsPath, "settings.txt");

                File.WriteAllLines(settingsFile, 
                    settings.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                // Сохраняем привязки программ
                string mappingsFile = Path.Combine(settingsPath, "program_mappings.txt");
                File.WriteAllLines(mappingsFile,
                    programMappings.Select(m => $"{m.ProgramName}|{m.ImagePath}"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WallpaperChanger"
                );

                string settingsFile = Path.Combine(settingsPath, "settings.txt");

                if (File.Exists(settingsFile))
                {
                    var settings = File.ReadAllLines(settingsFile)
                        .Select(line => line.Split('='))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(parts => parts[0], parts => parts[1]);

                    if (settings.ContainsKey("FolderPath") && !string.IsNullOrEmpty(settings["FolderPath"]))
                    {
                        FolderPathTextBox.Text = settings["FolderPath"];
                        LoadImages(settings["FolderPath"]);
                    }

                    if (settings.ContainsKey("FrequencyType"))
                        FrequencyTypeCombo.SelectedIndex = int.Parse(settings["FrequencyType"]);

                    if (settings.ContainsKey("Interval"))
                        IntervalSlider.Value = double.Parse(settings["Interval"]);

                    if (settings.ContainsKey("SpecificTimes"))
                        SpecificTimesTextBox.Text = settings["SpecificTimes"];

                    if (settings.ContainsKey("Shuffle"))
                        ShuffleCheckBox.IsChecked = bool.Parse(settings["Shuffle"]);

                    if (settings.ContainsKey("EnableDesktop"))
                        EnableDesktopChange.IsChecked = bool.Parse(settings["EnableDesktop"]);

                    if (settings.ContainsKey("EnableLockScreen"))
                        EnableLockScreenChange.IsChecked = bool.Parse(settings["EnableLockScreen"]);

                    if (settings.ContainsKey("EnableProgramBased"))
                        EnableProgramBasedLockScreen.IsChecked = bool.Parse(settings["EnableProgramBased"]);
                }

                // Загружаем привязки программ
                string mappingsFile = Path.Combine(settingsPath, "program_mappings.txt");
                if (File.Exists(mappingsFile))
                {
                    var mappings = File.ReadAllLines(mappingsFile)
                        .Select(line => line.Split('|'))
                        .Where(parts => parts.Length == 2);

                    foreach (var parts in mappings)
                    {
                        programMappings.Add(new ProgramMapping
                        {
                            ProgramName = parts[0],
                            ImagePath = parts[1]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }
    }

    public class ProgramMapping
    {
        public string ProgramName { get; set; }
        public string ImagePath { get; set; }
    }
}
