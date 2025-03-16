using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UWP_Template_Test_SQL.Helpers;
using UWP_Template_Test_SQL.Services;

using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UWP_Template_Test_SQL.Views
{
    // TODO: Add other settings as necessary. For help see https://github.com/microsoft/TemplateStudio/blob/main/docs/UWP/pages/settings-codebehind.md
    // TODO: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public bool IsEnglish { get; set; }
        public bool IsRussian { get; set; }
        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        public SettingsPage()
        {
            InitializeComponent();
            SetLanguageForChecked();
        }
        public void SetLanguageForChecked()
        {
            if (IsEnglish)
            {
                EnglishRadiobutton.IsChecked = true;
            }
            else
            {
                EnglishRadiobutton.IsChecked = false;
            }
        }
        private void LanguageChanged_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton?.CommandParameter is string language)
            {
                // Установка нового языка
                ApplicationLanguages.PrimaryLanguageOverride = language;
                
                // Обновление интерфейса
                var rootFrame = Window.Current.Content as Frame;
                rootFrame?.Navigate(rootFrame.CurrentSourcePageType);
                if (language == "ru-RU")
                {
                    IsEnglish = false;
                    IsRussian = true;
                }
                else
                {
                    IsEnglish = true;
                    IsRussian = false;
                }
                rootFrame?.GoBack(); // Возврат на текущую страницу для обновления
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null)
            {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
