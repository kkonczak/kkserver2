using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KKServerGUI
{
    /// <summary>
    /// Interaction logic for request_time_chart.xaml
    /// </summary>
    public partial class request_time_chart : UserControl
    {
        private struct chartData
        {
            public long numOfRequest;
        }

        public request_time_chart()
        {
            InitializeComponent();
        }

        Dictionary<DateTime, chartData> data = new Dictionary<DateTime, chartData>();
        System.Threading.Thread thr;
        private bool dataIsReady = false;
        private double loadPercent = 0;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            load(true);
        }
        private void load(bool clear = true)
        {
            dataIsReady = false;
            data.Clear();
            thr = new System.Threading.Thread(() => getData());
            thr.Start();
            var t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(3);

            t.Tick += (object sender1, EventArgs e1) => {
                textBlockLoadPercent.Text = ((int)(loadPercent * 100)).ToString() + "%"; if (dataIsReady) { next(clear); ((System.Windows.Threading.DispatcherTimer)sender1).Stop(); textBlockLoadPercent.Visibility = Visibility.Collapsed; }
            };
            t.Start();
            textBlockLoadPercent.Visibility = Visibility.Visible;
            textBlockLoadPercent.Text = "0%";
        }

        private void next(bool clear)
        {
            if (clear)
            {
                datepicker1.SelectedDate = date1;
                datepicker2.SelectedDate = date2;
            }

            drawChart();
        }



        DateTime date1;
        DateTime date2;

        private void datepicker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (datepicker1.SelectedDate != null && datepicker2.SelectedDate != null)
                {
                    if (date1 != (DateTime)datepicker1.SelectedDate || date2 != (DateTime)datepicker2.SelectedDate)
                    {
                        if(datepicker1.SelectedDate> datepicker2.SelectedDate)
                        {
                            DateTime? tmp = datepicker1.SelectedDate;
                            datepicker1.SelectedDate = datepicker2.SelectedDate;
                            datepicker2.SelectedDate = tmp;
                        }
                        date1 = (DateTime)datepicker1.SelectedDate;
                        date2 = (DateTime)datepicker2.SelectedDate;
                        load(false);
                    }

                }


            }
            catch
            {

            }

        }

        private void getData()
        {
            loadPercent = 0;
            long bytesLoad = 0;
            long bytesTotal = 0;
            var sett = new System.Xml.XmlDocument();
            sett.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
            string path = "";
            var s = sett.SelectNodes("settings/server/logs/log");
            for (int i = 0; i < s.Count; i++)
            {
                if (s[i].Attributes["type"].InnerText == "main")
                {
                    path = s[i].Attributes["path"].InnerText;
                }
            }
            path = path.Replace("%STARTPATH%", AppDomain.CurrentDomain.BaseDirectory);
            if (!(new System.IO.FileInfo(path).Exists))
            {
                MessageBox.Show("No logged data.");
                return;
            }
            var file = new System.IO.StreamReader(new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite));
            bytesTotal = file.BaseStream.Length;
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                if (line.StartsWith("GetFile\t"))
                {
                    string[] par = line.Split('\t');
                    bytesLoad += line.Length;
                    DateTime d;
                    if (DateTime.TryParse(par[1], out d))
                    {
                        d = new DateTime( d.Year,d.Month,d.Day,d.Hour ,0,0 );
                        if (data.ContainsKey(d))
                        {
                            data[d] = new chartData() { numOfRequest = data[d].numOfRequest + 1 };
                        }
                        else
                        {
                            data.Add(d, new chartData() { numOfRequest = 0 });
                        }

                    }
                }
                loadPercent = (double)bytesLoad / (double)bytesTotal;
            }
            if (date1.Ticks == 0)
            {
                date1 = data.Keys.Min<DateTime>();
            }
            if (date2.Ticks == 0)
            {
                date2 = data.Keys.Max<DateTime>();
            }
            loadPercent = 1;
            dataIsReady = true;
        }

        private void rect_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (data.Count > 0)
            {
                rect.Children.Clear();
                drawChart();
            }

        }

        private void drawChart()
        {
           
            listView.Items.Clear();
            rect.Children.Clear();
            for (int i = 0; i < data.Keys.Count; i++)
            {
                if (data.Keys.ElementAt<DateTime>(i).Date  < date1.Date )
                {
                    data.Remove(data.Keys.ElementAt<DateTime>(i));
                    i--;
                }
                else if (data.Keys.ElementAt<DateTime>(i).Date > date2.Date )
                {
                    data.Remove(data.Keys.ElementAt<DateTime>(i));
                    i--;
                }
            }
            if (data.Count == 0)
            {
                var tb = new TextBlock();
                tb.Text = "No data in selected range...";
                rect.Children.Add(tb);
                return;
            }



            double width = rect.ActualWidth - 50;
            double height = rect.ActualHeight - 10 - 20;
            double x = 40;
            double y = 10;
            double max = data.Values.First<chartData>().numOfRequest;
            double min = data.Values.First<chartData>().numOfRequest;
            double mov = width / (double)(data.Count - 1);
            if (width < 10) { return; }

            foreach (var el in data)
            {
                if (max < el.Value.numOfRequest) { max = el.Value.numOfRequest; }
                if (min > el.Value.numOfRequest) { min = el.Value.numOfRequest; }
            }

            rect.Children.Add(new Line() { X1 = x, X2 = x, Y1 = y, Y2 = y + height, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });
            rect.Children.Add(new Line() { X1 = x + width, X2 = x + width, Y1 = y, Y2 = y + height, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });

           

            double movPerDay = width / ((date2.Date - date1.Date).Days + 1);
            double movPerHour = height / 24;

            //zrobić wyświetlanie godzin

            for (int i = 0; i < 24; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString();
                tb.VerticalAlignment = VerticalAlignment.Top;
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.Width = 35;
                tb.TextAlignment = TextAlignment.Right ;
                tb.Margin = new Thickness(0, i*movPerHour+10, 0, 0);
                rect.Children.Add(tb);
                rect.Children.Add(new Line() { X1 = 40, X2 = 40+width , Y1 = i*movPerHour+10, Y2=i * movPerHour+10, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });
            }
            for (int i = 0; i < (date2.Date -date1.Date ).Days ; i++)
            {
                rect.Children.Add(new Line() { X1 = 40+movPerDay+i* movPerDay, X2 = 40 + movPerDay + i * movPerDay, Y1 =  10, Y2 = height + 10, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });

            }
           
           
            //dorobić czas

           
            foreach (var el in data)
            {
                // linia.Points.Add(new Point(x, (height + y) - (height * (el.Value.numOfRequest / max))));
                var recta = new Rectangle();
                recta.Fill = new SolidColorBrush(Color.FromArgb((byte)(((double)el.Value.numOfRequest / (double)max) * 255),255 , 0, 0));
                recta.VerticalAlignment = VerticalAlignment.Top;
                recta.HorizontalAlignment = HorizontalAlignment.Left;
                recta.Margin = new Thickness(movPerDay * ( el.Key - date1).Days+40, movPerHour * el.Key.Hour+10  , 0, 0);
                recta.Width = movPerDay;
                recta.Height = movPerHour;
                recta.RadiusX = 2;
                recta.RadiusY = 2;
                x += mov;
                rect.Children.Add(recta);
                listView.Items.Add(new dd { date = el.Key.ToShortDateString()+" "+el.Key.Hour +"h", request = el.Value.numOfRequest.ToString() });
            }
            

            //rect.Children.Add(linia);
            //linia.Points.Add();
        }

        public class dd
        {
            string _d;
            string _r;
            public string date
            {
                get
                {
                    return _d;
                }
                set
                {
                    _d = value;
                }
            }
            public string request
            {
                get
                {
                    return _r;
                }
                set
                {
                    _r = value;
                }
            }

        }

    }
}
