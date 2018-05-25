using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AgglomerativeСlustering.Clustering;
using AgglomerativeСlustering.Clustering.Algorithms;
using AgglomerativeСlustering.Clustering.Colors;
using AgglomerativeСlustering.Clustering.DistanceCalculators;
using System.IO;
using Microsoft.Win32;

namespace AgglomerativeСlustering
{
    public partial class MainWindow : Window
    {
        private ClusterSystem _clusterSystem;
        private Dendrogram _dendrogram;
        private List<Cluster> _clusters;

        private string _fileName = "файл не выбран";
        private int _objectAmount;
        private int _currentClusterNumber;
        private double _averageClusterDistance = 0;

        private FastLanceWilliamsAlgorithm _clusterizator;
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

            FirstInit();
        }
        
        private void FirstInit()
        {
            ClusterizatorCb.Items.Add(new FastLanceWilliamsAlgorithm());
            ClusterizatorCb.SelectedIndex = 0;

            DistanceCalculatorCb.Items.Add(new ClosestNeighborDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new FarestNeighborDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new AverageGroupDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new CenterDistanceCalculator());
            DistanceCalculatorCb.Items.Add(new WardDistanceCalculator());
            DistanceCalculatorCb.SelectedIndex = 4;
            FirstFeatureMinLbl.Visibility = Visibility.Hidden;
            FirstFeatureMaxLbl.Visibility = Visibility.Hidden;
            SecondFeatureMinLbl.Visibility = Visibility.Hidden;
            SecondFeatureMaxLbl.Visibility = Visibility.Hidden;
            DrawCoordinateLine();
        }

        private void Refresh()
        {
            FilenameLbl.Content = _fileName;
            ObjectsAmountLbl.Content = _objectAmount;
            CurrentClustersAmountLbl.Content = _currentClusterNumber;
            AvgClusterDistanceLbl.Content = String.Format("{0:0.00}", _averageClusterDistance);
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

        private void GetDataBtn_Click(object sender, RoutedEventArgs e)
        {
            _dendrogram = null;
            var fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".txt";
            fileDialog.Filter = "Text documents (.txt)|*.txt";
            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                _firstFeatureMin = double.MaxValue;
                _firstFeatureMax = double.MinValue;
                _secondFeatureMin = double.MaxValue;
                _secondFeatureMax = double.MinValue;
                var fileName = fileDialog.FileName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                _fileName = fileName.Last();
                var objects = ReadDataFromFile(fileDialog.FileName);

                _objectAmount = objects.Count;
                InitializeClusterSystem(objects);

                _averageClusterDistance = ClustersDistanceCalculator.GetAverageClusterDistance(_clusterSystem);
                Refresh();
                FirstFeatureMinLbl.Visibility = Visibility.Visible;
                FirstFeatureMaxLbl.Visibility = Visibility.Visible;
                SecondFeatureMinLbl.Visibility = Visibility.Visible;
                SecondFeatureMaxLbl.Visibility = Visibility.Visible;
                VisualizeClusters(_clusterSystem.Clusters.Values.ToList());
                ClusterizeBtn.IsEnabled = true;
            }
        }

        private void ClusterizeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dendrogram = _clusterizator.Clusterize((ClusterSystem)_clusterSystem.Clone());
                Revisualize();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void VisuzlizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Revisualize();
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
                    using (StreamWriter streamWriter = new StreamWriter(fileDialog.FileName))
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
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private List<ResearchObject> ReadDataFromFile(string path)
        {
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
            return objects;
        }

        private void InitializeClusterSystem(List<ResearchObject> objects)
        {
            var clusters = new List<Cluster>();
            int lastId = 0;
            foreach (var obj in objects)
            {
                var cluster = new Cluster(lastId, RGBColorCreator.GetRandomColor());
                lastId++;
                cluster.Objects.Add(obj);
                clusters.Add(cluster);
            }

            _clusterSystem = new ClusterSystem(clusters);
        }

        private void VisualizeClusters(List<Cluster> clusters)
        {
            VisualizationCanvas.Children.Clear();
            var brushConverter = new BrushConverter();
            DrawCoordinateLine();
            foreach (var cluster in clusters)
            {
                foreach (var obj in cluster.Objects)
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

                    elipse.Fill = new SolidColorBrush(Color.FromRgb(cluster.Color.R, cluster.Color.G, cluster.Color.B));

                    VisualizationCanvas.Children.Add(elipse);
                }
            }
        }

        private void Revisualize()
        {
            try
            {
                _clusters = _dendrogram.GetClusters(_clusterAmount);
                _currentClusterNumber = _clusters.Count;
                _averageClusterDistance = ClustersDistanceCalculator.GetAverageClusterDistance(_clusterSystem);
                Refresh();
                VisualizeClusters(_clusters);
                VisualizeBtn.IsEnabled = true;
                SaveDataBtn.IsEnabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        
        private double CalculateCoordinate(double currentMin, double currentMax, double realMin, double realMax, double current)
        {
            double positionCoefficient = Math.Abs(current - currentMin) / (currentMax - currentMin);
            return positionCoefficient * (realMax - realMin) + realMin;
        }

        private void ClusterizatorCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clusterizator = (FastLanceWilliamsAlgorithm)ClusterizatorCb.SelectedItem;
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
    }
}