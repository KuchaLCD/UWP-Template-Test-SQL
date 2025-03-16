using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UWP_Template_Test_SQL.Core.Models.UWPTClassLibrary;

namespace UWP_Template_Test_SQL.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        public List<Order> ords = new List<Order> { };
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
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно
            //var dialog = new ContentDialog
            //{
            //    Title = "Добавить элемент",
            //    Content = "Введите данные для нового элемента",
            //    PrimaryButtonText = "Добавить",
            //    CloseButtonText = "Отмена"
            //};

            //// Обработка нажатия на кнопку "Добавить"
            //dialog.PrimaryButtonClick += async (s, args) =>
            //{
            //    // Логика добавления элемента
            //    // Например, добавление в DataGrid
            //    var newItem = new Order
            //    {
            //        Id = ords.Count + 1,
            //        Name = "Новый заказ",
            //        Date = DateTime.Now
            //    };

            //    ords.Add(newItem);
            //    //OrdersDataGrid.ItemsSource = ords;
            //};

            //// Показать диалоговое окно
            //await dialog.ShowAsync();
        }
        //public class Order
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //    public DateTime Date { get; set; }
        //}
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        
    }
}
