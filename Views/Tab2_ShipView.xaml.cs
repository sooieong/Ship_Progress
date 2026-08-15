using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ship_Progress
{
    public class BlockProgressItem
    {
        public string Title { get; set; }
        public double Progress { get; set; }
        public Brush BarColor { get; set; }
        public string ProgressText => $"{Progress}%";

        public BlockProgressItem(string title, double progress)
        {
            Title = title;
            Progress = progress;

            if (progress >= 90)
            {
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            }
            else if (progress >= 60)
            {
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAB308"));
            }
            else
            {
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
            }
        }
    }

    public class ProcessStepItem : INotifyPropertyChanged
    {
        private string _stepName;
        public string StepName
        {
            get => _stepName;
            set { _stepName = value; OnPropertyChanged(); }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); OnPropertyChanged(); }
        }

        private Brush _circleColor;
        public Brush CircleColor
        {
            get => _circleColor;
            set { _circleColor = value; OnPropertyChanged(); }
        }

        private Brush _statusTextColor;
        public Brush StatusTextColor
        {
            get => _statusTextColor;
            set { _statusTextColor = value; OnPropertyChanged(); }
        }

        private bool _isCurrentActive;
        public bool IsCurrentActive
        {
            get => _isCurrentActive;
            set { _isCurrentActive = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class Tab2_ShipView : UserControl, INotifyPropertyChanged
    {
        private string[] dateLabels;

        private string _selectedShipNo = "H120";
        public string SelectedShipNo
        {
            get => _selectedShipNo;
            set { _selectedShipNo = value; OnPropertyChanged(); }
        }

        private string _selectedSeriesName = "A SERIES";
        public string SelectedSeriesName
        {
            get => _selectedSeriesName;
            set { _selectedSeriesName = value; OnPropertyChanged(); }
        }

        private string _overallProgress = "80.2";
        public string OverallProgress
        {
            get => _overallProgress;
            set { _overallProgress = value; OnPropertyChanged(); }
        }

        private string _expectedEndDate = "2026-11-20";
        public string ExpectedEndDate
        {
            get => _expectedEndDate;
            set { _expectedEndDate = value; OnPropertyChanged(); }
        }

        private string _startDate = "2026-05-05";
        public string StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private string _midExpectedEndDate = "2026-09-30";
        public string MidExpectedEndDate
        {
            get => _midExpectedEndDate;
            set { _midExpectedEndDate = value; OnPropertyChanged(); }
        }

        private string _progressRate = "62%";
        public string ProgressRate
        {
            get => _progressRate;
            set { _progressRate = value; OnPropertyChanged(); }
        }

        private List<BlockProgressItem> _blockProgressList;
        public List<BlockProgressItem> BlockProgressList
        {
            get => _blockProgressList;
            set { _blockProgressList = value; OnPropertyChanged(); }
        }

        private List<ProcessStepItem> _processStepsList;
        public List<ProcessStepItem> ProcessStepsList
        {
            get => _processStepsList;
            set { _processStepsList = value; OnPropertyChanged(); }
        }

        public class H120RiskItem
        {
            public string Series { get; set; }
            public string ShipNo { get; set; }
            public string Category { get; set; }
            public string EquipmentName { get; set; }
            public string Description { get; set; }
            public int DelayDays { get; set; }
            public string Status { get; set; }
            public Brush StatusBgColor { get; set; }
            public Brush StatusFgColor { get; set; }
            public string MainProcessImpact { get; set; }
            public string DelayText => $"{DelayDays}일";

            public H120RiskItem(string series, string shipNo, string category, string equipmentName, string description, int delayDays, string status, string mainProcessImpact)
            {
                Series = series;
                ShipNo = shipNo;
                Category = category;
                EquipmentName = equipmentName;
                Description = description;
                DelayDays = delayDays;
                Status = status;
                MainProcessImpact = mainProcessImpact;

                var converter = new BrushConverter();
                if (status == "위험")
                {
                    StatusBgColor = (Brush)converter.ConvertFromString("#FFEBEE");
                    StatusFgColor = (Brush)converter.ConvertFromString("#D32F2F");
                }
                else // "주의" 일 때 이미지의 주황빛 색상으로 적용
                {
                    StatusBgColor = (Brush)converter.ConvertFromString("#FFF3E0");
                    StatusFgColor = (Brush)converter.ConvertFromString("#E65100");
                }
            }
        }

        // 전체 위험 품목 마스터 리스트
        private List<H120RiskItem> _allRiskList = new List<H120RiskItem>();
        private string _currentCategoryFilter = "전체";

        public Tab2_ShipView()
        {
            InitializeComponent();
            this.DataContext = this;

            // 전체 마스터 데이터 초기화 (H120, H121, H122 통합)
            InitializeMasterRiskData();

            ChartCanvas.MouseDown += (s, e) =>
            {
                PointTooltip.Visibility = Visibility.Collapsed;
            };
        }

        private void InitializeMasterRiskData()
        {
            _allRiskList = new List<H120RiskItem>
    {
        // --- H120 데이터 ---
        new H120RiskItem("A SERIES", "H120", "기관", "자재 수급 지연으로 배관 팽창 루프 서포트 제작 지연", "", 8, "주의", "기관실의장(+5일)"),
        new H120RiskItem("A SERIES", "H120", "선실", "상세 설계 승인으로 거주구 창문 코밍 생산 일정 지연", "", 8, "주의", "선실의장(+5일)"),
        new H120RiskItem("A SERIES", "H120", "선실", "공장 점검으로 현수사다리 거치 브라켓 납품 지연", "", 11, "위험", "선실의장(+8일)"),
        new H120RiskItem("A SERIES", "H120", "전기", "검사 기준 강화로 전장 단로기 부착 브라켓 출하 지연", "", 10, "위험", "선실의장(+7일)"),
        new H120RiskItem("A SERIES", "H120", "전기", "물량 폭주로 세면대/수도 배관 서포트 일정 조정", "", 7, "주의", "선실의장(+4일)"),

        // --- H121 데이터 ---
        new H120RiskItem("A SERIES", "H121", "기관", "용접 지연으로 래싱 브릿지 Cross Bar 납품 지연", "", 7, "주의", "기관실의장(+4일)"),
        new H120RiskItem("A SERIES", "H121", "선체", "원자재 입고 지연으로 수직 사다리 A타입 생산 지연", "", 10, "위험", "선체 도장(+6일)"),
        new H120RiskItem("A SERIES", "H121", "전기", "도면 수정 작업으로 케이블 윈치 러그 제작 지연", "", 6, "주의", "선수 블록조립(+4일)"),
        new H120RiskItem("A SERIES", "H121", "전기", "조달 기간 연장으로 자동 전화기 납품 일정 조정", "", 3, "주의", "선실의장(+1일)"),

        // --- H122 데이터 ---
        new H120RiskItem("A SERIES", "H122", "선체", "원자재 입고 지연으로 수직 사다리 A타입 생산 지연", "", 10, "위험", "하부구조(+6일)"),
        new H120RiskItem("A SERIES", "H122", "선실", "공장 내부 보수로 현수사다리 거치 브라켓 납품 지연", "", 8, "주의", "상부의장(+4일)"),
        new H120RiskItem("A SERIES", "H122", "전기", "도면 수정 작업으로 케이블 윈치 러그 제작 지연", "", 6, "주의", "선수 블록조립(+3일)"),
        new H120RiskItem("A SERIES", "H122", "전기", "조달 기간 연장으로 자동 전화기 납품 일정 조정", "", 3, "주의", "상부의장(+1일)")
    };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadShipDashboardData("H120");
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSCurveChart();
        }

        private void ShipCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is string shipNo)
            {
                LoadShipDashboardData(shipNo);
            }
        }

        public void LoadShipDashboardData(string shipNo)
        {
            SelectedShipNo = shipNo;
            SelectedSeriesName = shipNo == "H122" ? "A SERIES" : "A SERIES";

            string[] stepNames = { "강재절단", "블록조립", "탑재", "의장", "시운전" };
            int activeIndex = 0;

            if (shipNo == "H120")
            {
                OverallProgress = "80.2";
                ExpectedEndDate = "2026-12-20";
                StartDate = "2025-06-20";
                MidExpectedEndDate = "2026-08-20";
                ProgressRate = "80%";
                activeIndex = 3;
                dateLabels = new string[] { "2025.06", "2025.10", "2026.02", "2026.06", "2026.08", "2026.12" };
            }
            else if (shipNo == "H121")
            {
                OverallProgress = "58.8";
                ExpectedEndDate = "2027-03-29";
                StartDate = "2025-09-29";
                MidExpectedEndDate = "2026-10-20";
                ProgressRate = "59%";
                activeIndex = 2;
                dateLabels = new string[] { "2025.10", "2026.02", "2026.06", "2026.10", "2027.01", "2027.03" };
            }
            else if (shipNo == "H122")
            {
                OverallProgress = "22.4";
                ExpectedEndDate = "2027-09-12";
                StartDate = "2026-03-12";
                MidExpectedEndDate = "2027-04-20";
                ProgressRate = "22%";
                activeIndex = 1;
                dateLabels = new string[] { "2026.04", "2026.08", "2026.12", "2027.04", "2027.08", "2027.09" };
            }

            var newSteps = new List<ProcessStepItem>();
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            var orangeBrush = Application.Current.Resources["HanwhaOrangeBrush"] as Brush ?? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F37321"));
            var grayBrush = Application.Current.Resources["BorderBrush"] as Brush ?? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B0"));
            var primaryTextBrush = Application.Current.Resources["PrimaryTextBrush"] as Brush ?? Brushes.Black;

            for (int i = 0; i < stepNames.Length; i++)
            {
                var step = new ProcessStepItem { StepName = stepNames[i] };

                if (i < activeIndex)
                {
                    step.StatusText = "완료";
                    step.CircleColor = greenBrush;
                    step.StatusTextColor = greenBrush;
                    step.IsCurrentActive = false;
                }
                else if (i == activeIndex)
                {
                    step.StatusText = "진행중";
                    step.CircleColor = orangeBrush;
                    step.StatusTextColor = orangeBrush;
                    step.IsCurrentActive = true;
                }
                else
                {
                    step.StatusText = "대기";
                    step.CircleColor = grayBrush;
                    step.StatusTextColor = primaryTextBrush;
                    step.IsCurrentActive = false;
                }
                newSteps.Add(step);
            }
            ProcessStepsList = newSteps;

            DrawSCurveChart();

            if (shipNo == "H120")
            {
                BlockProgressList = new List<BlockProgressItem>
                {
                    new BlockProgressItem("선실 / 거주구", 95),
                    new BlockProgressItem("기관 제어실", 90),
                    new BlockProgressItem("조타실", 85),
                    new BlockProgressItem("갑판 의장", 75),
                    new BlockProgressItem("연료 공급 장치", 60)
                };
            }
            else if (shipNo == "H121")
            {
                BlockProgressList = new List<BlockProgressItem>
                {
                    new BlockProgressItem("기관실 대형 주기계", 100),
                    new BlockProgressItem("발전기 엔진 및 보일러", 100),
                    new BlockProgressItem("추진 계통", 85),
                    new BlockProgressItem("갑판 대형 장비", 70),
                    new BlockProgressItem("화물창", 45)
                };
            }
            else if (shipNo == "H122")
            {
                BlockProgressList = new List<BlockProgressItem>
                {
                    new BlockProgressItem("선수 / 기관실 구역", 45),
                    new BlockProgressItem("선체바닥 / 하부구조", 35),
                    new BlockProgressItem("외판 및 갑판구역", 25),
                    new BlockProgressItem("선수구역", 15),
                    new BlockProgressItem("거주구 및 상부의장", 10)
                };
            }

            // 호선이 바뀔 때 카테고리 필터 초기화 및 해당 호선 데이터 적용
            _currentCategoryFilter = "전체";
            ApplyCategoryFilter();
        }

        private void ProcessFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu menu = new ContextMenu();
                // 현재 선택된 호선의 데이터에서만 카테고리 추출
                var distinctCategories = _allRiskList
                    .Where(x => x.ShipNo == SelectedShipNo)
                    .Select(x => x.Category)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                MenuItem allItem = new MenuItem { Header = "전체 보기", Tag = "전체" };
                allItem.Click += (s, ev) => { _currentCategoryFilter = "전체"; ApplyCategoryFilter(); };
                menu.Items.Add(allItem);

                foreach (var cat in distinctCategories)
                {
                    MenuItem item = new MenuItem { Header = cat, Tag = cat };
                    item.Click += (s, ev) => { _currentCategoryFilter = cat; ApplyCategoryFilter(); };
                    menu.Items.Add(item);
                }

                btn.ContextMenu = menu;
                menu.PlacementTarget = btn;
                menu.IsOpen = true;
            }
        }

        private void ApplyCategoryFilter()
        {
            if (H120RiskDataGrid == null) return;

            // 1차: 현재 선택된 호선(SelectedShipNo)으로 필터링
            var query = _allRiskList.Where(x => x.ShipNo == SelectedShipNo);

            // 2차: 카테고리 필터가 "전체"가 아니면 추가 필터링
            if (_currentCategoryFilter != "전체")
            {
                query = query.Where(x => x.Category == _currentCategoryFilter);
            }

            // 💡 3차: 지연 일수(DelayDays) 기준 내림차순 정렬 적용
            var sortedQuery = query.OrderByDescending(x => x.DelayDays).ToList();

            H120RiskDataGrid.ItemsSource = sortedQuery;
        }

        private void DrawSCurveChart()
        {
            if (ChartCanvas == null || ChartCanvas.ActualWidth <= 0 || ChartCanvas.ActualHeight <= 0) return;

            var elementsToRemove = ChartCanvas.Children.Cast<UIElement>()
                .Where(e => e != PointTooltip)
                .ToList();

            foreach (var elem in elementsToRemove)
            {
                ChartCanvas.Children.Remove(elem);
            }

            double[] planDataValues = { 0, 8, 25, 60, 88, 100 };
            double[] actualDataValues;

            if (SelectedShipNo == "H120")
            {
                actualDataValues = new double[] { 0, 10.9, 30.3, 62.4, 80.2 };
            }
            else if (SelectedShipNo == "H121")
            {
                actualDataValues = new double[] { 0, 12.2, 38.3, 58.8 };
            }
            else
            {
                actualDataValues = new double[] { 0, 22.4 };
            }

            double width = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;

            double paddingLeft = 40;
            double paddingRight = 30;
            double paddingTop = 20;
            double paddingBottom = 40;

            double chartW = width - paddingLeft - paddingRight;
            double chartH = height - paddingTop - paddingBottom;

            Brush textBrush = Application.Current.Resources["PrimaryTextBrush"] as Brush ?? Brushes.Black;

            for (int i = 0; i <= 5; i++)
            {
                double yVal = i * 20;
                double yPos = paddingTop + chartH - (yVal / 100.0 * chartH);

                Line gridLine = new Line { X1 = paddingLeft, Y1 = yPos, X2 = width - paddingRight, Y2 = yPos, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")), StrokeThickness = 1 };
                ChartCanvas.Children.Add(gridLine);

                TextBlock yLabel = new TextBlock { Text = $"{yVal}%", FontSize = 10, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
                Canvas.SetLeft(yLabel, 5);
                Canvas.SetTop(yLabel, yPos - 7);
                ChartCanvas.Children.Add(yLabel);
            }

            int dateCount = (dateLabels != null && dateLabels.Length > 0) ? dateLabels.Length : planDataValues.Length;
            if (dateLabels != null && dateLabels.Length > 0)
            {
                for (int m = 0; m < dateLabels.Length; m++)
                {
                    double xPos = paddingLeft + (m / (double)(dateLabels.Length - 1) * chartW);
                    TextBlock xLabel = new TextBlock { Text = dateLabels[m], FontSize = 10, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
                    Canvas.SetLeft(xLabel, xPos - 12);
                    Canvas.SetTop(xLabel, height - paddingBottom + 6);
                    ChartCanvas.Children.Add(xLabel);
                }
            }

            Path planPath = new Path { Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)), StrokeThickness = 2 };
            PathGeometry planGeometry = new PathGeometry();
            PathFigure planFigure = new PathFigure();

            if (planDataValues.Length > 0)
            {
                double startX = paddingLeft;
                double startY = paddingTop + chartH - (planDataValues[0] / 100.0 * chartH);
                planFigure.StartPoint = new Point(startX, startY);

                for (int i = 0; i < planDataValues.Length - 1; i++)
                {
                    double x1 = paddingLeft + (i / (double)(dateCount - 1) * chartW);
                    double y1 = paddingTop + chartH - (planDataValues[i] / 100.0 * chartH);
                    double x2 = paddingLeft + ((i + 1) / (double)(dateCount - 1) * chartW);
                    double y2 = paddingTop + chartH - (planDataValues[i + 1] / 100.0 * chartH);

                    Point cp1 = new Point(x1 + (x2 - x1) / 2, y1);
                    Point cp2 = new Point(x1 + (x2 - x1) / 2, y2);

                    planFigure.Segments.Add(new BezierSegment(cp1, cp2, new Point(x2, y2), true));
                }
            }
            planGeometry.Figures.Add(planFigure);
            planPath.Data = planGeometry;
            ChartCanvas.Children.Add(planPath);

            Path actualPath = new Path { Stroke = new SolidColorBrush(Color.FromRgb(243, 115, 33)), StrokeThickness = 2.5 };
            PathGeometry actualGeometry = new PathGeometry();
            PathFigure actualFigure = new PathFigure();

            if (actualDataValues.Length > 0)
            {
                double startX = paddingLeft;
                double startY = paddingTop + chartH - (actualDataValues[0] / 100.0 * chartH);
                actualFigure.StartPoint = new Point(startX, startY);

                for (int i = 0; i < actualDataValues.Length - 1; i++)
                {
                    double x1 = paddingLeft + (i / (double)(dateCount - 1) * chartW);
                    double y1 = paddingTop + chartH - (actualDataValues[i] / 100.0 * chartH);
                    double x2 = paddingLeft + ((i + 1) / (double)(dateCount - 1) * chartW);
                    double y2 = paddingTop + chartH - (actualDataValues[i + 1] / 100.0 * chartH);

                    Point cp1 = new Point(x1 + (x2 - x1) / 2, y1);
                    Point cp2 = new Point(x1 + (x2 - x1) / 2, y2);

                    actualFigure.Segments.Add(new BezierSegment(cp1, cp2, new Point(x2, y2), true));
                }
            }
            actualGeometry.Figures.Add(actualFigure);
            actualPath.Data = actualGeometry;
            ChartCanvas.Children.Add(actualPath);

            int latestIndex = actualDataValues.Length - 1;

            for (int i = 0; i < actualDataValues.Length; i++)
            {
                double x = paddingLeft + (i / (double)(dateCount - 1) * chartW);
                double y = paddingTop + chartH - (actualDataValues[i] / 100.0 * chartH);

                bool isLatest = (i == latestIndex);

                Ellipse dot = new Ellipse
                {
                    Width = isLatest ? 12 : 8,
                    Height = isLatest ? 12 : 8,
                    Fill = isLatest ? new SolidColorBrush(Color.FromRgb(243, 115, 33)) : (Application.Current.Resources["HeaderBackgroundBrush"] as Brush ?? Brushes.White),
                    Stroke = new SolidColorBrush(Color.FromRgb(243, 115, 33)),
                    StrokeThickness = 2,
                    Cursor = Cursors.Hand
                };

                double currentX = x;
                double currentY = y;
                double dotRadius = dot.Width / 2.0;
                double val = actualDataValues[i];

                dot.MouseDown += (s, ev) =>
                {
                    if (PointTooltip != null && TooltipText != null)
                    {
                        TooltipText.Text = $"{val:F1}%";
                        PointTooltip.Visibility = Visibility.Visible;
                        PointTooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        double tooltipWidth = PointTooltip.DesiredSize.Width;
                        double targetLeft = currentX - (tooltipWidth / 2.0);
                        double targetTop = currentY + dotRadius + 4;

                        Canvas.SetLeft(PointTooltip, Math.Max(0, targetLeft));
                        Canvas.SetTop(PointTooltip, targetTop);
                    }
                    ev.Handled = true;
                };

                Canvas.SetLeft(dot, x - dotRadius);
                Canvas.SetTop(dot, y - dotRadius);
                ChartCanvas.Children.Add(dot);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}