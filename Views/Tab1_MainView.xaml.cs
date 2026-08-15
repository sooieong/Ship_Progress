using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Ship_Progress
{
    public partial class Tab1_MainView : UserControl, INotifyPropertyChanged
    {
        // -----------------------------------------------------------
        // INotifyPropertyChanged 구현
        // -----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChangedImpl([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // -----------------------------------------------------------
        // 1. 상단 반원 게이지 차트용 Series 프로퍼티
        // -----------------------------------------------------------
        private SeriesCollection _h120GaugeSeries;
        public SeriesCollection H120GaugeSeries
        {
            get => _h120GaugeSeries;
            set { _h120GaugeSeries = value; OnPropertyChangedImpl(); }
        }

        private SeriesCollection _h121GaugeSeries;
        public SeriesCollection H121GaugeSeries
        {
            get => _h121GaugeSeries;
            set { _h121GaugeSeries = value; OnPropertyChangedImpl(); }
        }

        private SeriesCollection _h122GaugeSeries;
        public SeriesCollection H122GaugeSeries
        {
            get => _h122GaugeSeries;
            set { _h122GaugeSeries = value; OnPropertyChangedImpl(); }
        }

        // -----------------------------------------------------------
        // 2. 중단 기자재 위험 현황 프로퍼티 (차트 & 우측 표)
        // -----------------------------------------------------------
        private SeriesCollection _equipmentRiskSeries;
        public SeriesCollection EquipmentRiskSeries
        {
            get => _equipmentRiskSeries;
            set { _equipmentRiskSeries = value; OnPropertyChangedImpl(); }
        }

        private string[] _equipmentRiskLabels;
        public string[] EquipmentRiskLabels
        {
            get => _equipmentRiskLabels;
            set { _equipmentRiskLabels = value; OnPropertyChangedImpl(); }
        }

        private int _totalCriticalCount;
        public int TotalCriticalCount
        {
            get => _totalCriticalCount;
            set { _totalCriticalCount = value; OnPropertyChangedImpl(); }
        }

        private int _totalWarningCount;
        public int TotalWarningCount
        {
            get => _totalWarningCount;
            set { _totalWarningCount = value; OnPropertyChangedImpl(); }
        }

        public Func<double, string> CountFormatter { get; set; } = value => value.ToString("N0");

        private ObservableCollection<EquipmentRiskItem> _criticalEquipmentList;
        public ObservableCollection<EquipmentRiskItem> CriticalEquipmentList
        {
            get => _criticalEquipmentList;
            set { _criticalEquipmentList = value; OnPropertyChangedImpl(); }
        }

        private List<EquipmentRiskItem> _originalCriticalEquipmentList = new List<EquipmentRiskItem>();
        private string _currentProcessFilter = "전체";

        // -----------------------------------------------------------
        // 3. 하단 좌측: 호선별 자재 납기율 오각형 차트 프로퍼티
        // -----------------------------------------------------------
        private PointCollection _radarPolygonPoints;
        public PointCollection RadarPolygonPoints
        {
            get => _radarPolygonPoints;
            set { _radarPolygonPoints = value; OnPropertyChangedImpl(); }
        }

        private ObservableCollection<Point> _radarDataPoints;
        public ObservableCollection<Point> RadarDataPoints
        {
            get => _radarDataPoints;
            set { _radarDataPoints = value; OnPropertyChangedImpl(); }
        }

        private Dictionary<string, double[]> shipVendorData = new Dictionary<string, double[]>()
        {
            // 순서: 기자재, 기관의장, 선체의장, 선실의장, 전기의장
            { "H120", new double[] { 70.89, 85.24, 75.90, 80.41, 90.14 } },
            { "H121", new double[] { 60.26, 50.82, 65.24, 60.42, 50.76 } },
            { "H122", new double[] { 30.11, 40.09, 35.21, 45.43, 40.07 } }
        };

        // -----------------------------------------------------------
        // 4. 하단 우측: 품목 수급 현황 (도넛 차트 및 텍스트) 프로퍼티
        // -----------------------------------------------------------
        private SeriesCollection _supplyStatusSeries;
        public SeriesCollection SupplyStatusSeries
        {
            get => _supplyStatusSeries;
            set { _supplyStatusSeries = value; OnPropertyChangedImpl(); }
        }

        private string _onTimePercentageText = "92.8%";
        public string OnTimePercentageText
        {
            get => _onTimePercentageText;
            set { _onTimePercentageText = value; OnPropertyChangedImpl(); }
        }

        private string _delayedPercentageText = "4.9%";
        public string DelayedPercentageText
        {
            get => _delayedPercentageText;
            set { _delayedPercentageText = value; OnPropertyChangedImpl(); }
        }

        private string _pendingPercentageText = "2.7%";
        public string PendingPercentageText
        {
            get => _pendingPercentageText;
            set { _pendingPercentageText = value; OnPropertyChangedImpl(); }
        }

        // -----------------------------------------------------------
        // 데이터 모델 정의
        // -----------------------------------------------------------
        public class ProcessStatusItem
        {
            public string ShipNo { get; set; }
            public Brush SteelCuttingColor { get; set; }
            public Brush BlockAssemblyColor { get; set; }
            public Brush ErectionColor { get; set; }
            public Brush OutfittingColor { get; set; }
            public Brush SeaTrialColor { get; set; }
        }

        public class EquipmentRiskItem
        {
            public string Series { get; set; }
            public string ShipNo { get; set; }
            public string Process { get; set; }
            public string IssueDetail { get; set; }
            public string DelayDays { get; set; }
            public string Status { get; set; }

            public Brush StatusBgColor
            {
                get
                {
                    switch (Status)
                    {
                        case "위험": return new SolidColorBrush(Color.FromArgb(50, 229, 57, 53));
                        case "주의": return new SolidColorBrush(Color.FromArgb(50, 250, 140, 22));
                        case "정상": return new SolidColorBrush(Color.FromArgb(50, 76, 175, 80));
                        default: return Brushes.Transparent;
                    }
                }
            }

            public Brush StatusFgColor
            {
                get
                {
                    switch (Status)
                    {
                        case "위험": return new SolidColorBrush(Color.FromRgb(229, 57, 53));
                        case "주의": return new SolidColorBrush(Color.FromRgb(250, 140, 22));
                        case "정상": return new SolidColorBrush(Color.FromRgb(34, 197, 94));
                        default: return Brushes.White;
                    }
                }
            }
        }

        // -----------------------------------------------------------
        // 생성자 및 로드 이벤트
        // -----------------------------------------------------------
        public Tab1_MainView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGaugeData();
            LoadProcessStatusData();
            LoadEquipmentRiskData();
            LoadVendorDeliveryData("전체 (평균)");
            LoadSupplyStatusData("전체 (평균)");
        }

        // -----------------------------------------------------------
        // 1. 반원 게이지 로드 함수
        // -----------------------------------------------------------
        private void LoadGaugeData()
        {
            H120GaugeSeries = CreateSemiCircleGaugeSeries(80.2, Color.FromRgb(76, 175, 80));
            H121GaugeSeries = CreateSemiCircleGaugeSeries(58.8, Color.FromRgb(76, 175, 80));
            H122GaugeSeries = CreateSemiCircleGaugeSeries(22.4, Color.FromRgb(76, 175, 80));
        }

        private SeriesCollection CreateSemiCircleGaugeSeries(double percentage, Color activeColor)
        {
            return new SeriesCollection
            {
                new PieSeries { Values = new ChartValues<double> { percentage }, Fill = new SolidColorBrush(activeColor), StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { 100 - percentage }, Fill = new SolidColorBrush(Color.FromRgb(224, 224, 230)), StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { 100 }, Fill = Brushes.Transparent, StrokeThickness = 0 }
            };
        }

        // -----------------------------------------------------------
        // 2. 호선별 공정 현황 로드 함수
        // -----------------------------------------------------------
        private void LoadProcessStatusData()
        {
            var green = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            var orange = (SolidColorBrush)Application.Current.Resources["HanwhaOrangeBrush"];
            var gray = new SolidColorBrush(Color.FromRgb(224, 224, 224));

            var processList = new List<ProcessStatusItem>
            {
                new ProcessStatusItem { ShipNo = "H120", SteelCuttingColor = green, BlockAssemblyColor = green, ErectionColor = green, OutfittingColor = orange, SeaTrialColor = gray },
                new ProcessStatusItem { ShipNo = "H121", SteelCuttingColor = green, BlockAssemblyColor = green, ErectionColor = orange, OutfittingColor = gray, SeaTrialColor = gray },
                new ProcessStatusItem { ShipNo = "H122", SteelCuttingColor = green, BlockAssemblyColor = orange, ErectionColor = gray, OutfittingColor = gray, SeaTrialColor = gray }
            };

            ProcessStatusDataGrid.ItemsSource = processList;
        }

        // -----------------------------------------------------------
        // 3. 기자재 위험 현황 로드 함수
        // -----------------------------------------------------------
        private void LoadEquipmentRiskData()
        {
            var riskDataList = new List<EquipmentRiskItem>
            {
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "기관", IssueDetail = "배관 팽창 루프 서포트", DelayDays = "8일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "선실", IssueDetail = "거주구 창문 코밍", DelayDays = "8일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "선실", IssueDetail = "현수사다리 거치 브라켓", DelayDays = "11일", Status = "위험" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "전기", IssueDetail = "전장 단로기 부착 브라켓", DelayDays = "10일", Status = "위험" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "전기", IssueDetail = "세면대/수도 배관 서포트", DelayDays = "7일", Status = "주의" },

                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "기관", IssueDetail = "래싱 브릿지 Cross Bar", DelayDays = "7일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "선체", IssueDetail = "수직 사다리 A타입", DelayDays = "10일", Status = "위험" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "전기", IssueDetail = "케이블 윈치 러그", DelayDays = "6일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "전기", IssueDetail = "자동 전화기", DelayDays = "3일", Status = "주의" },

                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H122", Process = "선체", IssueDetail = "수직 사다리 A타입", DelayDays = "10일", Status = "위험" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H122", Process = "선실", IssueDetail = "현수사다리 거치 브라켓", DelayDays = "8일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H122", Process = "전기", IssueDetail = "케이블 윈치 러그", DelayDays = "6일", Status = "주의" },
                new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H122", Process = "전기", IssueDetail = "자동 전화기", DelayDays = "3일", Status = "주의" }
            };

            _originalCriticalEquipmentList = riskDataList;
            _currentProcessFilter = "전체";

            ApplyProcessFilter();

            TotalCriticalCount = riskDataList.Count(x => x.Status == "위험");
            TotalWarningCount = riskDataList.Count(x => x.Status == "주의");

            EquipmentRiskLabels = new string[] { "H122", "H121", "H120" };

            var dangerValues = new ChartValues<double>();
            var warningValues = new ChartValues<double>();
            var normalValues = new ChartValues<double>();

            foreach (var shipNo in EquipmentRiskLabels)
            {
                dangerValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "위험"));
                warningValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "주의"));
                normalValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "정상"));
            }

            Func<ChartPoint, string> countFormatter = point =>
            {
                if (point.Instance is double rawValue && rawValue > 0)
                {
                    return $"{rawValue}건";
                }
                return "";
            };

            EquipmentRiskSeries = new SeriesCollection
            {
                new StackedRowSeries
                {
                    Title = "위험",
                    Values = dangerValues,
                    Fill = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    StackMode = StackMode.Percentage,
                    DataLabels = true,
                    LabelPoint = countFormatter,
                    LabelsPosition = BarLabelPosition.Parallel,
                    FontSize = 11,
                    Foreground = Brushes.White,
                    MaxRowHeight = 16
                },
                new StackedRowSeries
                {
                    Title = "주의",
                    Values = warningValues,
                    Fill = new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                    StackMode = StackMode.Percentage,
                    DataLabels = true,
                    LabelPoint = countFormatter,
                    LabelsPosition = BarLabelPosition.Parallel,
                    FontSize = 11,
                    Foreground = Brushes.White,
                    MaxRowHeight = 16
                },
            };
        }

        // -----------------------------------------------------------
        // [구분] 헤더 버튼 클릭 시 동적 메뉴 생성 및 팝업
        // -----------------------------------------------------------
        private void ProcessFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu menu = new ContextMenu();

                var distinctProcesses = _originalCriticalEquipmentList.Select(x => x.Process).Distinct().OrderBy(x => x).ToList();

                MenuItem allItem = new MenuItem { Header = "전체 보기", Tag = "전체" };
                allItem.Click += (s, ev) => { _currentProcessFilter = "전체"; ApplyProcessFilter(); };
                menu.Items.Add(allItem);

                foreach (var process in distinctProcesses)
                {
                    MenuItem item = new MenuItem { Header = process, Tag = process };
                    item.Click += (s, ev) => { _currentProcessFilter = process; ApplyProcessFilter(); };
                    menu.Items.Add(item);
                }

                btn.ContextMenu = menu;
                menu.PlacementTarget = btn;
                menu.IsOpen = true;
            }
        }

        private void ApplyProcessFilter()
        {
            if (CriticalEquipmentDataGrid == null) return;

            var query = _originalCriticalEquipmentList.AsEnumerable();

            if (_currentProcessFilter != "전체")
            {
                query = query.Where(x => x.Process == _currentProcessFilter);
            }

            var sortedResult = query.OrderByDescending(x => GetDaysNumber(x.DelayDays)).ToList();
            CriticalEquipmentList = new ObservableCollection<EquipmentRiskItem>(sortedResult);
            CriticalEquipmentDataGrid.ItemsSource = CriticalEquipmentList;
        }

        private int GetDaysNumber(string delayDaysText)
        {
            if (string.IsNullOrEmpty(delayDaysText)) return 0;
            var match = Regex.Match(delayDaysText, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        // -----------------------------------------------------------
        // 4. 호선별 자재 납기율 (오각형 분포도) 로드 및 드롭다운 연동 함수
        // -----------------------------------------------------------
        private void ShipSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShipSelectionComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                string selection = selectedItem.Content.ToString();
                LoadVendorDeliveryData(selection);
                LoadSupplyStatusData(selection);
            }
        }

        private void LoadVendorDeliveryData(string targetKey)
        {
            double[] values = new double[5];

            if (targetKey.Contains("전체"))
            {
                for (int i = 0; i < 5; i++)
                {
                    values[i] = shipVendorData.Values.Average(arr => arr[i]);
                }
            }
            else if (shipVendorData.ContainsKey(targetKey))
            {
                values = shipVendorData[targetKey];
            }

            double centerX = 130;
            double centerY = 110;
            double maxRadius = 90;

            double[] angles = new double[] { -90, -18, 54, 126, 198 };

            var points = new PointCollection();
            var pointList = new ObservableCollection<Point>();

            for (int i = 0; i < 5; i++)
            {
                double rad = angles[i] * Math.PI / 180.0;
                double r = maxRadius * (values[i] / 100.0);

                double x = centerX + r * Math.Cos(rad);
                double y = centerY + r * Math.Sin(rad);

                Point p = new Point(x, y);
                points.Add(p);
                pointList.Add(p);
            }

            RadarPolygonPoints = points;
            RadarDataPoints = pointList;
        }

        // -----------------------------------------------------------
        // 5. 품목 수급 현황 (도넛 차트 및 텍스트) 데이터 로드 및 갱신 함수
        // -----------------------------------------------------------
        private void LoadSupplyStatusData(string targetKey = "전체 (평균)")
        {
            double onTimeVal = 0;
            double delayedVal = 0;
            double pendingVal = 0;

            if (targetKey.Contains("전체"))
            {
                double[] h120Stats = CalculateShipKpiRatio("H120");
                double[] h121Stats = CalculateShipKpiRatio("H121");
                double[] h122Stats = CalculateShipKpiRatio("H122");

                onTimeVal = (h120Stats[0] + h121Stats[0] + h122Stats[0]) / 3.0;
                delayedVal = (h120Stats[1] + h121Stats[1] + h122Stats[1]) / 3.0;
                pendingVal = (h120Stats[2] + h121Stats[2] + h122Stats[2]) / 3.0;
            }
            else
            {
                double[] stats = CalculateShipKpiRatio(targetKey);
                onTimeVal = stats[0];
                delayedVal = stats[1];
                pendingVal = stats[2];
            }

            OnTimePercentageText = $"{onTimeVal:F1}%";
            DelayedPercentageText = $"{delayedVal:F1}%";
            PendingPercentageText = $"{pendingVal:F1}%";

            SupplyStatusSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "납품 완료",
                    Values = new ChartValues<double> { onTimeVal },
                    Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    StrokeThickness = 0
                },
                new PieSeries
                {
                    Title = "지연",
                    Values = new ChartValues<double> { delayedVal },
                    Fill = new SolidColorBrush(Color.FromRgb(234, 179, 8)), // 머스타드색 (주의/지연 색상)
                    StrokeThickness = 0
                },
                new PieSeries
                {
                    Title = "보류/대기",
                    Values = new ChartValues<double> { pendingVal },
                    Fill = new SolidColorBrush(Color.FromRgb(229, 57, 53)), // 빨간색 (위험/보류 색상)
                    StrokeThickness = 0
                }
            };
        }

        private double[] CalculateShipKpiRatio(string shipNo)
        {
            int completed = 0, delayed = 0, pending = 0;

            if (shipNo == "H120")
            {
                completed = 18; delayed = 3; pending = 1;
            }
            else if (shipNo == "H121")
            {
                completed = 15; delayed = 5; pending = 2;
            }
            else if (shipNo == "H122")
            {
                completed = 12; delayed = 7; pending = 3;
            }

            int total = completed + delayed + pending;
            if (total == 0) return new double[] { 0, 0, 0 };

            double cPct = ((double)completed / total) * 100.0;
            double dPct = ((double)delayed / total) * 100.0;
            double pPct = ((double)pending / total) * 100.0;

            return new double[] { cPct, dPct, pPct };
        }
    }
}