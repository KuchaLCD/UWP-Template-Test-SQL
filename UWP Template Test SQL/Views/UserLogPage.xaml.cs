using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static UWP_Template_Test_SQL.Core.Models.UWPTClassLibrary;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Diagnostics;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Template_Test_SQL.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserLogPage : Page
    {
        public UserLogPage()
        {
            this.InitializeComponent();
            
            string IDPos = string.Empty;
            UserName.Text = ListsDB.users[0].ToString();
            string pos = ListsDB.users[0].IDPos;
            string sqlExpressionPos = "SELECT IDPos, PosName FROM Positions";

            using (SqlConnection connection = new SqlConnection(DataBase.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlExpressionPos, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    IDPos = reader.GetString(0);
                    string posName = reader.GetString(1);

                    if (pos == IDPos)
                    {
                        ProfCategory.Text = posName;
                        break;
                    }
                }
                reader.Close();
            }
            LoadAvatar();
        }
        private async void LoadAvatar()
        {
            string avt = ListsDB.users[0].AvatarPicture; // Убедитесь, что это корректный URI
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(avt);
                using (var stream = await file.OpenReadAsync())
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    UserAvatar.ProfilePicture = bitmapImage;
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            }
        }
        private async void Change_Avatar_Button_Click(object sender, RoutedEventArgs e)
        {
            string pos = ListsDB.users[0].IDPos;
            // Выбираем файл
            string userPicture = await PickImageAsync();

            // Если файл выбран, обновляем аватарку
            if (!string.IsNullOrEmpty(userPicture))
            {
                // Загружаем изображение в UserAvatar
                StorageFile file = await StorageFile.GetFileFromPathAsync(userPicture);
                using (var stream = await file.OpenReadAsync())
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    UserAvatar.ProfilePicture = bitmapImage;
                    string strInsertUser = string.Format($"UPDATE Users SET AvatarPicture = '{userPicture}' WHERE IDPos = {pos}");
                    SqlConnection connection = new SqlConnection(DataBase.connectionString);
                    connection.Open();
                    SqlCommand command = new SqlCommand(strInsertUser, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private async Task<string> PickImageAsync()
        {
            // Создаем экземпляр FileOpenPicker
            FileOpenPicker openPicker = new FileOpenPicker();

            // Указываем начальную локацию (например, библиотека изображений)
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            // Добавляем фильтр для файлов (например, только JPG)
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Открываем диалог выбора файла
            StorageFile file = await openPicker.PickSingleFileAsync();

            // Если файл выбран, возвращаем его путь
            if (file != null)
            {
                return file.Path;
            }

            // Если файл не выбран, возвращаем пустую строку
            return string.Empty;
        }
    }
}
