using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static UWP_Template_Test_SQL.Core.Models.UWPTClassLibrary;

namespace UWP_Template_Test_SQL.Core.Models
{
    public class UWPTClassLibrary
    {
        public static string servermessage = "";
        public static string error = "";
        public class DataBase
        {
            //"User ID=bobas.lab7;" + "Password=KYKKV" 
            //public static string connectionString = @"Data Source=LAPTOP-6BNSAOJB;" + "Initial Catalog=AutoPark;" + "Integrated Security=true;";    //Это старая строка
            public static string connectionString = @"Data Source=LAPTOP-6BNSAOJB;" + "Initial Catalog=AutoPark;" + "User ID=UWPtest;" + "Password=1234;";      //Это новая сторка с немного большим функционалом.
                                                                                                                                                                //В UWP чтобы войти в систему понадобится настроеный свободный акк SQL в базе,
                                                                                                                                                                //которую мы используем и подключится можно только так.
                                                                                                                                                                //Внимательней при её изменении
            SqlConnection sqlConnectionString = new SqlConnection(connectionString);

            public void openConnection()
            {
                if (sqlConnectionString.State == System.Data.ConnectionState.Closed)
                    sqlConnectionString.Open();
            }
            public void closeConnection()
            {
                if (sqlConnectionString.State == System.Data.ConnectionState.Open)
                    sqlConnectionString.Close();
            }
            public SqlConnection getConnection()
            {
                return sqlConnectionString;
            }
        }
        public class ServerOptions
        {
            public async Task httpRequest()
            {
                string responseString = "";
                string message = "";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        message = servermessage;

                        var content = new StringContent(message, Encoding.UTF8, "text/plain");
                        HttpResponseMessage response = await client.PostAsync("http://localhost:8080/", content);

                        responseString = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            public ServerOptions()
            {

            }
        }
        public class UserDB
        {
            public string Login { get; }
            public string Password { get; }
            public string FirstName { get; }
            public string SureName { get; }
            public string IDPos { get; }
            public string AvatarPicture { get; }
            public override string ToString()
            {
                string st = string.Format("{0} {1}", FirstName, SureName);
                return st;
            }
            public UserDB(string login, string password, string firstName, string sureName, string idPos)
            {
                this.Login = login;
                this.Password = password;
                this.FirstName = firstName;
                this.SureName = sureName;
                this.IDPos = idPos;
            }
            public UserDB(string login, string password, string firstName, string sureName, string idPos, string avatarPicture)
            {
                this.Login = login;
                this.Password = password;
                this.FirstName = firstName;
                this.SureName = sureName;
                this.IDPos = idPos;
                this.AvatarPicture = avatarPicture;
            }
            public UserDB()
            {

            }
        }
        #region Транспорт, его наследники и ПАРК
        public interface ICalc
        {
            string CalculateOwn();
            string CalculateIncome();
        }
        public class Transport : ICalc
        {
            public int RegisterNumberForPark { get; }
            public string Naming { get; set; }
            public double Mass { get; }
            public double Whidth { get; set; }
            public DateTime TimeOfRegistrForPark { get; set; }
            public DateTime StayTime { get; set; }
            public string Picture { get; set; }
            public int id_Agent { get; set; }
            public string Notes { get; }

            public virtual string InfoString()
            {
                string inf = $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::\n---Транспорт---\nНаименование: {Naming}" +
                             $"\nНомер регистрации в парке: {RegisterNumberForPark}" +
                             $"\nВремя регистрации в парке: {TimeOfRegistrForPark}" +
                             $"\nВремя пребывания(до): {StayTime}" +
                             $"\nМасса: {Mass} кг." +
                             $"\nШирина: {Whidth} м." +
                             $"\nПримечания: {Notes}" +
                             $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::";

                return inf;
            }
            public double Index_Bill_for_Hour()
            {
                int id = 2;
                double billForHour = 2;
                SqlConnection cn = new SqlConnection();
                cn.ConnectionString = DataBase.connectionString;
                cn.Open();
                string strSelectTransport = $"Select * From Indexes WHERE (id_Index = '{id}')";
                SqlCommand cmdSelectTransport = new SqlCommand(strSelectTransport, cn);

                SqlDataReader transportsDataReader = cmdSelectTransport.ExecuteReader();
                while (transportsDataReader.Read())
                {
                    billForHour = transportsDataReader.GetDouble(2);
                }
                // Закрытие соединения
                cn.Close();
                return billForHour;
            }
            public string CalculateOwn()
            {
                double Count1 = TimeOfRegistrForPark.Year * 8760 + TimeOfRegistrForPark.Month * 720 + TimeOfRegistrForPark.Day * 24 + TimeOfRegistrForPark.Hour + TimeOfRegistrForPark.Minute * 0.017 + TimeOfRegistrForPark.Second * 0.00028;
                double Count2 = StayTime.Year * 8760 + StayTime.Month * 720 + StayTime.Day * 24 + StayTime.Hour + StayTime.Minute * 0.017 + StayTime.Second * 0.00028;
                double billForHour = Index_Bill_for_Hour();
                double hours = Count2 - Count1;
                double result = billForHour * hours;
                string inf = $"Совокупное количество часов стоянки для выбранного транспорта = {hours} ч." +
                             $"\nСумма стоянки = {result} BYN";
                return inf;
            }
            public string CalculateIncome()
            {
                double k = 0;       // Число для подсчёта транспорта в парке 
                double billForHour = Index_Bill_for_Hour();
                double Count1 = 0;      //промежуточное значение
                double Count2 = 0;      //и это тоже
                                        //Этот "сложный" алгоритм считает общее количество часов 
                for (int i = 0; i < ListsDB.transports.Count; i++)
                {
                    Count1 += ListsDB.transports[i].TimeOfRegistrForPark.Year * 8760 + ListsDB.transports[i].TimeOfRegistrForPark.Month * 720 + ListsDB.transports[i].TimeOfRegistrForPark.Day * 24 + ListsDB.transports[i].TimeOfRegistrForPark.Hour + ListsDB.transports[i].TimeOfRegistrForPark.Minute * 0.017 + ListsDB.transports[i].TimeOfRegistrForPark.Second * 0.00028;
                    Count2 += ListsDB.transports[i].StayTime.Year * 8760 + ListsDB.transports[i].StayTime.Month * 720 + ListsDB.transports[i].StayTime.Day * 24 + ListsDB.transports[i].StayTime.Hour + ListsDB.transports[i].StayTime.Minute * 0.017 + ListsDB.transports[i].StayTime.Second * 0.00028;
                    k++;
                }
                //Считаем часы и результат ("дата пребытия в часах" - "дата отъезда")
                double hours = Count2 - Count1;
                double result = billForHour * hours;
                string inf = $"Совокупное количество транспорта в парке = {k}" +
                             $"\nВыручка от деятельности парка = {result} BYN";
                return inf;
            }
            public string OurIncome()
            {
                double ourIncome = 0;
                double k = 0;       // Число для подсчёта транспорта в парке 
                double billForHour = Index_Bill_for_Hour();
                double Count1 = 0;      //промежуточное значение
                double Count2 = 0;      //и это тоже
                                        //Этот "сложный" алгоритм считает общее количество часов 
                for (int i = 0; i < ListsDB.transports.Count; i++)
                {
                    Count1 += ListsDB.transports[i].TimeOfRegistrForPark.Year * 8760 + ListsDB.transports[i].TimeOfRegistrForPark.Month * 720 + ListsDB.transports[i].TimeOfRegistrForPark.Day * 24 + ListsDB.transports[i].TimeOfRegistrForPark.Hour + ListsDB.transports[i].TimeOfRegistrForPark.Minute * 0.017 + ListsDB.transports[i].TimeOfRegistrForPark.Second * 0.00028;
                    Count2 += ListsDB.transports[i].StayTime.Year * 8760 + ListsDB.transports[i].StayTime.Month * 720 + ListsDB.transports[i].StayTime.Day * 24 + ListsDB.transports[i].StayTime.Hour + ListsDB.transports[i].StayTime.Minute * 0.017 + ListsDB.transports[i].StayTime.Second * 0.00028;
                    k++;
                }
                //Считаем часы и результат ("дата пребытия в часах" - "дата отъезда")
                double hours = Count2 - Count1;
                double result = billForHour * hours;
                for (int i = 0; i < ListsDB.orders.Count; i++)
                {
                    ourIncome += ListsDB.orders[i].Bill;
                }
                double SalarySum = 0;
                double salary = 0;
                SqlConnection cn = new SqlConnection();
                cn.ConnectionString = DataBase.connectionString;
                cn.Open();
                string strSelectTransport = "select index_countable from Indexes Where (id_Index IN (1,3,4,5,6))";
                SqlCommand cmdSelectTransport = new SqlCommand(strSelectTransport, cn);
                SqlDataReader transportsDataReader = cmdSelectTransport.ExecuteReader();
                while (transportsDataReader.Read())
                {
                    salary = transportsDataReader.GetDouble(0);
                    SalarySum += salary;
                }
                // Закрытие соединения
                cn.Close();
                return ListsDB.transports[0].CalculateIncome() + $"\nВыручка с учётом заказов {ourIncome + result} бел. руб." + $"\nСовокупная прибыль с учётом издержек = {ourIncome + result - SalarySum} бел. руб.";
            }
            public override string ToString()
            {
                string st = string.Format("Транспорт {0}, ID {1}", Naming, RegisterNumberForPark);
                return st;
            }
            public Transport(int registerNumberForPark, string naming, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, int idAgent, string notes)
            {
                this.RegisterNumberForPark = registerNumberForPark;
                this.Naming = naming;
                this.Mass = mass;
                this.Whidth = whidth;
                TimeOfRegistrForPark = timeOfRegistrForPark;
                StayTime = stayTime;
                this.Picture = picture;
                this.id_Agent = idAgent;
                this.Notes = notes;
            }
            public Transport(int registerNumberForPark, string naming, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, string notes)
            {
                this.RegisterNumberForPark = registerNumberForPark;
                this.Naming = naming;
                this.Mass = mass;
                this.Whidth = whidth;
                TimeOfRegistrForPark = timeOfRegistrForPark;
                StayTime = stayTime;
                this.Picture = picture;
                this.Notes = notes;
            }
            public Transport()
            {

            }
        }
        public class EngineTransport : Transport
        {
            double maxSpeed; //максимальная скорость (в км/ч)
            double volumeOfEngine;  //объём двигателя (в см. куб.)
            string roadNumber; //дорожные номера. Пример(белорусские): 1231 AD-2
            public string RoadNumber { get { return roadNumber; } }
            public double VolumeOfEngine { get { return volumeOfEngine; } }
            public double MaxSpeed { get { return maxSpeed; } }
            public EngineTransport(double volumeOfEngine, double maxSpeed, string roadNumber, string naming, int registerNumberForPark, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, int idAgent, string notes)
                : base(registerNumberForPark, naming, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes)
            {
                this.roadNumber = roadNumber;
                this.maxSpeed = maxSpeed;
                this.volumeOfEngine = volumeOfEngine;
            }
            public EngineTransport() { }
        }
        public class Bus : EngineTransport
        {
            public double CountOfWheels { get; set; }
            public Bus(double volumeOfEngine, double maxSpeed, string roadNumber, string naming, int registerNumberForPark, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, int idAgent, string notes, double countOfWhells)
                : base(volumeOfEngine, maxSpeed, roadNumber, naming, registerNumberForPark, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes)
            {
                this.CountOfWheels = countOfWhells;
            }
            public override string ToString()
            {
                string st = string.Format("Автобус {0}, ID {1}", Naming, RegisterNumberForPark);
                return st;
            }
            public override string InfoString()
            {
                string agName = "";
                for (int i = 0; i < ListsDB.agents.Count; i++)
                {
                    if (ListsDB.agents[i].id_agent == id_Agent) agName = ListsDB.agents[i].agent_name;
                }
                string inf = $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::\n---Автобус---\nНаименование: {Naming}" +
                             $"\nНомер регистрации в парке: {RegisterNumberForPark}" +
                             $"\nВремя регистрации в парке: {TimeOfRegistrForPark}" +
                             $"\nНомера: {RoadNumber}" +
                             $"\n---Дополнительная информация---\nОбъём двигателя: {VolumeOfEngine} см. куб." +
                             $"\nМаксимальная скорость: {MaxSpeed} км/ч" +
                             $"\nШирина: {Whidth} м." +
                             $"\nВес: {Mass} кг." +
                             $"\nКоличество колёс: {CountOfWheels}" +
                             $"\nВремя пребывания(до): {StayTime}" +
                             $"\nАгент: {agName}" +
                             $"\nПримечания: {Notes}" +
                             $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::";

                return inf;
            }
        }
        public class Car : EngineTransport
        {
            public Car(double volumeOfEngine, double maxSpeed, string roadNumber, string naming, int registerNumberForPark, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, int idAgent, string notes)
                : base(volumeOfEngine, maxSpeed, roadNumber, naming, registerNumberForPark, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes)
            { }
            public override string ToString()
            {
                string st = string.Format("Машина {0}, ID {1}", Naming, RegisterNumberForPark);
                return st;
            }
            public override string InfoString()
            {
                string agName = "";
                for (int i = 0; i < ListsDB.agents.Count; i++)
                {
                    if (ListsDB.agents[i].id_agent == id_Agent) agName = ListsDB.agents[i].agent_name;
                }
                string inf = $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::\n---Машина(Автомобиль)---\nНаименование: {Naming}" +
                             $"\nНомер регистрации в парке: {RegisterNumberForPark}" +
                             $"\nВремя регистрации в парке: {TimeOfRegistrForPark}" +
                             $"\nНомера: {RoadNumber}" +
                             $"\n---Дополнительная информация---\nОбъём двигателя: {VolumeOfEngine} см. куб." +
                             $"\nМаксимальная скорость: {MaxSpeed} км/ч" +
                             $"\nШирина: {Whidth} м." +
                             $"\nВес: {Mass} кг." +
                             $"\nВремя пребывания(до): {StayTime}" +
                             $"\nАгент: {agName}" +
                             $"\nПримечания: {Notes}" +
                             $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::";

                return inf;
            }
        }
        public class Motocicle : EngineTransport
        {
            public Motocicle(double volumeOfEngine, double maxSpeed, string roadNumber, string naming, int registerNumberForPark, double mass, double whidth, DateTime timeOfRegistrForPark, DateTime stayTime, string picture, int idAgent, string notes)
              : base(volumeOfEngine, maxSpeed, roadNumber, naming, registerNumberForPark, mass, whidth, timeOfRegistrForPark, stayTime, picture, idAgent, notes)
            { }
            public override string ToString()
            {
                string st = string.Format("Мотоцикл {0}, ID {1}", Naming, RegisterNumberForPark);
                return st;
            }
            public override string InfoString()
            {
                string agName = "";
                for (int i = 0; i < ListsDB.agents.Count; i++)
                {
                    if (ListsDB.agents[i].id_agent == id_Agent) agName = ListsDB.agents[i].agent_name;
                }
                string inf = $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::\n---Мотоцикл---\nНаименование: {Naming}" +
                             $"\nНомер регистрации в парке: {RegisterNumberForPark}" +
                             $"\nВремя регистрации в парке: {TimeOfRegistrForPark}" +
                             $"\nНомера: {RoadNumber}" +
                             $"\n---Дополнительная информация---\nОбъём двигателя: {VolumeOfEngine} см. куб." +
                             $"\nМаксимальная скорость: {MaxSpeed} км/ч" +
                             $"\nШирина: {Whidth} м." +
                             $"\nВес: {Mass} кг." +
                             $"\nВремя пребывания(до): {StayTime}" +
                             $"\nАгент: {agName}" +
                             $"\nПримечания: {Notes}" +
                             $"\n::::::::::::::::::::::::::::::::::::::::::::::::::::::";

                return inf;
            }
        }
        public class Park
        {
            public string Name { get; set; }
            public void AddTrasport(Transport t)
            {
                ListsDB.transports.Add(t);
            }
            public void RemoveTransport(Transport t)
            {
                ListsDB.transports.Remove(t);
            }
            public virtual string About()
            {
                string inf = $"\n++++++++++++++++++++++++++++++++++++++++++++++++++\n---Парк---\nНаименование: {Name}" +
                "\nСодержание парка:";
                for (int i = 0; i < ListsDB.transports.Count; i++)
                {
                    inf += ListsDB.transports[i].InfoString();
                }
                inf += $"\n++++++++++++++++++++++++++++++++++++++++++++++++++";
                return inf;
            }
            public Park(string name, List<Transport> transport)
            {
                this.Name = name;
                ListsDB.transports = transport;
            }
            public static Park park = new Park("LuxuryPark", ListsDB.transports);
        }
        #endregion
        #region Агенты(Поставщики), Закзачики, Заказы и Должности
        public class Agent
        {
            public int id_agent { get; set; }
            public string agent_name { get; set; }
            public string agent_surename { get; set; }
            public string phone_number { get; set; }
            override public string ToString()
            {
                // формирование строки с помощью метода Format() класса string
                string st = string.Format("{0} {1}", agent_name, agent_surename);
                return st;
            }
            public string InfoString()
            {
                string inf = $"\n:::::::::::::*************************::::::::::::::::\n+---Агент---+\nНомер: {id_agent}" +
                             $"\nИмя агента: {agent_name}" +
                             $"\nФамилия агента: {agent_surename}" +
                             $"\nТелефон +375 {phone_number}" +
                             $"\n:::::::::::::*************************::::::::::::::::";

                return inf;
            }
            public Agent(int id, string name, string surename, string phone)
            {
                this.id_agent = id;
                this.agent_name = name;
                this.agent_surename = surename;
                this.phone_number = phone;
            }
        }
        public class Customer
        {
            string id;
            string firstName;
            string sureName;
            string lastName;
            string passport;
            public string ID
            {
                get { return id; }
            }
            public string FirstName
            {
                get { return firstName; }
            }
            public string SureName
            {
                get { return sureName; }
            }
            public string LastName
            {
                get { return lastName; }
            }
            public string PassportDataCust
            {
                get { return passport; }
            }
            // Конструктор
            public Customer(string id, string firstName, string sureName, string lastName, string pass)
            {
                this.id = id;
                this.firstName = firstName;
                this.sureName = sureName;
                this.lastName = lastName;
                this.passport = pass;
            }
            public string InfoString()
            {
                string inf = $"\n:::::::::::::*************************::::::::::::::::\n=======Заказчик========" +
                             $"\nИН Заказчика: {id}" +
                             $"\nИмя: {firstName}" +
                             $"\nФамилия: {sureName}" +
                             $"\nТелефон: + 375 {lastName}" +
                             $"\nПаспортные данные (серия и номер пасспорта): {passport}" +
                             $"\n:::::::::::::*************************::::::::::::::::";
                return inf;
            }
            // Переопределение метода ToString()
            override public string ToString()
            {
                // формирование строки с помощью метода Format() класса string
                string st = string.Format("{0} {1} {2}",
                    firstName.PadRight(10),
                    sureName.ToString().PadRight(12),
                    passport.ToString().PadRight(14));
                return st;
            }
        }
        public class Order
        {
            public int IDOrd { get; }
            public string IDCustomer { get; }
            public DateTime StartRent { get; set; }
            public DateTime EndRent { get; set; }
            public int IDTransp { get; }
            public double Bill { get; set; }
            public override string ToString()
            {
                string st = string.Format("Заказ № {0}, ИН Заказчика: {1}", IDOrd, IDCustomer);
                return st;
            }
            public double CalculateBill(DateTime startRent, DateTime endRent)
            {
                //Вытаскивеам индекс ставки за час нахождения в парке авто
                double ind_countable = 0;
                SqlConnection cn = new SqlConnection();
                cn.ConnectionString = DataBase.connectionString;
                cn.Open();
                string strSelectTransport = "select index_countable from Indexes Where (id_Index = 2)";
                SqlCommand cmdSelectTransport = new SqlCommand(strSelectTransport, cn);
                SqlDataReader transportsDataReader = cmdSelectTransport.ExecuteReader();
                while (transportsDataReader.Read())
                {
                    ind_countable = transportsDataReader.GetDouble(0);
                }
                // Закрытие соединения
                cn.Close();
                //double billForHour = 5;
                double Count1 = 0;      //промежуточное значение
                double Count2 = 0;      //и это тоже
                                        //Этот "сложный" алгоритм считает общее количество часов 
                Count1 += startRent.Year * 8760 + startRent.Month * 720 + startRent.Day * 24 + startRent.Hour + startRent.Minute * 0.017 + startRent.Second * 0.00028;
                Count2 += endRent.Year * 8760 + endRent.Month * 720 + endRent.Day * 24 + endRent.Hour + endRent.Minute * 0.017 + endRent.Second * 0.00028;
                //Считаем часы и результат ("дата пребытия в часах" - "дата отъезда")
                double hours = Count2 - Count1;
                double result = ind_countable * hours;
                return result;
            }
            public string InfoString()
            {
                string custName = "";
                string transpName = "";
                EngineTransport booferTr = new EngineTransport();
                for (int i = 0; i < ListsDB.customers.Count; i++)
                {
                    if (ListsDB.customers[i].ID == IDCustomer) custName = ListsDB.customers[i].FirstName + " " + ListsDB.customers[i].SureName;
                }
                for (int i = 0; i < ListsDB.transports.Count; i++)
                {
                    if (ListsDB.transports[i].RegisterNumberForPark == IDTransp)
                    {
                        booferTr = (EngineTransport)ListsDB.transports[i];
                        transpName = ListsDB.transports[i].Naming + " " + booferTr.RoadNumber;
                    }
                }
                double ind_countable = 0;
                SqlConnection cn = new SqlConnection();
                cn.ConnectionString = DataBase.connectionString;
                cn.Open();
                string strSelectTransport = "select index_countable from Indexes Where (id_Index = 2)";
                SqlCommand cmdSelectTransport = new SqlCommand(strSelectTransport, cn);
                SqlDataReader transportsDataReader = cmdSelectTransport.ExecuteReader();
                while (transportsDataReader.Read())
                {
                    ind_countable = transportsDataReader.GetDouble(0);
                }
                // Закрытие соединения
                cn.Close();
                string inf = $"\n:::::::::::::*************************::::::::::::::::\n$---Заказ---$\nНомер: {IDOrd}" +
                             $"\nЗаказчик: {custName}" +
                             $"\nНачало аренды: {StartRent}" +
                             $"\nОкончание аренды: {EndRent}" +
                             $"\nТранспорт: {transpName}" +
                             $"\nИтог к оплате: {Bill} бел. руб." +
                             $"\nЦена за час стоянки: {ind_countable} бел. руб." +
                             $"\n:::::::::::::*************************::::::::::::::::";

                return inf;
            }

            public Order(int idOrd, string idCust, DateTime startRent, DateTime endRent, int idTr, double bill)
            {
                this.IDOrd = idOrd;
                this.IDCustomer = idCust;
                this.StartRent = startRent;
                this.EndRent = endRent;
                this.IDTransp = idTr;
                this.Bill = bill;
            }
            public Order()
            {

            }
        }
        public class Positions
        {
            string idPos;
            string name;
            int salary;
            public string ID
            {
                get { return idPos; }
            }
            public string Name
            {
                get { return name; }
            }
            public int Salary
            {
                get { return salary; }
                set { salary = value; }
            }
            // Конструктор
            public Positions(string id, string name, int salary)
            {
                this.idPos = id;
                this.name = name;
                this.salary = salary;
            }
            // Переопределение метода ToString()
            override public string ToString()
            {
                // формирование строки с помощью метода Format() класса string
                string st = string.Format("{0}{1}{2}",
                    idPos.ToString().PadRight(4),
                    name.PadRight(10),
                    salary.ToString().PadRight(4));
                return st;
            }
            public string InfoString()
            {
                string indName = "";
                for (int i = 0; i < ListsDB.indexes.Count; i++)
                {
                    if (ListsDB.indexes[i].id_Index == Salary) indName = ListsDB.indexes[i].index_Name;
                }
                string inf = $"\n:::::::::::::*************************::::::::::::::::\n$---Должность---$\nНомер: {ID}" +
                             $"\nНазвание: {Name}" +
                             $"\nНазвание индекса: {indName}" +
                             $"\n:::::::::::::*************************::::::::::::::::";

                return inf;
            }
        }
        public class Indexes
        {
            public int id_Index { get; set; }
            public string index_Name { get; set; }
            //Salary
            public double index_count { get; set; }
            public Indexes(int id, string name, double count)
            {
                this.id_Index = id;
                this.index_Name = name;
                this.index_count = count;
            }
            override public string ToString()
            {
                // формирование строки с помощью метода Format() класса string
                string st = string.Format("{0}{1} {2}",
                    id_Index.ToString().PadRight(4),
                    index_Name.PadRight(10),
                    index_count.ToString().PadRight(4));
                return st;
            }
            public string InfoString()
            {
                string inf = $"\n:::::::::::::*************************::::::::::::::::\n$---Индекс---$\nНомер: {id_Index}" +
                             $"\nНазвание: {index_Name}" +
                             $"\nСумма индекса (бел. руб.): {index_count}" +
                             $"\n:::::::::::::*************************::::::::::::::::";

                return inf;
            }
        }
        #endregion
        public class ListsDB
        {
            //Application consisting
            public static List<Transport> transports = new List<Transport> { };
            public static List<Customer> customers = new List<Customer> { };
            public static List<UserDB> users = new List<UserDB> { };
            public static List<Order> orders = new List<Order> { };
            public static List<Agent> agents = new List<Agent> { };
            public static List<Positions> positions = new List<Positions> { };
            public static List<Indexes> indexes = new List<Indexes> { };

        }
    }
}
