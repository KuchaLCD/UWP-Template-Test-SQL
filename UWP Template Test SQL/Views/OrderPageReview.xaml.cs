using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class OrderPageReview : Page
    {
        public OrderPageReview()
        {
            this.InitializeComponent();
            int chossen = ManagePage.choose;
            for (int i = 0; i < ListsDB.customers.Count; i++)
            {
                Customers_ItemSource.Items.Add(ListsDB.customers[i].ToString());
            }
            for (int i = 0; i < ListsDB.transports.Count; i++)
            {
                Transport_ItemSource.Items.Add(ListsDB.transports[i].ToString());
            }
            if (chossen == 1)
            {
                Order boofer = ListsDB.orders[0];
                string cust = "";
                string transp = "";
                for (int i = 0; i < ListsDB.customers.Count; i++)
                {
                    if (ListsDB.orders[0].IDCustomer == ListsDB.customers[i].ID) cust = ListsDB.customers[i].ToString();
                }
                for (int i = 0; i < ListsDB.transports.Count; i++)
                {
                    if (ListsDB.orders[0].IDTransp == ListsDB.transports[i].RegisterNumberForPark) transp = ListsDB.transports[i].ToString();
                }
                StartDatePicker.SelectedDate = ListsDB.orders[0].StartRent;
                EndDatePicker.SelectedDate = ListsDB.orders[0].EndRent;
                Customers_ItemSource.SelectedItem = cust;
                Transport_ItemSource.SelectedItem = transp;
            }
        }
        public class DialogContent
        {
            public string Message { get; set; }
        }
        private async void EnterChangesToDataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //int idStore = Convert.ToInt32(ListsDB.storages[StoragePicker.SelectedIndex].id_Store);

                Random randomizer = new Random();
                int randomNumber = randomizer.Next(2000, 9999);

                int idOrd = Convert.ToInt32(randomNumber);
                string idCust = Convert.ToString(ListsDB.customers[Customers_ItemSource.SelectedIndex].ID);

                DateTimeOffset selectedDateOfStartRent = StartDatePicker.Date;
                string formattedDateSt = selectedDateOfStartRent.ToString("dd/MM/yyyy");

                DateTime startRent = Convert.ToDateTime(formattedDateSt);

                DateTimeOffset selectedDateOfEndRent = EndDatePicker.Date;
                string formattedDateEnd = selectedDateOfEndRent.ToString("dd/MM/yyyy");

                DateTime endRent = Convert.ToDateTime(formattedDateEnd);

                int idTransp = Convert.ToInt32(ListsDB.transports[Transport_ItemSource.SelectedIndex].RegisterNumberForPark);
                Order ordD = new Order();
                double bill = ordD.CalculateBill(startRent, endRent);

                Order ord = new Order(idOrd, idCust, startRent, endRent, idTransp, bill);
                ListsDB.orders.Add(ord);
            }
            catch
            {
                //MessageBox.Show("Некоторые поля были не заполнены при внесении в базу!", "Сообщение");
            }

            //Теперь начинаем внесение в базу данных
            try
            {
                int chossen = ManagePage.choose;
                Random randomizer = new Random();
                int randomNumber = randomizer.Next(2000, 9999);

                int idOrd = Convert.ToInt32(randomNumber);
                string idCust = Convert.ToString(ListsDB.customers[Customers_ItemSource.SelectedIndex].ID);
                DateTimeOffset selectedDateOfStartRent = StartDatePicker.Date;
                string formattedDateSt = selectedDateOfStartRent.ToString("dd/MM/yyyy");

                DateTime startRent = Convert.ToDateTime(formattedDateSt);

                DateTimeOffset selectedDateOfEndRent = EndDatePicker.Date;
                string formattedDateEnd = selectedDateOfEndRent.ToString("dd/MM/yyyy");

                DateTime endRent = Convert.ToDateTime(formattedDateEnd);
                int idTransp = Convert.ToInt32(ListsDB.transports[Transport_ItemSource.SelectedIndex].RegisterNumberForPark);
                Order ordD = new Order();
                double bill = ordD.CalculateBill(startRent, endRent);

                ServerOptions sp = new ServerOptions();
                string strInsertOrder = "";
                // Создание SQL команды ввода UPDATE UploadingPoints SET Point_name = '{0}', count_of_upload = '{1}' WHERE id_Uploading_point = '{2}'
                if (chossen == 1)
                {
                    strInsertOrder = string.Format("UPDATE Orders SET IDCust = '{0}', DateOfSR = '{1}', DateOfER = '{2}', RegiisterNumberForPark = '{3}', Billing = '{4}' WHERE IDOrder = '{5}'", idCust, startRent, endRent, idTransp, bill, ListsDB.orders[0].IDOrd);
                    var dialogContentSuccess = new DialogContent
                    {
                        Message = "Заказ был успешно обновлён!"
                    };
                    var dialogSuccess = new ContentDialog
                    {
                        Title = "Успешно выполненный запрос",
                        ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                        DataContext = dialogContentSuccess,
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Закрыть"
                    };
                    await dialogSuccess.ShowAsync();
                }
                else
                {
                    strInsertOrder = string.Format("INSERT INTO Orders VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')", idOrd, idCust, startRent, endRent, idTransp, bill);
                    var dialogContentSuccess = new DialogContent
                    {
                        Message = "Заказ был успешно добавлен!"
                    };
                    var dialogSuccess = new ContentDialog
                    {
                        Title = "Успешно выполненный запрос",
                        ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                        DataContext = dialogContentSuccess,
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Закрыть"
                    };
                    await dialogSuccess.ShowAsync();
                }
                // Создание SQL команды ввода
                //MessageBox.Show("Индекс был успешно добавлен!");
                

                //Тут был добавлен обновлённый механизм для общения с серваком. Но увы, что-то пошло не так
                //await sp.httpRequest(strInsertOrder);

                SqlConnection cn = new SqlConnection();
                cn.ConnectionString = DataBase.connectionString;
                // Открытие подключения
                cn.Open();
                // Создание объекта-команды
                SqlCommand cmdInsertUser = new SqlCommand(strInsertOrder, cn);
                // Исполнение команды ввода
                try
                {
                    cmdInsertUser.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    var dialogContent = new DialogContent
                    {
                        Message = ex.ToString()
                    };
                    var dialog = new ContentDialog
                    {
                        Title = "Техническая ошибка",
                        ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                        DataContext = dialogContent,
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Закрыть"
                    };
                    await dialog.ShowAsync();
                }
                cn.Close();
            }
            catch
            {
                //MessageBox.Show("Some content was wrong or not input");
            }
        }
    }
}
