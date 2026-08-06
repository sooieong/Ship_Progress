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

namespace Ship_Progress.Views
{
    public partial class Tab3_LeadtimeView : UserControl, INotifyPropertyChanged
    {
        // -----------------------------------------------------------
        // 1. 1행 KPI 동적 바인딩 프로퍼티
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

        private double _onTimeRate = 90;
        public double OnTimeRate
        {
            get => _onTimeRate;
            set { _onTimeRate = value; OnPropertyChanged(); }
        }

        private int _avgLeadTime = 7;
        public int AvgLeadTime
        {
            get => _avgLeadTime;
            set { _avgLeadTime = value; OnPropertyChanged(); }
        }

        private int _riskCount = 3;
        public int RiskCount
        {
            get => _riskCount;
            set { _riskCount = value; OnPropertyChanged(); }
        }

        // -----------------------------------------------------------
        // 2. 2행 납기 현황 차트용 데이터 및 설정
        // -----------------------------------------------------------
        private readonly string[] projectLabels = { "Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026", "May 2026", "Jun 2026" };
        private readonly double[] targetBarValues = { 206, 319, 211, 416, 511, 398 };
        private readonly double[] actualBarValues = { 40, 25, 204, 186, 492, 160 };

        // -----------------------------------------------------------
        // 3. 3행 하단 좌측 표 모델 및 원본 리스트
        // -----------------------------------------------------------
        private List<LeadtimeItem> _allLeadtimeList;

        public class LeadtimeItem
        {
            public string Series { get; set; }
            public string ShipNo { get; set; }
            public string Category { get; set; }
            public string EquipmentName { get; set; }
            public string Vendor { get; set; }
            public string DeliveryDate { get; set; }
            public int RemainingDays { get; set; }
            public string RemainingDaysText => $"{RemainingDays}일";
            public string Status { get; set; }

            public Brush StatusBgColor => Status switch
            {
                "위험" => new SolidColorBrush(Color.FromArgb(40, 229, 57, 53)),
                "주의" => new SolidColorBrush(Color.FromArgb(40, 251, 140, 0)),
                _ => new SolidColorBrush(Color.FromArgb(40, 67, 160, 71))
            };

            public Brush StatusFgColor => Status switch
            {
                "위험" => new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                "주의" => new SolidColorBrush(Color.FromRgb(251, 140, 0)),
                _ => new SolidColorBrush(Color.FromRgb(67, 160, 71))
            };

            public LeadtimeItem(string series, string shipNo, string category, string equipmentName, string vendor, string deliveryDate, int remainingDays, string status)
            {
                Series = series;
                ShipNo = shipNo;
                Category = category;
                EquipmentName = equipmentName;
                Vendor = vendor;
                DeliveryDate = deliveryDate;
                RemainingDays = remainingDays;
                Status = status;
            }
        }

        // -----------------------------------------------------------
        // 4. 3행 하단 우측 알림 리스트 모델 및 원본 리스트
        // -----------------------------------------------------------
        private List<NotificationItem> _allNotificationList;
        private string _currentNotificationFilter = "전체"; // 현재 선택된 알림 탭 (전체/지연/결품) 유지용

        public class NotificationItem
        {
            public string Series { get; set; }
            public string ShipNo { get; set; }
            public string Category { get; set; } // "지연" 또는 "결품"
            public string Issuer { get; set; }   // 발신주체
            public string Vendor { get; set; }   // 협력사명
            public string EquipmentName { get; set; }
            public string NoticeMessage { get; set; }
            public string RegisterDate { get; set; }

            public string IssuerText => $"발신: {Issuer} ({Vendor})";

            public Brush CategoryBgBrush => Category == "지연"
                ? new SolidColorBrush(Color.FromArgb(40, 229, 57, 53))
                : new SolidColorBrush(Color.FromArgb(40, 251, 140, 0));

            public Brush CategoryFgBrush => Category == "지연"
                ? new SolidColorBrush(Color.FromRgb(229, 57, 53))
                : new SolidColorBrush(Color.FromRgb(251, 140, 0));

            public NotificationItem(string series, string shipNo, string category, string issuer, string vendor, string equipmentName, string noticeMessage, string registerDate)
            {
                Series = series;
                ShipNo = shipNo;
                Category = category;
                Issuer = issuer;
                Vendor = vendor;
                EquipmentName = equipmentName;
                NoticeMessage = noticeMessage;
                RegisterDate = registerDate;
            }
        }

        // -----------------------------------------------------------
        // 5. 생성자 및 이벤트 핸들러
        // -----------------------------------------------------------
        public Tab3_LeadtimeView()
        {
            InitializeComponent();
            this.DataContext = this;

            LeadtimeChartCanvas.MouseDown += (s, e) =>
            {
                if (LeadtimeTooltip != null) LeadtimeTooltip.Visibility = Visibility.Collapsed;
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawLeadtimeChart();
            LoadLeadtimeTableData();
            LoadNotificationData();
        }

        private void LeadtimeChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawLeadtimeChart();
        }

        // -----------------------------------------------------------
        // 6. 상단 호선 선택 카드 클릭 이벤트 (좌측 표 + 우측 알림 리스트 동시 필터링)
        // -----------------------------------------------------------
        private void ShipCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is string shipNo)
            {
                SelectedShipNo = shipNo;
                SelectedSeriesName = shipNo == "H122" ? "B SERIES" : "A SERIES";

                // 1. 좌측 표 데이터 필터링
                FilterDataByShip(shipNo);

                // 2. 우측 알림 리스트 데이터 필터링 (호선 + 현재 선택된 알림 탭 조건 유지)
                FilterNotifications(shipNo, _currentNotificationFilter);
            }
        }

        private void FilterDataByShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            var filteredList = _allLeadtimeList.Where(x => x.ShipNo == shipNo).ToList();

            if (LeadtimeRiskDataGrid != null)
            {
                LeadtimeRiskDataGrid.ItemsSource = filteredList;
            }
        }

        // -----------------------------------------------------------
        // 7. 우측 상단 결품/지연 알림 라디오버튼 필터링 이벤트 핸들러
        // -----------------------------------------------------------
        private void NotificationFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string filterType)
            {
                _currentNotificationFilter = filterType;
                // 현재 선택되어 있는 호선(SelectedShipNo)과 탭 필터 조건 결합하여 갱신
                FilterNotifications(SelectedShipNo, filterType);
            }
        }

        // 호선과 알림 종류(전체/지연/결품)를 모두 고려하여 우측 리스트를 갱신하는 통합 메서드
        private void FilterNotifications(string shipNo, string categoryFilter)
        {
            if (_allNotificationList == null || NotificationListView == null) return;

            var query = _allNotificationList.Where(x => x.ShipNo == shipNo);

            if (categoryFilter != "전체")
            {
                query = query.Where(x => x.Category == categoryFilter);
            }

            NotificationListView.ItemsSource = query.ToList();
        }

        // -----------------------------------------------------------
        // 8. 2행 중단 차트 드로잉 로직 (보조축 및 수치 레이블 적용)
        // -----------------------------------------------------------
        private void DrawLeadtimeChart()
        {
            if (LeadtimeChartCanvas == null || LeadtimeChartCanvas.ActualWidth <= 0 || LeadtimeChartCanvas.ActualHeight <= 0) return;

            var elementsToRemove = LeadtimeChartCanvas.Children.Cast<UIElement>()
                .Where(e => e != LeadtimeTooltip)
                .ToList();

            foreach (var elem in elementsToRemove)
            {
                LeadtimeChartCanvas.Children.Remove(elem);
            }

            double width = LeadtimeChartCanvas.ActualWidth;
            double height = LeadtimeChartCanvas.ActualHeight;

            double paddingLeft = 50;
            double paddingRight = 60;
            double paddingTop = 25;
            double paddingBottom = 40;

            double chartW = width - paddingLeft - paddingRight;
            double chartH = height - paddingTop - paddingBottom;

            Brush textBrush = Application.Current.Resources["PrimaryTextBrush"] as Brush ?? Brushes.Black;

            double maxBarValue = 600;
            double maxGapValue = 300;

            // 1. 좌측 주축 Y축 격자 및 레이블
            for (int i = 0; i <= 4; i++)
            {
                double yVal = i * 150;
                double yPos = paddingTop + chartH - (yVal / maxBarValue * chartH);

                Line gridLine = new Line
                {
                    X1 = paddingLeft,
                    Y1 = yPos,
                    X2 = width - paddingRight,
                    Y2 = yPos,
                    Stroke = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    StrokeThickness = 0.5,
                    Opacity = 0.5
                };
                LeadtimeChartCanvas.Children.Add(gridLine);

                TextBlock yLabel = new TextBlock { Text = $"{yVal}", FontSize = 10, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
                Canvas.SetLeft(yLabel, 10);
                Canvas.SetTop(yLabel, yPos - 7);
                LeadtimeChartCanvas.Children.Add(yLabel);
            }

            // 2. 우측 보조축 Y축 레이블 (잔여 필요량 Gap용)
            for (int i = 0; i <= 4; i++)
            {
                double gVal = i * 75;
                double yPos = paddingTop + chartH - (gVal / maxGapValue * chartH);

                TextBlock rightYLabel = new TextBlock
                {
                    Text = $"{gVal}",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(rightYLabel, width - paddingRight + 8);
                Canvas.SetTop(rightYLabel, yPos - 7);
                LeadtimeChartCanvas.Children.Add(rightYLabel);
            }

            int count = projectLabels.Length;
            double slotWidth = chartW / count;

            Polyline gapPolyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                StrokeThickness = 2
            };

            for (int i = 0; i < count; i++)
            {
                double slotX = paddingLeft + (i * slotWidth);
                double centerX = slotX + (slotWidth / 2.0);

                TextBlock xLabel = new TextBlock { Text = projectLabels[i], FontSize = 10, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
                Canvas.SetLeft(xLabel, centerX - 18);
                Canvas.SetTop(xLabel, height - paddingBottom + 8);
                LeadtimeChartCanvas.Children.Add(xLabel);

                double barWidth = slotWidth * 0.32;

                // Target (전체 필요) 막대 정의
                double tVal = targetBarValues[i];
                double tHeight = (tVal / maxBarValue) * chartH;
                double tY = paddingTop + chartH - tHeight;

                Rectangle targetBar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, tHeight),
                    Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94)) // 초록색 단색 (#22C55E)
                };
                Canvas.SetLeft(targetBar, centerX - barWidth - 2);
                Canvas.SetTop(targetBar, tY);
                LeadtimeChartCanvas.Children.Add(targetBar);

                // 🎯 [이 부분의 aHeight, aY 변수 정의가 빠져있었1습니다]
                double aVal = actualBarValues[i];
                double aHeight = (aVal / maxBarValue) * chartH;
                double aY = paddingTop + chartH - aHeight;

                // Actual (현재 입고) 막대 정의
                Rectangle actualBar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, aHeight),
                    Fill = new SolidColorBrush(Color.FromRgb(251, 140, 0)) // 주황색 단색 (#FB8C00)
                };
                Canvas.SetLeft(actualBar, centerX + 2);
                Canvas.SetTop(actualBar, aY);
                LeadtimeChartCanvas.Children.Add(actualBar);

                double gapVal = Math.Max(0, tVal - aVal);
                double gapHeight = (gapVal / maxGapValue) * chartH;
                double gapY = paddingTop + chartH - gapHeight;

                gapPolyline.Points.Add(new Point(centerX, gapY));

                Ellipse dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Application.Current.Resources["HeaderBackgroundBrush"] as Brush ?? Brushes.White,
                    Stroke = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    StrokeThickness = 2,
                    Cursor = Cursors.Hand
                };

                double currentX = centerX;
                double currentY = gapY;

                TextBlock valueLabel = new TextBlock
                {
                    Text = $"{gapVal:F0}",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    FontWeight = FontWeights.Bold
                };
                valueLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(valueLabel, centerX - (valueLabel.DesiredSize.Width / 2.0));
                Canvas.SetTop(valueLabel, gapY - 16);
                LeadtimeChartCanvas.Children.Add(valueLabel);

                dot.MouseDown += (s, ev) =>
                {
                    if (LeadtimeTooltip != null && LeadtimeTooltipText != null)
                    {
                        LeadtimeTooltipText.Text = $"{projectLabels[i]} 잔여 필요량: {gapVal:F0} (Target: {tVal}, Actual: {aVal})";
                        LeadtimeTooltip.Visibility = Visibility.Visible;
                        LeadtimeTooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        double tooltipWidth = LeadtimeTooltip.DesiredSize.Width;
                        double targetLeft = currentX - (tooltipWidth / 2.0);
                        double targetTop = currentY - 32;

                        Canvas.SetLeft(LeadtimeTooltip, Math.Max(0, targetLeft));
                        Canvas.SetTop(LeadtimeTooltip, targetTop);
                    }
                    ev.Handled = true;
                };

                Canvas.SetLeft(dot, centerX - 4);
                Canvas.SetTop(dot, gapY - 4);
                LeadtimeChartCanvas.Children.Add(dot);
            }

            LeadtimeChartCanvas.Children.Add(gapPolyline);
        }

        // -----------------------------------------------------------
        // 9. 3행 하단 좌측 표 데이터 로드 로직
        // -----------------------------------------------------------
        private void LoadLeadtimeTableData()
        {
            _allLeadtimeList = new List<LeadtimeItem>
            {
                new LeadtimeItem("A SERIES", "H120", "의장", "메인엔진 패키지", "(주)엔케이밸브", "2026-03-10", 5, "주의"),
                new LeadtimeItem("A SERIES", "H120", "배관", "고압 LNG 밸브", "(주)엔케이밸브", "2026-03-05", 1, "위험"),
                new LeadtimeItem("A SERIES", "H121", "기관", "BWTS 펌프", "삼영기계", "2026-03-15", 10, "정상"),
                new LeadtimeItem("A SERIES", "H121", "전기", "스위치보드 배전반", "KCC글로벌", "2026-03-12", 8, "정상"),
                new LeadtimeItem("A SERIES", "H121", "도장", "특수 방오도료", "KCC글로벌", "2026-03-08", 3, "주의"),
                new LeadtimeItem("B SERIES", "H122", "의장", "거주구 판넬 자재", "KCC글로벌", "2026-03-20", 15, "정상"),
                new LeadtimeItem("B SERIES", "H122", "기관", "비상발전기 세트", "비상발전기코리아", "2026-03-04", 0, "위험"),
                new LeadtimeItem("B SERIES", "H122", "배관", "유압 파이프 자재", "태광유압", "2026-03-14", 9, "정상")
            };

            // 기본 선택된 "H120" 기준으로 필터링 적용
            FilterDataByShip(SelectedShipNo);
        }

        // -----------------------------------------------------------
        // 10. 3행 하단 우측 알림 리스트 데이터 로드 로직
        // -----------------------------------------------------------
        private void LoadNotificationData()
        {
            _allNotificationList = new List<NotificationItem>
            {
                new NotificationItem("A SERIES", "H120", "지연", "생산팀", "(주)엔케이밸브", "메인엔진 패키지", "설치 일정을 고려하여 현장 반입 조기화 요청", "2026-08-02 14:30"),
                new NotificationItem("A SERIES", "H120", "지연", "협력사", "(주)엔케이밸브", "고압 LNG 밸브", "원자재 수급 차질로 인해 납기 4일 연기 요청", "2026-08-02 11:15"),
                new NotificationItem("A SERIES", "H121", "지연", "협력사", "삼영기계", "BWTS 펌프", "주요 주물 부품 선적 지연으로 7일 지연 발생 통보", "2026-08-01 17:40"),
                new NotificationItem("A SERIES", "H121", "결품", "생산팀", "KCC글로벌", "스위치보드 배전반", "공장 FAT(공장 수락 검사) 일정 조율 및 서류 검사 가속 요청", "2026-08-01 09:20"),
                new NotificationItem("A SERIES", "H121", "지연", "협력사", "KCC글로벌", "특수 방오도료", "도료 배합 승인 지연으로 인한 공급 소폭 지연 안내", "2026-07-31 16:50"),
                new NotificationItem("B SERIES", "H122", "결품", "생산팀", "KCC글로벌", "거주구 판넬 자재", "선실 의장 작업 구역 선행을 위한 자재 투입 앞당김 요청", "2026-07-31 10:05"),
                new NotificationItem("B SERIES", "H122", "지연", "협력사", "비상발전기코리아", "비상발전기(Emergency Gen) 테스트", "부하 시험 중 일정 조정 안내", "2026-07-30 15:25"),
                new NotificationItem("B SERIES", "H122", "결품", "협력사", "태광유압", "유압 파이프 자재", "현장 수령 물량 중 일부 규격 변경에 따른 확인 요청", "2026-07-30 08:45")
            };

            // 기본 선택된 "H120" 및 "전체" 필터 기준으로 초기 로드
            FilterNotifications(SelectedShipNo, _currentNotificationFilter);
        }

        // -----------------------------------------------------------
        // 11. INotifyPropertyChanged 구현
        // -----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}