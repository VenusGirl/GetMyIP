﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace GetMyIP
{
    public partial class MainWindow : Window
    {
        #region NLog Instance
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        #endregion NLog Instance

        #region Stopwatch
        private readonly Stopwatch stopwatch = new Stopwatch();
        #endregion Stopwatch

        public MainWindow()
        {
            InitializeSettings();

            InitializeComponent();

            ReadSettings();
        }

        #region Settings
        private void InitializeSettings()
        {
            stopwatch.Start();

            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);
        }

        public void ReadSettings()
        {
            // Set NLog configuration
            NLHelpers.NLogConfig();

            // Unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Put the version number in the title bar
            Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";

            // Startup message in the temp file
            log.Info($"{AppInfo.AppName} {AppInfo.AppVersion} is starting up");

            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Height = UserSettings.Setting.WindowHeight;
            Width = UserSettings.Setting.WindowWidth;
            Topmost = UserSettings.Setting.KeepOnTop;

            // .NET version, app framework and OS platform
            string version = Environment.Version.ToString();
            log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}  ({version})");
            log.Debug(AppInfo.Framework);
            log.Debug(AppInfo.OsPlatform);

            // Light or dark
            SetBaseTheme(UserSettings.Setting.DarkMode);

            // Primary color
            SetPrimaryColor(UserSettings.Setting.PrimaryColor);

            // UI size
            double size = UIScale(UserSettings.Setting.UISize);
            MainGrid.LayoutTransform = new ScaleTransform(size, size);

            // Initial page viewed
            NavigateToPage(UserSettings.Setting.InitialPage);

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;
        }
        #endregion Settings

        #region Navigation
        private void NavigateToPage(int page)
        {
            switch (page)
            {
                default:
                    _ = MainFrame.Navigate(new Page1());
                    PageTitle.Text = "Internal IP Addresses";
                    NavDrawer.IsLeftDrawerOpen = false;
                    break;
                case 1:
                    _ = MainFrame.Navigate(new Page2());
                    PageTitle.Text = "External IP & Geolocation Information";
                    NavDrawer.IsLeftDrawerOpen = false;
                    break;
                case 2:
                    _ = MainFrame.Navigate(new Page3());
                    PageTitle.Text = "Settings";
                    NavDrawer.IsLeftDrawerOpen = false;
                    break;
                case 3:
                    _ = MainFrame.Navigate(new Page4());
                    PageTitle.Text = "About";
                    NavDrawer.IsLeftDrawerOpen = false;
                    break;
                case 4:
                    Application.Current.Shutdown();
                    break;
            }
            NavListBox.SelectedIndex = page;
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToPage(NavListBox.SelectedIndex);
        }
        #endregion Navigation

        #region Set primary color
        private void SetPrimaryColor(int color)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            PrimaryColor primary;
            switch (color)
            {
                case 0:
                    primary = PrimaryColor.Red;
                    break;
                case 1:
                    primary = PrimaryColor.Pink;
                    break;
                case 2:
                    primary = PrimaryColor.Purple;
                    break;
                case 3:
                    primary = PrimaryColor.DeepPurple;
                    break;
                case 4:
                    primary = PrimaryColor.Indigo;
                    break;
                case 5:
                    primary = PrimaryColor.Blue;
                    break;
                case 6:
                    primary = PrimaryColor.LightBlue;
                    break;
                case 7:
                    primary = PrimaryColor.Cyan;
                    break;
                case 8:
                    primary = PrimaryColor.Teal;
                    break;
                case 9:
                    primary = PrimaryColor.Green;
                    break;
                case 10:
                    primary = PrimaryColor.LightGreen;
                    break;
                case 11:
                    primary = PrimaryColor.Lime;
                    break;
                case 12:
                    primary = PrimaryColor.Yellow;
                    break;
                case 13:
                    primary = PrimaryColor.Amber;
                    break;
                case 14:
                    primary = PrimaryColor.Orange;
                    break;
                case 15:
                    primary = PrimaryColor.DeepOrange;
                    break;
                case 16:
                    primary = PrimaryColor.Brown;
                    break;
                case 17:
                    primary = PrimaryColor.Grey;
                    break;
                case 18:
                    primary = PrimaryColor.BlueGrey;
                    break;
                default:
                    primary = PrimaryColor.Blue;
                    break;
            }
            Color primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];
            theme.SetPrimaryColor(primaryColor);
            paletteHelper.SetTheme(theme);
        }
        #endregion Set primary color

        #region UI scale converter
        private static double UIScale(int size)
        {
            switch (size)
            {
                case 0:
                    return 0.90;
                case 1:
                    return 0.95;
                case 2:
                    return 1.0;
                case 3:
                    return 1.05;
                case 4:
                    return 1.1;
                default:
                    return 1.0;
            }
        }
        #endregion UI scale converter

        #region Window Events
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InternalIP.GetMyInternalIP();

            ExternalInfo.GetExtInfo();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            stopwatch.Stop();
            log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

            // Shut down NLog
            LogManager.Shutdown();

            // Save settings
            UserSettings.Setting.WindowLeft = Math.Floor(Left);
            UserSettings.Setting.WindowTop = Math.Floor(Top);
            UserSettings.Setting.WindowWidth = Math.Floor(Width);
            UserSettings.Setting.WindowHeight = Math.Floor(Height);
            UserSettings.SaveSettings();
        }
        #endregion Window Events

        #region PopupBox button events
        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(NLHelpers.GetLogfileName());
        }

        private void BtnReadme_Click(object sender, RoutedEventArgs e)
        {
            string dir = AppInfo.AppDirectory;
            TextFileViewer.ViewTextFile(Path.Combine(dir, "ReadMe.txt"));
        }

        private void BtnCopyToClip_Click(object sender, RoutedEventArgs e)
        {
            CopytoClipBoard();
        }

        private void BtnSaveText_Click(object sender, RoutedEventArgs e)
        {
            Copyto2TextFile();
        }

        private void BtnShowMap_Click(object sender, RoutedEventArgs e)
        {
            IPInfo lat = IPInfo.GeoInfoList.FirstOrDefault(x => x.Parameter == "Latitude");
            IPInfo lon = IPInfo.GeoInfoList.FirstOrDefault(x => x.Parameter == "Longitude");
            string url = $"https://www.latlong.net/c/?lat={lat.Value}&long={lon.Value}";
            try
            {
                _ = Process.Start(url);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Unable to open default browser");

                _ = MessageBox.Show("Unable to open default browser. See the log file",
                                    "ERROR",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
        }

        #endregion PopupBox button events

        #region Keyboard Events
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.E)
                {
                    NavigateToPage(1);
                }

                if (e.Key == Key.I)
                {
                    NavigateToPage(0);
                }

                if (e.Key == Key.M)
                {
                    switch (UserSettings.Setting.DarkMode)
                    {
                        case 0:
                            UserSettings.Setting.DarkMode = 1;
                            break;
                        case 1:
                            UserSettings.Setting.DarkMode = 2;
                            break;
                        case 2:
                            UserSettings.Setting.DarkMode = 0;
                            break;
                    }
                }
                if (e.Key == Key.Add)
                {
                    EverythingLarger();
                }
                if (e.Key == Key.Subtract)
                {
                    EverythingSmaller();
                }
                if (e.Key == Key.OemComma)
                {
                    NavigateToPage(2);
                }
            }

            if (e.Key == Key.F1)
            {
                NavigateToPage(3);
            }
        }
        #endregion Keyboard Events

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            object newValue = prop?.GetValue(sender, null);
            log.Debug($"Setting change: {e.PropertyName} New Value: {newValue}");
            switch (e.PropertyName)
            {
                case nameof(UserSettings.Setting.KeepOnTop):
                    Topmost = (bool)newValue;
                    break;

                case nameof(UserSettings.Setting.IncludeV6):
                    InternalIP.GetMyInternalIP();
                    break;

                case nameof(UserSettings.Setting.IncludeDebug):
                    NLHelpers.SetLogLevel((bool)newValue);
                    break;

                case nameof(UserSettings.Setting.DarkMode):
                    SetBaseTheme((int)newValue);
                    break;

                case nameof(UserSettings.Setting.PrimaryColor):
                    SetPrimaryColor((int)newValue);
                    break;

                case nameof(UserSettings.Setting.UISize):
                    int size = (int)newValue;
                    double newSize = UIScale(size);
                    MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                    break;
            }
        }
        #endregion Setting change

        #region Smaller/Larger
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                EverythingLarger();
            }
            else if (e.Delta < 0)
            {
                EverythingSmaller();
            }
        }

        public void EverythingSmaller()
        {
            int size = UserSettings.Setting.UISize;
            if (size > 0)
            {
                size--;
                UserSettings.Setting.UISize = size;
                double newSize = UIScale(size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            }
        }

        public void EverythingLarger()
        {
            int size = UserSettings.Setting.UISize;
            if (size < 4)
            {
                size++;
                UserSettings.Setting.UISize = size;
                double newSize = UIScale(size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            }
        }
        #endregion Smaller/Larger

        #region Window Title
        public void WindowTitleVersion()
        {
            Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";
        }
        #endregion Window Title

        #region Set light or dark theme
        private static void SetBaseTheme(int mode)
        {
            //Retrieve the app's existing theme
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            switch (mode)
            {
                case 0:
                    theme.SetBaseTheme(Theme.Light);
                    break;
                case 1:
                    theme.SetBaseTheme(Theme.Dark);
                    break;
                case 2:
                    if (GetSystemTheme().Equals("light", StringComparison.OrdinalIgnoreCase))
                    {
                        theme.SetBaseTheme(Theme.Light);
                    }
                    else
                    {
                        theme.SetBaseTheme(Theme.Dark);
                    }
                    break;
                default:
                    theme.SetBaseTheme(Theme.Light);
                    break;
            }

            //Change the app's current theme
            paletteHelper.SetTheme(theme);
        }

        private static string GetSystemTheme()
        {
            BaseTheme? sysTheme = Theme.GetSystemTheme();
            if (sysTheme != null)
            {
                return sysTheme.ToString();
            }
            return string.Empty;
        }
        #endregion Set light or dark theme

        #region Copy to clipboard and text file
        private void CopytoClipBoard()
        {
            StringBuilder sb = ListView2Sb();
            // Clear the clipboard of any previous text
            Clipboard.Clear();
            // Copy to clipboard
            Clipboard.SetText(sb.ToString());
            log.Debug("IP information copied to clipboard");
        }

        private void Copyto2TextFile()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save",
                Filter = "Text File|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = "IP_Info.txt"
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                StringBuilder sb = ListView2Sb();
                File.WriteAllText(dialog.FileName, sb.ToString());
                log.Debug($"IP information written to {dialog.FileName}");
            }
        }

        private StringBuilder ListView2Sb()
        {
            //Get ListView contents and separate parameter and value with a tab
            StringBuilder sb = new StringBuilder();
            foreach (IPInfo item in IPInfo.InternalList)
            {
                sb.Append(item.Parameter).Append('\t').AppendLine(item.Value);
            }
            foreach (IPInfo item in IPInfo.GeoInfoList)
            {
                sb.Append(item.Parameter).Append('\t').AppendLine(item.Value);
            }
            return sb;
        }
        #endregion Copy to clipboard and text file

        #region Unhandled Exception Handler
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            log.Error("Unhandled Exception");
            Exception e = (Exception)args.ExceptionObject;
            log.Error(e.Message);
            if (e.InnerException != null)
            {
                log.Error(e.InnerException.ToString());
            }
            log.Error(e.StackTrace);

            _ = MessageBox.Show("An error has occurred. See the log file",
                                "ERROR",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }
        #endregion Unhandled Exception Handler
    }
}