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
    /// Interaction logic for request_line_chart.xaml
    /// </summary>
    public partial class request_line_chart : UserControl
    {
        public request_line_chart()
        {
            InitializeComponent();
        }
        Dictionary<DateTime, chartData> data = new Dictionary<DateTime, chartData>();
        System.Threading.Thread thr;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0);

            t.Tick += (object sender1, EventArgs e1) => { load(); ((System.Windows.Threading.DispatcherTimer)sender1).Stop(); };
            t.Start();
        }
        private void load(bool clear = true)
        {
            data.Clear();
            thr = new System.Threading.Thread(() => getData());
            thr.Start();
            thr.Join();
            next(clear);
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
        private struct chartData
        {
            public long numOfRequest;
        }
        DateTime date1;
        DateTime date2;

        private void drawChart()
        {

            listView.Items.Clear();
            rect.Children.Clear();
            for (int i = 0; i < data.Keys.Count; i++)
            {
                if (data.Keys.ElementAt<DateTime>(i) < date1)
                {
                    data.Remove(data.Keys.ElementAt<DateTime>(i));
                    i--;
                }
                else if (data.Keys.ElementAt<DateTime>(i) > date2)
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

            Polyline linia = new Polyline();
            var br = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            linia.Stroke = br;

            double width = rect.ActualWidth - 50;
            double height = rect.ActualHeight - 10 - 20;
            const double startX = 40;
            const double startY = 10;
            double max = data.Values.First<chartData>().numOfRequest;
            double min = data.Values.First<chartData>().numOfRequest;
            double mov = width / (double)(data.Count - 1);
            if (width < 10) { return; }

            foreach (var el in data)
            {
                if (max < el.Value.numOfRequest) { max = el.Value.numOfRequest; }
                if (min > el.Value.numOfRequest) { min = el.Value.numOfRequest; }
            }

            rect.Children.Add(new Line() { X1 = startX, X2 = startX, Y1 = 10, Y2 = 10 + height, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });
            rect.Children.Add(new Line() { X1 = startX + width, X2 = startX + width, Y1 = 10, Y2 = 10 + height, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });

            int i1 = 0;
            for (double my = 10; my <= height + 10; my += height / 15)
            {
                rect.Children.Add(new Line() { X1 = startX, X2 = startX + width, Y1 = my, Y2 = my, Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), StrokeThickness = 1 });
                var tb = new TextBlock();
                tb.Text = ((int)(max - ((double)i1 * (max / 15)))).ToString();
                tb.VerticalAlignment = VerticalAlignment.Top;
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.Margin = new Thickness(0, my - 10, 0, 0);
                rect.Children.Add(tb);
                i1++;
            }
            i1 = 0;
            int intenum = 0;
            double tMargin = (data.Count - 1) / (width / 77);
            for (double mx = 40; mx <= width + 40; mx += width / (data.Keys.Count - 1))
            {
                var tb = new TextBlock();
                //tb.Text = ((int)(max - ((double)i1 * (max / 15)))).ToString();
                if (i1 == 0 && intenum < data.Keys.Count)
                {
                    tb.Text = data.Keys.ElementAt<DateTime>(intenum).ToShortDateString();
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    tb.Width = 100;
                    tb.TextAlignment = TextAlignment.Center;
                    tb.Margin = new Thickness(mx - (tb.Width / 2), 0, 0, 0);
                    rect.Children.Add(tb);
                }

                i1++;
                if (i1 > tMargin) { i1 = 0; }
                intenum++;

            }

            double x = startX;
            foreach (var el in data)
            {
                linia.Points.Add(new Point(x, (height + startY) - (height * (el.Value.numOfRequest / max))));
                x += mov;
                listView.Items.Add(new dd { date = el.Key.ToShortDateString(), request = el.Value.numOfRequest.ToString() });
            }
            //tworzenie ładnego podtła
            var back = new Polygon();
            back.Points = linia.Points.Clone();
            back.Points.Add(new Point(width + 40, height + startY));
            back.Points.Add(new Point(40, height + startY));
            back.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            rect.Children.Add(back);

            rect.Children.Add(linia);
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

        private void datepicker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (datepicker1.SelectedDate != null && datepicker2.SelectedDate != null)
                {
                    if (date1 != (DateTime)datepicker1.SelectedDate || date2 != (DateTime)datepicker2.SelectedDate)
                    {
                        if (datepicker1.SelectedDate > datepicker2.SelectedDate)
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
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                if (line.StartsWith("GetFile\t"))
                {
                    string[] par = line.Split('\t');
                    DateTime d;
                    if (DateTime.TryParse(par[1], out d))
                    {
                        d = d.Date;
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
            }
            if (date1.Ticks == 0)
            {
                date1 = data.Keys.Min<DateTime>();
            }
            if (date2.Ticks == 0)
            {
                date2 = data.Keys.Max<DateTime>();
            }

        }

        private void rect_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (data.Count > 0)
            {
                rect.Children.Clear();
                drawChart();
            }

        }
    }

}
