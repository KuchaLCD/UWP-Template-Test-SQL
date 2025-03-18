using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWP_Template_Test_SQL.Core.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Net.Mime.MediaTypeNames;
using static UWP_Template_Test_SQL.Core.Models.UWPTClassLibrary;
using static UWP_Template_Test_SQL.Views.MainPage;
using ClosedXML.Excel;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Template_Test_SQL.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ManagePage : Page
    {
        //public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

        public static int choose = -1; //Опционально, но может понадобится
                                       //0 - Customers
                                       //1 - Orders
                                       //2 - Transport (Bus, Car and Motocicle)
                                       //3 - Agent
                                       //4 - Positions
                                       //5 - Indexes
        public ManagePage()
        {
            this.InitializeComponent();
            UpdateOrdersInDataGrid();
            UpdateTranspInDataGrid();
            UpdateCustomersInDataGrid();
        }
        public class DialogContent //кастомное представление для вывода DialogContent для DataContext
        {
            public string Message { get; set; }
        }

        #region Кнопки контекстного меню окна Заказов и выборка
        private void UpdateOrdersInDataGrid()
        {
            ListsDB.orders.Clear();
            SqlConnection cn = new SqlConnection();     // Объект-соединение
            cn.ConnectionString = DataBase.connectionString;
            // Открытие подключения
            cn.Open();
            // Формирование команды на языке SQL для выборки данных из таблицы
            string strSelectOrder = "Select * From Orders";
            SqlCommand cmdSelectOrder = new SqlCommand(strSelectOrder, cn);
            SqlDataReader orderDataReader = cmdSelectOrder.ExecuteReader();
            while (orderDataReader.Read())
            {
                int id = orderDataReader.GetInt32(0);
                string idCust = orderDataReader.GetString(1);
                DateTime startRent = Convert.ToDateTime(orderDataReader.GetString(2));
                DateTime endRent = Convert.ToDateTime(orderDataReader.GetString(3));
                int idTransp = orderDataReader.GetInt32(4);
                double bill = orderDataReader.GetDouble(5);
                // Формирование очередного объекта и помещение его в коллекцию
                Order ord = new Order(id, idCust, startRent, endRent, idTransp, bill);
                ListsDB.orders.Add(ord);
            }
            // Закрытие соединения
            cn.Close();
            DataGridOrd.ItemsSource = ListsDB.orders;
        }
        private void OrdEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            choose = 1;
            Order booferOrd = (Order)DataGridOrd.SelectedItem;
            ListsDB.orders.Clear();
            ListsDB.orders.Add(booferOrd);
            Frame.Navigate(typeof(OrderPageReview));
        }
        private async void OrdDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Order ord = (Order)DataGridOrd.SelectedItem;
            var dialogContent = new DialogContent
            {
                Message = $"Вы действительно хотите удалить {ord.ToString()}?"
            };
            var dialog = new ContentDialog
            {
                Title = "Подтверждение удаления",
                ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                DataContext = dialogContent,
                PrimaryButtonText = "Да",
                CloseButtonText = "Нет"
            };

            // Подписываемся на событие нажатия на кнопку "Да"
            dialog.PrimaryButtonClick += async (s, args) =>
            {
                // Закрываем текущий диалог
                dialog.Hide();

                // Вызываем метод для открытия нового диалога
                await OrdDialog_PrimaryButtonClick();
            };

            await dialog.ShowAsync(); // Ожидаем закрытия диалога
            UpdateOrdersInDataGrid();
        }

        private async Task OrdDialog_PrimaryButtonClick()
        {
            // Получаем выбранный элемент
            Order booferCust = (Order)DataGridOrd.SelectedItem;

            int id = Convert.ToInt32(booferCust.IDOrd);

            ListsDB.orders.Clear();
            ListsDB.orders.Add(booferCust);

            ServerOptions sp = new ServerOptions();
            string strDeleteCust = string.Format($"DELETE FROM Orders WHERE (IDOrder = '{id}')");
            //await sp.httpRequest(strDeleteCust);
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = DataBase.connectionString;
            // Открытие подключения
            cn.Open();
            // Создание объекта-команды
            SqlCommand cmdInsertUser = new SqlCommand(strDeleteCust, cn);
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
                await dialog.ShowAsync(); // Ожидаем закрытия диалога
            }
            cn.Close();

            var dialogContentSuccess = new DialogContent
            {
                Message = "Заказ был успешно удалён!"
            };
            var dialogSuccess = new ContentDialog
            {
                Title = "Успешно выполненный запрос",
                ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                DataContext = dialogContentSuccess,
                PrimaryButtonText = "OK",
                CloseButtonText = "Закрыть"
            };
            await dialogSuccess.ShowAsync(); // Ожидаем закрытия диалога
        }
        private async void OrdInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Order ord = (Order)DataGridOrd.SelectedItem;
                var dialogContent = new DialogContent
                {
                    Message = ord.InfoString()
                };
                var dialog = new ContentDialog
                {
                    Title = "Вывод информации",
                    ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
                    DataContext = dialogContent,
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Закрыть"
                };

                await dialog.ShowAsync();
            }
            catch(Exception ex)
            {
                MessageDialog error = new MessageDialog(ex.ToString());
                await error.ShowAsync();
            }
        }
        #endregion

        #region Кнопки контекстного меню окна Транспорта и выборка
        private void UpdateTranspInDataGrid()
        {
            ListsDB.transports.Clear();
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = DataBase.connectionString;
            cn.Open();
            string strSelectTransport = "Select * From Transport";
            SqlCommand cmdSelectTransport = new SqlCommand(strSelectTransport, cn);

            SqlDataReader transportsDataReader = cmdSelectTransport.ExecuteReader();
            while (transportsDataReader.Read())
            {
                int id = transportsDataReader.GetInt32(0);
                string naming = transportsDataReader.GetString(1);
                double maxSpeed = transportsDataReader.GetDouble(2);
                double volumeOfEngine = transportsDataReader.GetDouble(3);
                double mass = transportsDataReader.GetDouble(4);
                double whidth = transportsDataReader.GetDouble(5);
                int wheelCount = transportsDataReader.GetInt32(6);
                DateTime timeOfRegistrForPark = Convert.ToDateTime(transportsDataReader.GetString(7));
                DateTime stayTime = Convert.ToDateTime(transportsDataReader.GetString(8));
                string roadNumber = transportsDataReader.GetString(9);
                string picture = transportsDataReader.GetString(10);
                int idAgent = transportsDataReader.GetInt32(11);
                string notes = transportsDataReader.GetString(12);

                // Формирование очередного объекта и помещение его в коллекцию
                if (maxSpeed == 0 && volumeOfEngine == 0 && wheelCount == 0 && roadNumber == "0")
                {
                    Transport tr = new Transport(id, naming, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes);
                    ListsDB.transports.Add(tr);
                }
                else if (wheelCount == 2)
                {
                    Motocicle moto = new Motocicle(volumeOfEngine, maxSpeed, roadNumber, naming, id, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes);
                    ListsDB.transports.Add(moto);
                }
                else if (wheelCount >= 6)
                {
                    Bus bus = new Bus(volumeOfEngine, maxSpeed, roadNumber, naming, id, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes, wheelCount);
                    ListsDB.transports.Add(bus);
                }
                else
                {
                    Car car = new Car(volumeOfEngine, maxSpeed, roadNumber, naming, id, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes);
                    ListsDB.transports.Add(car);
                }
            }

            // Закрытие соединения
            cn.Close();
            DataGridTransp.ItemsSource = ListsDB.transports;
        }
        private async void TranspEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            choose = 2;
            var booferTransp = DataGridTransp.SelectedItem;
            ListsDB.transports.Clear();

            switch (booferTransp)
            {
                case Car:
                    ListsDB.transports.Add((Car)booferTransp);
                    //CarRegister car = new CarRegister();
                    //car.ShowDialog();
                    break;
                case Bus:
                    ListsDB.transports.Add((Bus)booferTransp);
                    //BusRegister bus = new BusRegister();
                    //bus.ShowDialog();
                    break;
                case Motocicle:
                    ListsDB.transports.Add((Motocicle)booferTransp);
                    //MotoRegister moto = new MotoRegister();
                    //moto.ShowDialog();
                    break;
                default:
                    var dialogContent = new DialogContent
                    {
                        Message = "Неизвестный или базовый тип"
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
                    break;
            }
            //Order booferOrd = (Order)DataGridOrd.SelectedItem;
            //ListsDB.orders.Clear();
            //ListsDB.orders.Add(booferOrd);
            //Frame.Navigate(typeof(OrderPageReview));
        }

        private void TranspDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void TranspInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    Order ord = (Order)DataGridOrd.SelectedItem;
            //    var dialogContent = new DialogContent
            //    {
            //        Message = ord.InfoString()
            //    };
            //    var dialog = new ContentDialog
            //    {
            //        Title = "Вывод информации",
            //        ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
            //        DataContext = dialogContent,
            //        PrimaryButtonText = "OK",
            //        CloseButtonText = "Закрыть"
            //    };

            //    await dialog.ShowAsync();
            //}
            //catch (Exception ex)
            //{
            //    MessageDialog error = new MessageDialog(ex.ToString());
            //    await error.ShowAsync();
            //}
        }
        #endregion

        #region Кнопки контекстного меню окна Клиентов и выборка
        private void UpdateCustomersInDataGrid()
        {
            ListsDB.customers.Clear();
            SqlConnection cn = new SqlConnection();     // Объект-соединение
            cn.ConnectionString = DataBase.connectionString;
            // Открытие подключения
            cn.Open();
            // Формирование команды на языке SQL для выборки данных из таблицы
            string strSelectCust = "Select * From Customer";
            SqlCommand cmdSelectCust = new SqlCommand(strSelectCust, cn);
            SqlDataReader custDataReader = cmdSelectCust.ExecuteReader();
            while (custDataReader.Read())
            {
                string id = custDataReader.GetString(0);
                string fname = custDataReader.GetString(1);
                string sname = custDataReader.GetString(2);
                string phone = custDataReader.GetString(3);
                string passport = custDataReader.GetString(4);
                // Формирование очередного объекта и помещение его в коллекцию
                Customer cust = new Customer(id, fname, sname, phone, passport);
                ListsDB.customers.Add(cust);
            }
            // Закрытие соединения
            cn.Close();
            DataGridCust.ItemsSource = ListsDB.customers;
        }
        private void CustEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            choose = 0;
            //Order booferOrd = (Order)DataGridOrd.SelectedItem;
            //ListsDB.orders.Clear();
            //ListsDB.orders.Add(booferOrd);
            //Frame.Navigate(typeof(OrderPageReview));
        }

        private void CustDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void CustInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    Order ord = (Order)DataGridOrd.SelectedItem;
            //    var dialogContent = new DialogContent
            //    {
            //        Message = ord.InfoString()
            //    };
            //    var dialog = new ContentDialog
            //    {
            //        Title = "Вывод информации",
            //        ContentTemplate = (DataTemplate)Resources["CustomDialogTemplate"],
            //        DataContext = dialogContent,
            //        PrimaryButtonText = "OK",
            //        CloseButtonText = "Закрыть"
            //    };

            //    await dialog.ShowAsync();
            //}
            //catch (Exception ex)
            //{
            //    MessageDialog error = new MessageDialog(ex.ToString());
            //    await error.ShowAsync();
            //}
        }
        #endregion

        #region Добавление элементов из меню Менеджера ресурсов
        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            choose = -1;
            Frame.Navigate(typeof(OrderPageReview));
        }
        private void AddTranspButton_Click(object sender, RoutedEventArgs e)
        {
            //choose = -1;
            //Frame.Navigate(typeof(OrderPageReview));
        }
        #endregion

        #region Кнопки вывода инфы по всем ключевым объектам
        private async void OutputDocButton_Click(object sender, RoutedEventArgs e)
        {
            // Путь к файлу
            //string filePath = @"C:\Users\CATAT\OneDrive\Documents\Приказ_о_введении_автобуса_в_эксплуатацию_8228.doc";

            // Открываем диалог выбора файла (рабочий метод. Можно редачить вообще ЛЮБУЮ ячейку в .xlsx)
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".xlsx");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    // Создаем временный поток для сохранения данных
                    using (var memoryStream = new MemoryStream())
                    {
                        // Загружаем файл Excel в память
                        using (var fileStream = await file.OpenStreamForReadAsync())
                        {
                            fileStream.CopyTo(memoryStream);
                        }

                        // Работаем с файлом в памяти
                        using (var workbook = new XLWorkbook(memoryStream))
                        {
                            var worksheet = workbook.Worksheet(1); // Получаем первый лист

                            // Вставляем данные в ячейку
                            if (DataGridTransp.SelectedIndex >= 0 && DataGridTransp.SelectedIndex < ListsDB.transports.Count)
                            {
                                worksheet.Cell(14, 1).Value = $"1. Автобус {ListsDB.transports[DataGridTransp.SelectedIndex].Naming}";
                                worksheet.Cell(14, 4).Value = $" с {ListsDB.transports[DataGridTransp.SelectedIndex].TimeOfRegistrForPark}";
                            }
                            else
                            {
                                worksheet.Cell(14, 1).Value = "1. Автобус (не выбран)";
                            }

                            // Сохраняем изменения в поток памяти
                            workbook.Save();
                        }

                        // Перезаписываем файл из памяти
                        using (var fileStream = await file.OpenStreamForWriteAsync())
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin); // Сбрасываем позицию потока
                            await memoryStream.CopyToAsync(fileStream);
                        }
                    }

                    // Показываем сообщение об успешном сохранении
                    var dialog = new MessageDialog("Файл успешно сохранён.", "Успех");
                    await dialog.ShowAsync();

                    // Открываем сохранённый файл
                    await Windows.System.Launcher.LaunchFileAsync(file);
                }
                catch (Exception ex)
                {
                    var dialog = new MessageDialog($"Ошибка при сохранении файла: {ex.Message}", "Ошибка");
                    await dialog.ShowAsync();
                }
            }
        }
        #endregion
    }
}
