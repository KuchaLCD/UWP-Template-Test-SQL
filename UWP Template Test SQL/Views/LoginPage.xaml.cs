using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UWP_Template_Test_SQL.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UWP_Template_Test_SQL.Core.Models.UWPTClassLibrary;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Template_Test_SQL.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }
        public class DialogContent
        {
            public string Message { get; set; }
        }
        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Активируем кнопку
                EnterToMainpageButton_Click(sender, e);
            }
        }
        private async void EnterToMainpageButton_Click(object sender, RoutedEventArgs e)
        {
            int controlSum = 0;
            ListsDB.users.Clear();      //предварительное очищение списка пользователелей
            string sqlExpression = "SELECT * FROM Users";
            try
            {
                using (SqlConnection connection = new SqlConnection(DataBase.connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string login = reader.GetString(0);
                        string password = reader.GetString(1);
                        string firstName = reader.GetString(2);
                        string sureName = reader.GetString(3);
                        string idPos = reader.GetString(4);
                        string avatar = reader.GetString(5);
                        if (PassBox.Password == password && LoginBox.Text == login)
                        {
                            controlSum++;
                            UserDB newUser = new UserDB(login, password, firstName, sureName, idPos, avatar);
                            ListsDB.users.Add(newUser);
                            Frame.Navigate(typeof(ShellPage));
                            return;
                        }
                    }

                    reader.Close();
                    if (controlSum < 1 || PassBox.Password == null || LoginBox.Text == string.Empty)
                    {
                        var dialogContent = new DialogContent
                        {
                            Message = "Some content was wrong or input data is non-detected! \nPlease, check for right input data or fill the fields for login/password"
                        };
                        var dialog = new ContentDialog
                        {
                            Title = "Техническая ошибка",
                            ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                            DataContext = dialogContent,
                            PrimaryButtonText = "OK",
                            //Content = "Some content was wrong or input data is non-detected! \nPlease check for right input data or fill the fields for login/password",
                            //SecondaryButtonText = "Cancel"
                            CloseButtonText = "Закрыть"
                        };

                        await dialog.ShowAsync();
                        //MessageDialog dialog = new MessageDialog("Some content was wrong or input data is non-detected! \nPlease check for right input data or fill the fields for login/password.");
                        //await dialog.ShowAsync();
                        return;
                    }
                    PassBox.Password = null;
                    LoginBox.Text = string.Empty;
                }
            }
            catch(Exception ex)
            {
                MessageDialog dialog = new MessageDialog(ex.ToString());
                await dialog.ShowAsync();
            }
        }
    }
}
