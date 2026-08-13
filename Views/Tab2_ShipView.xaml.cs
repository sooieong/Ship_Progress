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
    // -----------------------------------------------------------
    // [신규] 블록별 공정 현황 데이터 모델 클래스 (한국어/수치 전용)
    // -----------------------------------------------------------
    public class BlockProgressItem
    {
        public string Title { get; set; }         // 항목 명칭 (한국어 전용)
        public double Progress { get; set; }        // 진행률 수치 (0 ~ 100)

        public Brush BarColor { get; set; }         // 바 및 텍스트 색상
        public string ProgressText => $"{Progress}%";

        public BlockProgressItem(string title, double progress)
        {
            Title = title;
            Progress = progress;

            // -----------------------------------------------------------
            // if 문 조건 지정을 통한 진행 상태별 색상 설정
            // (90% 이상: 초록, 60% 이상: 주황/노랑, 60% 미만: 빨강)
            // -----------------------------------------------------------
            if (progress >= 90)
            {
                // 초록색 (Green)
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            }
            else if (progress >= 60)
            {
                // 주황색 (Orange/Yellow)
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAB308"));
            }
            else
            {
                // 빨간색 (Red)
                BarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
            }
        }
    }

    public partial class Tab2_ShipView : UserControl, INotifyPropertyChanged
    {
        // -----------------------------------------------------------
        // 1. S-Curve 데이터 세트 및 설정
        // -----------------------------------------------------------
        private readonly string[] dateLabels = { "2026.05", "2026.06", "2026.07", "2026.08", "2026.09", "2026.10", "2026.11" };
        private readonly double[] planData = { 15.0, 30.0, 48.0, 65.0, 80.0, 92.0, 100.0 };
        private readonly double[] actualData = { 12.0, 26.0, 42.0, 62.0 };
        private const int CurrentMonthIndex = 3;

        // -----------------------------------------------------------
        // 2. 1행 KPI 동적 바인딩 프로퍼티
        // -----------------------------------------------------------
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

        private string _expectedEndDate = "2028-08-07";
        public string ExpectedEndDate
        {
            get => _expectedEndDate;
            set { _expectedEndDate = value; OnPropertyChanged(); }
        }

        // -----------------------------------------------------------
        // 3. 3행 하단 블록별 공정 현황 바인딩 프로퍼티
        // -----------------------------------------------------------
        private List<BlockProgressItem> _blockProgressList;
        public List<BlockProgressItem> BlockProgressList
        {
            get => _blockProgressList;
            set { _blockProgressList = value; OnPropertyChanged(); }
        }

        // -----------------------------------------------------------
        // 4. H120 위험 기자재 모델 클래스
        // -----------------------------------------------------------
        public class H120RiskItem
        {
            public string Category { get; set; }
            public string EquipmentName { get; set; }
            public int DelayDays { get; set; }
            public string Status { get; set; }
            public Brush StatusBgColor { get; set; }
            public Brush StatusFgColor { get; set; }
            public string MainProcessImpact { get; set; }

            public string DelayText => $"{DelayDays}일";

            public H120RiskItem(string category, string equipmentName, int delayDays, string status, string statusBgHex, string statusFgHex, string mainProcessImpact)
            {
                Category = category;
                EquipmentName = equipmentName;
                DelayDays = delayDays;
                Status = status;

                // Hex 색상 문자열을 Brush 객체로 변환
                var converter = new BrushConverter();
                StatusBgColor = (Brush)converter.ConvertFromString(statusBgHex);
                StatusFgColor = (Brush)converter.ConvertFromString(statusFgHex);

                MainProcessImpact = mainProcessImpact;
            }
        }

        // 필터링을 위한 원본 데이터 및 필터 상태 변수 추가
        private List<H120RiskItem> _originalH120RiskList = new List<H120RiskItem>();
        private string _currentCategoryFilter = "전체";

        // -----------------------------------------------------------
        // 5. 생성자 및 이벤트 핸들러
        // -----------------------------------------------------------
        public Tab2_ShipView()
        {
            InitializeComponent();
            this.DataContext = this;

            ChartCanvas.MouseDown += (s, e) =>
            {
                PointTooltip.Visibility = Visibility.Collapsed;
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

        // 상단 호선 선택 카드 클릭 이벤트 추가
        private void ShipCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is string shipNo)
            {
                SelectedShipNo = shipNo;
                SelectedSeriesName = shipNo == "H122" ? "B SERIES" : "A SERIES";
                LoadShipDashboardData(shipNo);
            }
        }

        // -----------------------------------------------------------
        // 6. 데이터 로드 및 차트 생성 로직
        // -----------------------------------------------------------
        public void LoadShipDashboardData(string shipNo)
        {
            SelectedShipNo = shipNo;
            SelectedSeriesName = shipNo == "H122" ? "B SERIES" : "A SERIES";
            ExpectedEndDate = shipNo == "H122" ? "2026-12-15" : "2026-11-20";

            // S-Curve 차트 그리기
            DrawSCurveChart();

            // 영문 및 대괄호[] 모두 제거 / 순수 한국어 명칭과 수치만 세팅
            BlockProgressList = new List<BlockProgressItem>
            {
                new BlockProgressItem("기관실 대형 주기계", 100),
                new BlockProgressItem("발전기 엔진 및 보일러", 100),
                new BlockProgressItem("추진 계통", 85),
                new BlockProgressItem("갑판 대형 장비", 70),
                new BlockProgressItem("화물창", 45)
            };

            // 위험 품목 현황 데이터 (상태 및 배지 색상 포함)
            var riskItems = new List<H120RiskItem>
            {
                new H120RiskItem("의장", "메인 배전반 (MSBD)", 14, "위험", "#FFEBEE", "#D32F2F", "추진 계통"),
                new H120RiskItem("배관", "고압 밸브 블록", 4, "주의", "#FFF3E0", "#E65100", "갑판 대형 장비")
            };

            // 원본 데이터 백업 및 필터 초기화
            _originalH120RiskList = riskItems;
            _currentCategoryFilter = "전체";

            ApplyCategoryFilter();
        }

        // -----------------------------------------------------------
        // [구분] 헤더 버튼 클릭 시 동적 메뉴 생성 및 팝업
        // -----------------------------------------------------------
        private void ProcessFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu menu = new ContextMenu();

                var distinctCategories = _originalH120RiskList.Select(x => x.Category).Distinct().OrderBy(x => x).ToList();

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

        // -----------------------------------------------------------
        // 선택된 구분에 따라 DataGrid 필터링 적용
        // -----------------------------------------------------------
        private void ApplyCategoryFilter()
        {
            if (H120RiskDataGrid == null) return;

            var query = _originalH120RiskList.AsEnumerable();

            if (_currentCategoryFilter != "전체")
            {
                query = query.Where(x => x.Category == _currentCategoryFilter);
            }

            H120RiskDataGrid.ItemsSource = query.ToList();
        }

        // S-Curve 캔버스 드로잉 로직 (다크/라이트 모드 리소스 대응 적용)
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

            double[] planDataValues = { 0, 15, 35, 60, 85, 100 };
            double[] actualDataValues = { 0, 12, 32, 58, 86.4 };

            double width = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;

            double paddingLeft = 40;
            double paddingRight = 30;
            double paddingTop = 20;
            double paddingBottom = 40;

            double chartW = width - paddingLeft - paddingRight;
            double chartH = height - paddingTop - paddingBottom;

            // 앱 전역 테마 텍스트 컬러 가져오기 (다크/라이트 자동 연동)
            Brush textBrush = Application.Current.Resources["PrimaryTextBrush"] as Brush ?? Brushes.Black;
            Brush gridLineBrush = Application.Current.Resources["BorderBrush"] as Brush ?? new SolidColorBrush(Color.FromRgb(235, 235, 235));

            // 그리드 라인 및 Y축 레이블 생성
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

            // X축 레이블 생성
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

            Polyline planPolyline = new Polyline { Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)), StrokeThickness = 2 };
            Polyline actualPolyline = new Polyline { Stroke = new SolidColorBrush(Color.FromRgb(243, 115, 33)), StrokeThickness = 2.5 };

            int dateCount = (dateLabels != null && dateLabels.Length > 0) ? dateLabels.Length : planDataValues.Length;

            for (int i = 0; i < planDataValues.Length; i++)
            {
                double x = paddingLeft + (i / (double)(dateCount - 1) * chartW);
                double y = paddingTop + chartH - (planDataValues[i] / 100.0 * chartH);
                planPolyline.Points.Add(new Point(x, y));
            }

            int latestIndex = actualDataValues.Length - 1;

            for (int i = 0; i < actualDataValues.Length; i++)
            {
                double x = paddingLeft + (i / (double)(dateCount - 1) * chartW);
                double y = paddingTop + chartH - (actualDataValues[i] / 100.0 * chartH);
                actualPolyline.Points.Add(new Point(x, y));

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

            ChartCanvas.Children.Insert(0, planPolyline);
            ChartCanvas.Children.Insert(1, actualPolyline);
        }

        // -----------------------------------------------------------
        // 7. INotifyPropertyChanged 구현
        // -----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}