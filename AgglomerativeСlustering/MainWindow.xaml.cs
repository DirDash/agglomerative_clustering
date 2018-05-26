using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AgglomerativeСlustering.Clustering;
using AgglomerativeСlustering.Clustering.Clusterizators;
using AgglomerativeСlustering.Clustering.DistanceCalculators;
using System.IO;
using Microsoft.Win32;

namespace AgglomerativeСlustering
{
    public partial class MainWindow : Window
    {
        private List<ResearchObject> _objects;
        private List<Cluster> _clusters;

        private string _fileName = "файл не выбран";
        private int _objectAmount;

        private IClusterizator _clusterizator;
        private IDistanceCalculator _distanceCalculator;
        private int _n1 = 50;
        private int _n2 = 20;

        private int _clusterAmount = 1;
        private int _elipseRadius = 4;

        private string _firstFeature = "Первое свойство";
        private double _firstFeatureMin = double.MaxValue;
        private double _firstFeatureMax = double.MinValue;
        private string _secondFeature = "Второе свойство";
        private double _secondFeatureMin = double.MaxValue;
        private double _secondFeatureMax = double.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            SetDefaultValues();
            DrawCoordinateLine();
        }

        #region Interface methods

        private void SetDefaultValues()
        {
            InitializeClusterizator();

            InitializeClusterDistanceCalculator();

            FirstFeatureMinLbl.Visibility = Visibility.Hidden;
            FirstFeatureMaxLbl.Visibility = Visibility.Hidden;
            SecondFeatureMinLbl.Visibility = Visibility.Hidden;
            SecondFeatureMaxLbl.Visibility = Visibility.Hidden;
        }

        private void InitializeClusterizator()
        {
            ClusterizatorCb.Items.Clear();
            ClusterizatorCb.Items.Add(new FastLanceWilliamsAlgorithmClusterizator(_n1, _n2));
            ClusterizatorCb.SelectedIndex = 0;
        }

        private void InitializeClusterDistanceCalculator()
        {
            DistanceCalculatorCb.Items.Add(new ClosestNeighborDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new FarestNeighborDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new AverageGroupDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new CenterDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new WardDistanceCalculator());
            DistanceCalculatorCb.SelectedIndex = 4;
        }

        private void DrawCoordinateLine()
        {
            Line horizontalLine = new Line();
            horizontalLine.X1 = 0;
            horizontalLine.Y1 = VisualizationCanvas.Height / 2;
            horizontalLine.X2 = VisualizationCanvas.Width;
            horizontalLine.Y2 = VisualizationCanvas.Height / 2;
            horizontalLine.Stroke = Brushes.Black;
            horizontalLine.StrokeThickness = 0.3;
            Line verticalLine = new Line();
            verticalLine.X1 = VisualizationCanvas.Width / 2;
            verticalLine.Y1 = 0;
            verticalLine.X2 = VisualizationCanvas.Width / 2;
            verticalLine.Y2 = VisualizationCanvas.Height;
            verticalLine.Stroke = Brushes.Black;
            verticalLine.StrokeThickness = 0.3;
            VisualizationCanvas.Children.Add(horizontalLine);
            VisualizationCanvas.Children.Add(verticalLine);
        }

        private void Refresh()
        {
            FilenameLbl.Content = _fileName;
            ObjectsAmountLbl.Content = _objectAmount;
            if (_clusters != null)
                CurrentClustersAmountLbl.Content = _clusters.Count;
            else
                CurrentClustersAmountLbl.Content = 0;
            N1Tbx.Text = _n1.ToString();
            N2Tbx.Text = _n2.ToString();
            ClusterAmountTbx.Text = _clusterAmount.ToString();
            RadiusTbx.Text = _elipseRadius.ToString();
            FirstFeatureLbl.Content = _firstFeature;
            FirstFeatureMinLbl.Content = _firstFeatureMin;
            FirstFeatureMaxLbl.Content = _firstFeatureMax;
            SecondFeatureLbl.Content = _secondFeature;
            SecondFeatureMinLbl.Content = _secondFeatureMin;
            SecondFeatureMaxLbl.Content = _secondFeatureMax;
        }

        private double CalculateCoordinate(double currentMin, double currentMax, double realMin, double realMax, double current)
        {
            double positionCoefficient = Math.Abs(current - currentMin) / (currentMax - currentMin);
            return positionCoefficient * (realMax - realMin) + realMin;
        }

        private void VisualizeClusters(List<Cluster> clusters)
        {
            VisualizationCanvas.Children.Clear();
            var brushConverter = new BrushConverter();
            DrawCoordinateLine();
            foreach (var cluster in clusters)
            {
                VisualizeObjects(cluster.Objects, new SolidColorBrush(Color.FromRgb(cluster.Color.R, cluster.Color.G, cluster.Color.B)));
            }
        }

        private void VisualizeObjects(List<ResearchObject> objects, SolidColorBrush colorBrush)
        {
            foreach (var obj in objects)
            {
                double x = CalculateCoordinate(
                    double.Parse(FirstFeatureMinLbl.Content.ToString()),
                    double.Parse(FirstFeatureMaxLbl.Content.ToString()),
                    _elipseRadius + 2,
                    VisualizationCanvas.Width - _elipseRadius - 2,
                    obj.Features[0]);
                double y = CalculateCoordinate(
                    double.Parse(SecondFeatureMinLbl.Content.ToString()),
                    double.Parse(SecondFeatureMaxLbl.Content.ToString()),
                    _elipseRadius + 2,
                    VisualizationCanvas.Height - _elipseRadius - 2,
                    obj.Features[1]);
                Point point = new Point(x, VisualizationCanvas.Height - y);
                Ellipse elipse = new Ellipse();

                elipse.Width = 2 * _elipseRadius;
                elipse.Height = 2 * _elipseRadius;

                elipse.StrokeThickness = 1;
                elipse.Stroke = Brushes.DarkGray;
                elipse.Margin = new Thickness(point.X - _elipseRadius, point.Y - _elipseRadius, 0, 0);

                elipse.Fill = colorBrush;

                VisualizationCanvas.Children.Add(elipse);
            }
        }

        #endregion

        #region Business logic methods

        private void GetObjectsFromFile(string path)
        {
            _firstFeatureMin = double.MaxValue;
            _firstFeatureMax = double.MinValue;
            _secondFeatureMin = double.MaxValue;
            _secondFeatureMax = double.MinValue;
            var objects = new List<ResearchObject>();
            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string line = streamReader.ReadLine();
                    var headers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _firstFeature = headers[0];
                    _secondFeature = headers[1];
                    int lastId = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var splitedLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        double firstFeature = double.Parse(splitedLine[0]);
                        double secondFeature = double.Parse(splitedLine[1]);

                        if (firstFeature < _firstFeatureMin)
                            _firstFeatureMin = firstFeature;
                        if (firstFeature > _firstFeatureMax)
                            _firstFeatureMax = firstFeature;
                        if (secondFeature < _secondFeatureMin)
                            _secondFeatureMin = secondFeature;
                        if (secondFeature > _secondFeatureMax)
                            _secondFeatureMax = secondFeature;

                        string mark = "";
                        if (headers.Length == 3 && splitedLine.Length == 3)
                        {
                            mark = splitedLine[2];
                        }
                        else
                        {
                            mark = lastId.ToString();
                            lastId++;
                        }
                        var obj = new ResearchObject(mark, new List<double>() { firstFeature, secondFeature });
                        objects.Add(obj);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            _objectAmount = objects.Count;
            _objects = objects;
        }

        private void Clusterize()
        {
            _clusterizator.Clusterize(_objects);
        }

        private void GetClusters()
        {
            _clusters = _clusterizator.GetClusters(_clusterAmount);
        }

        private void SaveClustersToFile(string path)
        {
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                foreach (var cluster in _clusters)
                {
                    string clusterInfo = String.Format("ClusterId: {0} ({1} object", cluster.Id, cluster.Objects.Count);
                    if (cluster.Objects.Count > 1)
                        clusterInfo += "s";
                    clusterInfo += "):";
                    streamWriter.WriteLine(clusterInfo);
                    foreach (var obj in cluster.Objects)
                    {
                        string objInfo = String.Format("    {0} ({1}: {2}; {3}: {4})", obj.Mark, _firstFeature, obj.Features[0], _secondFeature, obj.Features[1]);
                        streamWriter.WriteLine(objInfo);
                    }
                    streamWriter.WriteLine("");
                }
            }
            MessageBox.Show("Сохрание успешно завершено");
        }

        #endregion

        #region Events

        #region OnClick Events

        private void GetDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".txt";
            fileDialog.Filter = "Text documents (.txt)|*.txt";
            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = fileDialog.FileName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                _fileName = fileName.Last();

                GetObjectsFromFile(fileDialog.FileName);

                FirstFeatureMinLbl.Visibility = Visibility.Visible;
                FirstFeatureMaxLbl.Visibility = Visibility.Visible;
                SecondFeatureMinLbl.Visibility = Visibility.Visible;
                SecondFeatureMaxLbl.Visibility = Visibility.Visible;

                Refresh();
                VisualizationCanvas.Children.Clear();
                VisualizeObjects(_objects, Brushes.DarkSlateGray);

                InitializeClusterizator();
                ClusterizeBtn.IsEnabled = true;
            }
        }

        private void ClusterizeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clusterize();

                GetClusters();

                Refresh();

                VisualizeClusters(_clusters);
                VisualizeBtn.IsEnabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void VisuzlizeBtn_Click(object sender, RoutedEventArgs e)
        {
            GetClusters();

            Refresh();

            VisualizeClusters(_clusters);
        }

        private void SaveDataBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new SaveFileDialog();
                fileDialog.DefaultExt = ".txt";
                fileDialog.Filter = "Text documents (.txt)|*.txt";
                fileDialog.FileName = "clusters_" + _clusters.Count + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
                var result = fileDialog.ShowDialog();
                if (result == true)
                {
                    SaveClustersToFile(fileDialog.FileName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region OnChange Events

        private void ClusterizatorCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClusterizatorCb.SelectedItem != null)
            {
                _clusterizator = (IClusterizator)ClusterizatorCb.SelectedItem;
                if (_distanceCalculator != null)
                    _clusterizator.ClusterDistanceCalculator = _distanceCalculator;
            }
        }

        private void DistanceCalculatorCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _distanceCalculator = (IDistanceCalculator)DistanceCalculatorCb.SelectedItem;
            _clusterizator.ClusterDistanceCalculator = _distanceCalculator;
            if (VisualizeBtn != null)
                VisualizeBtn.IsEnabled = false;
        }

        private void N1Tbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n1 = 2;
            if (int.TryParse(N1Tbx.Text, out n1))
            {
                if (n1 < 2)
                    n1 = 2;
            }
            else
            {
                _n1 = 2;
                N1Tbx.Text = _n1.ToString();
            }
            InitializeClusterizator();
            if (VisualizeBtn != null)
                VisualizeBtn.IsEnabled = false;
        }

        private void N2Tbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n2 = 2;
            if (int.TryParse(N2Tbx.Text, out n2))
            {
                if (n2 < 2)
                    n2 = 2;
            }
            else
            {
                _n2 = 2;
                N2Tbx.Text = _n2.ToString();
            }
            InitializeClusterizator();
            if (VisualizeBtn != null)
                VisualizeBtn.IsEnabled = false;
        }

        private void ClusterAmountTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            int clusterAmount = 1;
            if (int.TryParse(ClusterAmountTbx.Text, out clusterAmount))
            {
                if (clusterAmount > _objectAmount)
                    clusterAmount = _objectAmount;
                if (clusterAmount < 1)
                    clusterAmount = 1;
                _clusterAmount = clusterAmount;
            }
            else
            {
                _clusterAmount = 1;
                ClusterAmountTbx.Text = _clusterAmount.ToString();
            }
        }

        private void RadiusTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            int radius = 1;
            if (int.TryParse(RadiusTbx.Text, out radius))
            {
                if (radius > 30)
                    radius = 30;
                if (radius < 1)
                    radius = 1;
                _elipseRadius = radius;
            }
            else
            {
                _elipseRadius = 1;
                RadiusTbx.Text = _elipseRadius.ToString();
            }
        }

        #endregion

        #endregion
    }
}