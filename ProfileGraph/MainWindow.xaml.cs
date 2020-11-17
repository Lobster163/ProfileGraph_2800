
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Timers;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Markup;

namespace ProfileGraph
{
    /// <summary>
    /// refresh object
    /// </summary>
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };
        public static void Refresh(this System.Windows.UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
    

    /// <summary>lo
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool cicle = true;

        private Thread myThread, myThreadUhod;
        public bool trigerON = false;
        public float maxAx = 0;
        public float minAx = 0;
        readonly IniFile INI = new IniFile("config.ini");
        private Grafiki_profile_clin viewModel_GrafikSum = new Grafiki_profile_clin(); // класс для отображения графика ухода полосы 1 /2 /3 /4 /5 клеть
        private Grafik_uhod viewModel_mem = new Grafik_uhod(); // класс для отображения графика ухода полосы 1 /2 /3 /4 /5 клеть
        

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct structMy  //структура данных в телеграмме - должна совпадать со структорой в wshcgag.c на 2 уровне
        {
            public int countPos;
            public float nominal;
            public float fPrfMillPctCenter;
            public float fPrfTrimPctWedge;
            public fixed int position[2];
            public fixed float values[64];            
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct StructDataUhod  //структура данных в телеграмме - должна совпадать со структорой в wshcgag.c на 2 уровне
        {
            public fixed float fValues[64];
        };

        //-----------------------------------------------------------------------------------------------------------------------
        //переменные для получение графиков с профилемера
        private byte[] data = new byte[10];
        private IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 43500); //принимаем сообщения с любого айпи на порт 43500
        private UdpClient newsock;
        IPEndPoint send = new IPEndPoint(IPAddress.Any, 0); //ловим данный отправителя
        //-----------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------------------------
        //переменные для ухода
        private byte[] dataUhod = new byte[10];
        private IPEndPoint ipepUhod = new IPEndPoint(IPAddress.Any, 43600); //принимаем сообщения с любого айпи на порт 43600
        private UdpClient newsockUhod;
        IPEndPoint sendUhod = new IPEndPoint(IPAddress.Any, 0); //ловим данный отправителя
        //-----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// инициализация приложения
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            var monitor = Screen.AllScreens;    //получаем список мониторов
            mainForm.Left = -monitor[1].Bounds.Width;   //задаем положение окна
            mainForm.Top = -monitor[1].Bounds.Height;   //задаем положение окна
            mainForm.WindowStartupLocation = WindowStartupLocation.Manual; //задаем положение в ручную
            mainForm.WindowState = WindowState.Normal;  //состояние окна
            mainForm.Show();    //показать окно
            mainForm.WindowState = WindowState.Maximized;   //разверуть на максимум окно

            newsock = new UdpClient(ipep);
            myThread = new Thread(new ThreadStart(udpServer));  //создание нового потока в процессоре
            myThread.Start();   //запуск нового потока

            newsockUhod = new UdpClient(ipepUhod);
            myThreadUhod = new Thread(new ThreadStart(udpServerUhod));  //создание нового потока в процессоре
            myThreadUhod.Start();   //запуск нового потока

            Timer timer = new Timer();  //таймер проверки нового рулона
            timer.Elapsed += OnTimedEvent;  //обработчик события таймера
            timer.Interval = 1000 * 60; //60 секунд.
            timer.Start();  //запуск таймера            
        }

        /// <summary>
        /// Запись в еррор лог
        /// </summary>
        /// <param name="MSG"></param>
        private void WriteErrorLog(string MSG)
        {
            StreamWriter Log;
            Log = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "error.log");
            Log.WriteLine("------ "+DateTime.Now+" ---------------------------------------------------------------------------------");
            Log.WriteLine(MSG);            
            Log.Close();
        }

        /// <summary>
        /// Запист в дебаг файл
        /// </summary>
        /// <param name="MSG"></param>
        private void WriteDebugLog(string MSG)
        {
        }

        /// <summary>
        /// принимаем данные по UDP пропиль полосы
        /// </summary>
        unsafe private void udpServer()
        {   
            while (cicle)
            {
                try
                {
                    data = newsock.Receive(ref send);   //принимаем данные из сокета по UDP
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {                   
                        structMy myStruct = new structMy();
                        myStruct = ByteArrayToNewStuff(data);
                        grafBuilder(myStruct);
                    }));
                }
                catch(Exception ex)
                {
                    WriteErrorLog(ex.ToString());
                }
            }
        }

        /// <summary>
        /// принимаем данные по udp уход полосы 
        /// </summary>
        unsafe private void udpServerUhod()
        {
            while (cicle)   //включаем цикл получение данных из сети
            {
                try //если будет ошибка
                {
                    dataUhod = newsockUhod.Receive(ref sendUhod);   //принимаем данные из сокета по UDP
                    this.Dispatcher.BeginInvoke(new Action(() =>    //предоставляем доступ из другому потока в основной
                    {
                        StructDataUhod myStruct = new StructDataUhod();
                        myStruct = ByteArrayToNewStuff_u(dataUhod);
                        grafBuilder_UHOD(myStruct);
                    }));
                }
                catch (Exception ex) // обработки ошибки
                {
                    WriteErrorLog("udpServerUhod: " + ex.ToString());
                }
            }
        }
        
        /// <summary>
        /// постройка графиков для ухода полосы
        /// </summary>
        /// <param name="dataRECV"></param>
        unsafe void grafBuilder_UHOD(StructDataUhod dataRECV)
        {
            var viewModel = new Grafik_uhod();  
            DataContext = viewModel;
            DataContext = viewModel_mem;
            if (dataRECV.fValues[5] > 100.0f && dataRECV.fValues[45] > 1500.0 && !trigerON )    //тригер на включение записи
            {
                viewModel_mem = new Grafik_uhod();  
                trigerON = true;
            }
            if (dataRECV.fValues[5] < 100.0f && dataRECV.fValues[9] < 100.0f &&
                dataRECV.fValues[45] < 1500.0 && dataRECV.fValues[46] < 1500.0 && trigerON) //выключение тригера записи
            {
                trigerON = false;
            }
                        

            if (trigerON)  //разрешаем рисовать графики
            {
                if (viewModel_mem.Points_1.Count > 800)     //если кол-во точек больше 800, то расширяем график на +1 точку
                    viewModel_mem.AxisX_max = viewModel_mem.AxisX_max + 1;

                //масштаб оси по Y клети 1
                if (dataRECV.fValues[0] > viewModel_mem.leftMax_1)
                    viewModel_mem.leftMax_1 = Convert.ToInt32(viewModel_mem.leftMax_1 + 50);
                if (dataRECV.fValues[0] < viewModel_mem.leftMin_1)
                    viewModel_mem.leftMin_1 = Convert.ToInt32(viewModel_mem.leftMin_1 - 50);

                //масштаб оси по Y клети 2
                if (dataRECV.fValues[1] > viewModel_mem.leftMax_2)
                    viewModel_mem.leftMax_2 = Convert.ToInt32(viewModel_mem.leftMax_2 + 50);
                if (dataRECV.fValues[1] < viewModel_mem.leftMin_2)
                    viewModel_mem.leftMin_2 = Convert.ToInt32(viewModel_mem.leftMin_2 - 50);

                //масштаб оси по Y клети 3
                if (dataRECV.fValues[2] > viewModel_mem.leftMax_3)
                    viewModel_mem.leftMax_3 = Convert.ToInt32(viewModel_mem.leftMax_3 + 50);
                if (dataRECV.fValues[2] < viewModel_mem.leftMin_3)
                    viewModel_mem.leftMin_3 = Convert.ToInt32(viewModel_mem.leftMin_3 - 50);

                maxAx = 0;
                minAx = 0;
                //масштаб оси по Y клеть 4 и 5
                for (int i = 3; i < 4; i++)
                {
                    if (dataRECV.fValues[i] > dataRECV.fValues[i + 1])
                        maxAx = dataRECV.fValues[i];
                    else
                        maxAx = dataRECV.fValues[i + 1];

                    if (dataRECV.fValues[i] < dataRECV.fValues[i + 1])
                        minAx = dataRECV.fValues[i];
                    else
                        minAx = dataRECV.fValues[i + 1];
                }
                if (maxAx > viewModel_mem.leftMax45)
                    viewModel_mem.leftMax45 = Convert.ToInt32(maxAx + 50);
                if (minAx < viewModel_mem.leftMin45)
                    viewModel_mem.leftMin45 = Convert.ToInt32(minAx - 50);

                //добавление точек в графики
                viewModel_mem.Points_1.Add(new DataPoint(viewModel_mem.Points_1.Count + 1, dataRECV.fValues[0]));
                viewModel_mem.Points_2.Add(new DataPoint(viewModel_mem.Points_2.Count + 1, dataRECV.fValues[1]));
                viewModel_mem.Points_3.Add(new DataPoint(viewModel_mem.Points_3.Count + 1, dataRECV.fValues[2]));
                viewModel_mem.Points_4.Add(new DataPoint(viewModel_mem.Points_3.Count + 1, dataRECV.fValues[3]));
                viewModel_mem.Points_5.Add(new DataPoint(viewModel_mem.Points_3.Count + 1, dataRECV.fValues[4]));
                viewModel = viewModel_mem;
            }
        }

        private int numberScan = -1;
        private int numberScanPred = -1;
        private int countScanActual = 0;
        private int[] positionMain = new int[2]; 
        private double[,] scansData;

        /// <summary>
        /// таймер для сброса счетчика профилей 
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (numberScanPred != numberScan)   //проверка на новый рулон
                    numberScanPred = numberScan;
                else
                {
                    numberScan = -1;
                    scansData = new double[25, 50];
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex.ToString());
            }
        }
          
        /// <summary>
        /// постройка среднего графика
        /// </summary>
        unsafe private void grafBuilderSum(structMy data)
        {
            var model = new PlotModel { Title = "Усредненный график" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = -1200, Maximum = 1200 });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = chartOxy.Model.Axes[1].Minimum,
                Maximum = chartOxy.Model.Axes[1].Maximum,
            });

            double[] grafSum = new double[countScanActual + 1];
            double profilePrcSum = 0.0;
            double klinPrcSum = 0.0;
            for (int i = 0; i < grafSum.Length; i++)    //обнуляем массив
                grafSum[i] = 0.0;

            for(int pos = 0; pos <= countScanActual; pos++)   //усредняем графики
            {
                for (int numscan = 1; numscan <= numberScan; numscan++)
                    grafSum[pos] = grafSum[pos] + scansData[numscan, pos];

                grafSum[pos] = grafSum[pos] / numberScan;                
            }

            bool lastScan = false;
            for (int numscan = 1; numscan <= numberScan; numscan++) //усредняем показания клина и профиля
            {
                if (scansData[numscan, countScanActual + 1] == -99)
                {
                    profilePrcSum = profilePrcSum + 0;
                    klinPrcSum = klinPrcSum + 0;
                    if (numscan == numberScan)
                        lastScan = true;
                }
                else
                {
                    profilePrcSum = profilePrcSum + scansData[numscan, countScanActual + 1];
                    klinPrcSum = klinPrcSum + scansData[numscan, countScanActual + 2];
                }
            }

            if (!lastScan)
            {
                profilePrcSum = profilePrcSum / numberScan;
                klinPrcSum = klinPrcSum / numberScan;
            }
            else
            {
                profilePrcSum = profilePrcSum / (numberScan - 1);
                klinPrcSum = klinPrcSum / (numberScan - 1);
            }

            LabelfPrfMillPctCenterSum.Content = String.Format("{0:0.000}", profilePrcSum);
            LabelfPrfTrimPctWedgeSum.Content = String.Format("{0:0.000}", klinPrcSum);

            LineSeries lineSeries = new LineSeries();
            for (int i = countScanActual-1; i >= 0; i--)  //отрисовка графика
                lineSeries.Points.Add(new DataPoint(data.position[1] + 50 * i, grafSum[i]));

            var ttt = INI.ReadINI("grafik_sum", "thickness");
            lineSeries.StrokeThickness = Double.Parse(INI.ReadINI("grafik_sum", "thickness"));    //толщина линии
            lineSeries.Color = OxyColor.FromRgb(
                byte.Parse(INI.ReadINI("grafik_sum", "colorR")),
                byte.Parse(INI.ReadINI("grafik_sum", "colorG")),
                byte.Parse(INI.ReadINI("grafik_sum", "colorB"))
                );
            model.Series.Add(lineSeries);
            chartOxySum.Model = model;
        }

        /// <summary>
        /// Постройка актуального графика
        /// </summary>
        /// <param name="dataBytes"></param>
        unsafe private void grafBuilder(structMy data)
        {
            try
            {
                var viewModel = new Grafiki_profile_clin();
                DataContext = viewModel;
                DataContext = viewModel_GrafikSum;


                var model = new PlotModel { Title = "Последний измеренный профиль" };   //заголовок графика
                mainForm.Refresh();

                if (numberScan >= 20) // не может быть больше 20 сканов
                    numberScan = -1;

                numberScan++;

                if (numberScan == 0) // для первого скана
                {
                    //обнуление
                    scansData = new double[25, data.countPos + 3];
                    countScanActual = data.countPos;
                    for (int sc = 0; sc < 25; sc++)
                        for (int point = 0; point < data.countPos + 3; point++)
                            scansData[sc, point] = data.nominal;
                }
                else if (numberScan == 1)
                {
                    countScanActual = data.countPos;    //установка кол-во сканов по 2м профилю
                    positionMain[0] = data.position[0];
                    positionMain[1] = data.position[1];
                }

                labelNumber.Content = "Номер профиля: " + (numberScan + 1).ToString();
                model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = -1200, Maximum = 1200 }); //границы по ширине 
                model.Axes.Add(new LinearAxis   //границы по высоте
                {
                    Position = AxisPosition.Left,
                    Minimum = data.nominal - (data.nominal * float.Parse(INI.ReadINI("main", "scaleDown"))/100.0),
                    Maximum = data.nominal + (data.nominal * float.Parse(INI.ReadINI("main", "scaleUp")) / 100.0)
                });

                LabelfPrfMillPctCenter.Content = String.Format("{0:0.000}", data.fPrfMillPctCenter);
                LabelfPrfTrimPctWedge.Content = String.Format("{0:0.000}", data.fPrfTrimPctWedge);
                //допуск для отбраковки профиля по допуску +-
                double minDopusk = data.nominal - (data.nominal * float.Parse(INI.ReadINI("main", "dopuskDown")) / 100.0); //4%   
                double maxDopusk = data.nominal + (data.nominal * float.Parse(INI.ReadINI("main", "dopuskUp")) / 100.0); //4%
                bool failProfile = false;   //ошибочный профиль

                LineSeries lineSeries = new LineSeries();
                for (int i = countScanActual - 1; i >= 0; i--) //создание актуального графика
                {
                    if (data.values[i] < maxDopusk && data.values[i] > minDopusk 
                        && !failProfile 
                        && data.countPos == countScanActual
                        && data.position[0] == positionMain[0]
                        && data.position[1] == positionMain[1]
                        )   //если в допуске и не ошибочный профиль, и кол-во точек равно кол-ву на 2 профиле
                    {
                        if (numberScan != 0)
                            scansData[numberScan, i] = data.values[i];
                    }
                    else
                        //дефектный
                        failProfile = true;

                    //рисуем график
                    lineSeries.Points.Add(new DataPoint(data.position[1] + 50 * i, data.values[i]));
                }
                if (failProfile) //обработка ошибочного профиля
                {
                    for (int i = countScanActual - 1; i >= 0; i--)
                    {
                        if (numberScan != 0)
                            scansData[numberScan, i] = scansData[numberScan - 1, i];    //запись в текущий скан предыдущего  профиля
                    }
                    scansData[numberScan, countScanActual + 1] = -99; 
                    scansData[numberScan, countScanActual + 2] = -99;
                    failProfile = false;
                }
                else
                {
                    scansData[numberScan, countScanActual + 1] = data.fPrfMillPctCenter;
                    scansData[numberScan, countScanActual + 2] = data.fPrfTrimPctWedge;
                }

                lineSeries.StrokeThickness = Double.Parse(INI.ReadINI("grafik_actual", "thickness"));    //толщина линии
                lineSeries.Color = OxyColor.FromRgb(
                    byte.Parse(INI.ReadINI("grafik_actual", "colorR")),
                    byte.Parse(INI.ReadINI("grafik_actual", "colorG")),
                    byte.Parse(INI.ReadINI("grafik_actual", "colorB"))
                    );
                model.Series.Add(lineSeries);   //добавляем график в модель
                chartOxy.Model = model; //выводим график
                chartOxy.Refresh();

                if (numberScan >=1)
                    grafBuilderSum(data);   //вызываем метод для отрисовки среднего графика
            }
            catch (Exception ex)    //обработка исключения
            {
                WriteErrorLog(ex.ToString());   //запись в лог ошибки
            }            
        }

        /// <summary>
        /// чтение данных из массива байт в структуру
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        structMy ByteArrayToNewStuff(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            structMy stuff;
            try
            {
                stuff = (structMy)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(structMy));
            }
            finally
            {
                handle.Free();
            }
            return stuff;
        }

        /// <summary>
        /// чтение данных из массива байт в структуру
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        StructDataUhod ByteArrayToNewStuff_u(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            StructDataUhod stuff;
            try
            {
                stuff = (StructDataUhod)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(StructDataUhod));
            }
            finally
            {
                handle.Free();
            }
            return stuff;
        }

        /// <summary>
        /// действие при выходе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            cicle = false;
            myThread.Abort();   //завершаем втрой поток
            myThreadUhod.Abort();   //завершаем втрой поток
        }

        /// <summary>
        /// действие при закрытии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cicle = false;
            System.Diagnostics.Process.GetCurrentProcess().Kill(); //принудительно убиваем все процессы и потоки, которые зависят от нашей программы(чистка  ОП)
        }

        private void print_test_BTN_Click(object sender, RoutedEventArgs e)
        {            
        }

        private void mainForm_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
