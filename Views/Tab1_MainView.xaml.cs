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

        public Func<double, string> CountFormatter { get; set; } = value => value.ToString("N0");

        private ObservableCollection<EquipmentRiskItem> _criticalEquipmentList;
        public ObservableCollection<EquipmentRiskItem> CriticalEquipmentList
        {
            get => _criticalEquipmentList;
            set { _criticalEquipmentList = value; OnPropertyChangedImpl(); }
        }

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

        // [신규 추가] 호선별 5대 부문 임의 데이터 (기자재, 기관의장, 선체의장, 선실의장, 전기의장 순서)
        private Dictionary<string, double[]> shipVendorData = new Dictionary<string, double[]>()
        {
            { "H120", new double[] { 85.0, 90.0, 78.0, 92.0, 88.0 } },
            { "H121", new double[] { 75.0, 82.0, 80.0, 70.0, 85.0 } },
            { "H122", new double[] { 95.0, 88.0, 91.0, 94.0, 90.0 } }
        };

        // -----------------------------------------------------------
        // 4. 하단 우측: 기자재 수급 현황 (도넛 차트 및 텍스트) 프로퍼티
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
                        case "정상": return new SolidColorBrush(Color.FromArgb(50, 34, 197, 94));
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
            LoadVendorDeliveryData("전체 (평균)"); // 기본값 전체(평균)으로 로드
            LoadSupplyStatusData();
        }

        // -----------------------------------------------------------
        // 1. 반원 게이지 로드 함수
        // -----------------------------------------------------------
        private void LoadGaugeData()
        {
            H120GaugeSeries = CreateSemiCircleGaugeSeries(87.2, Color.FromRgb(76, 175, 80));
            H121GaugeSeries = CreateSemiCircleGaugeSeries(82.1, Color.FromRgb(255, 152, 0));
            H122GaugeSeries = CreateSemiCircleGaugeSeries(91.3, Color.FromRgb(76, 175, 80));
        }

        private SeriesCollection CreateSemiCircleGaugeSeries(double percentage, Color activeColor)
        {
            return new SeriesCollection
            {
                new PieSeries { Values = new ChartValues<double> { percentage }, Fill = new SolidColorBrush(activeColor), StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { 100 - percentage }, Fill = new SolidColorBrush(Color.FromRgb(220, 224, 230)), StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { 100 }, Fill = Brushes.Transparent, StrokeThickness = 0 }
            };
        }

        // -----------------------------------------------------------
        // 2. 호선별 공정 현황 로드 함수
        // -----------------------------------------------------------
        private void LoadProcessStatusData()
        {
            var green = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            var orange = new SolidColorBrush(Color.FromRgb(250, 140, 22));
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
        new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "의장", IssueDetail = "메인엔진 패키지 입고 지연", DelayDays = "14일", Status = "위험" },
        new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H120", Process = "배관", IssueDetail = "고압 LNG 밸브 수급 소폭 지연", DelayDays = "4일", Status = "주의" },
        new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "기관", IssueDetail = "선박평형수 처리장치(BWTS) 펌프 입고 지연", DelayDays = "7일", Status = "주의" },
        new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "전기", IssueDetail = "스위치보드 배전반 서류 검사 지연", DelayDays = "3일", Status = "정상" },
        new EquipmentRiskItem { Series = "A SERIES", ShipNo = "H121", Process = "도장", IssueDetail = "특수 방오도료 수급 소폭 지연", DelayDays = "2일", Status = "정상" },
        new EquipmentRiskItem { Series = "B SERIES", ShipNo = "H122", Process = "의장", IssueDetail = "선실 거주구 판넬 자재 입고 소폭 지연", DelayDays = "2일", Status = "정상" },
        new EquipmentRiskItem { Series = "B SERIES", ShipNo = "H122", Process = "기관", IssueDetail = "비상발전기(Emergency Gen) 테스트 일정 조정", DelayDays = "5일", Status = "주의" },
        new EquipmentRiskItem { Series = "B SERIES", ShipNo = "H122", Process = "배관", IssueDetail = "유압 파이프 자재 현장 수령 소폭 지연", DelayDays = "1일", Status = "정상" }
    };

            var sortedList = riskDataList
                .OrderByDescending(x => GetDaysNumber(x.DelayDays))
                .ToList();

            CriticalEquipmentList = new ObservableCollection<EquipmentRiskItem>(sortedList);
            TotalCriticalCount = riskDataList.Count(x => x.Status == "위험");

            // Y축 레이블 정의 (아래에서 위 순서: H122 -> H121 -> H120)
            EquipmentRiskLabels = new string[] { "H122", "H121", "H120" };

            // -----------------------------------------------------------
            // [동적 데이터 집계] EquipmentRiskLabels 순서에 맞게 건수 자동 계산
            // -----------------------------------------------------------
            var dangerValues = new ChartValues<double>();
            var warningValues = new ChartValues<double>();
            var normalValues = new ChartValues<double>();

            foreach (var shipNo in EquipmentRiskLabels)
            {
                // 해당 호선의 상태별 개수를 동적으로 카운트
                dangerValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "위험"));
                warningValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "주의"));
                normalValues.Add(riskDataList.Count(x => x.ShipNo == shipNo && x.Status == "정상"));
            }

            // Formatter 설정
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
            Values = dangerValues, // 동적 집계된 Values 할당
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
            Values = warningValues, // 동적 집계된 Values 할당
            Fill = new SolidColorBrush(Color.FromRgb(250, 140, 22)),
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
            }
        }

        private void LoadVendorDeliveryData(string targetKey)
        {
            double[] values = new double[5];

            if (targetKey.Contains("전체"))
            {
                // 각 호선별 값들의 진짜 평균 계산 (기자재, 기관의장, 선체의장, 선실의장, 전기의장 각 항목별 평균)
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

            // 5대 축 각도 (기자재: -90도, 기관의장: -18도, 선체의장: 54도, 선실의장: 126도, 전기의장: 198도)
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
        // 5. 기자재 수급 현황 도넛 차트 데이터 로드 함수
        // -----------------------------------------------------------
        private void LoadSupplyStatusData()
        {
            double onTimeVal = 92.8;
            double delayedVal = 4.9;
            double pendingVal = 2.7;

            OnTimePercentageText = $"{onTimeVal}%";
            DelayedPercentageText = $"{delayedVal}%";
            PendingPercentageText = $"{pendingVal}%";

            SupplyStatusSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "On Time",
                    Values = new ChartValues<double> { onTimeVal },
                    Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                    StrokeThickness = 0
                },
                new PieSeries
                {
                    Title = "Delayed",
                    Values = new ChartValues<double> { delayedVal },
                    Fill = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    StrokeThickness = 0
                },
                new PieSeries
                {
                    Title = "Pending",
                    Values = new ChartValues<double> { pendingVal },
                    Fill = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    StrokeThickness = 0
                }
            };
        }
    }
}