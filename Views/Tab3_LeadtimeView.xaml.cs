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
using LiveCharts;
using LiveCharts.Wpf;

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
        // 2. 3행 좌측 3개 독립 도넛 차트 바인딩 프로퍼티
        // -----------------------------------------------------------
        private SeriesCollection _orderGaugeSeries;
        public SeriesCollection OrderGaugeSeries { get => _orderGaugeSeries; set { _orderGaugeSeries = value; OnPropertyChanged(); } }

        private SeriesCollection _inspectGaugeSeries;
        public SeriesCollection InspectGaugeSeries { get => _inspectGaugeSeries; set { _inspectGaugeSeries = value; OnPropertyChanged(); } }

        private SeriesCollection _receiveGaugeSeries;
        public SeriesCollection ReceiveGaugeSeries { get => _receiveGaugeSeries; set { _receiveGaugeSeries = value; OnPropertyChanged(); } }

        private string _orderPercentText = "0%";
        public string OrderPercentText { get => _orderPercentText; set { _orderPercentText = value; OnPropertyChanged(); } }
        private string _orderSubText = "0건 / 776건";
        public string OrderSubText { get => _orderSubText; set { _orderSubText = value; OnPropertyChanged(); } }
        private Brush _orderColor = Brushes.Gray;
        public Brush OrderColor { get => _orderColor; set { _orderColor = value; OnPropertyChanged(); } }

        private string _inspectPercentText = "0%";
        public string InspectPercentText { get => _inspectPercentText; set { _inspectPercentText = value; OnPropertyChanged(); } }
        private string _inspectSubText = "0건 / 776건";
        public string InspectSubText { get => _inspectSubText; set { _inspectSubText = value; OnPropertyChanged(); } }
        private Brush _inspectColor = Brushes.Gray;
        public Brush InspectColor { get => _inspectColor; set { _inspectColor = value; OnPropertyChanged(); } }

        private string _receivePercentText = "0%";
        public string ReceivePercentText { get => _receivePercentText; set { _receivePercentText = value; OnPropertyChanged(); } }
        private string _receiveSubText = "0건 / 776건";
        public string ReceiveSubText { get => _receiveSubText; set { _receiveSubText = value; OnPropertyChanged(); } }
        private Brush _receiveColor = Brushes.Gray;
        public Brush ReceiveColor { get => _receiveColor; set { _receiveColor = value; OnPropertyChanged(); } }

        // -----------------------------------------------------------
        // 3. 2행 납기 현황 차트용 데이터 및 설정
        // -----------------------------------------------------------
        private string[] currentChartLabels = Array.Empty<string>();
        private double[] currentChartTargets = Array.Empty<double>();
        private double[] currentChartActuals = Array.Empty<double>();

        // 체크박스 상태 변경 시 차트 실시간 갱신 이벤트
        private void RiskItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            var currentItems = GetFilteredLeadtimeList(SelectedShipNo);
            var selectedItems = currentItems.Where(x => x.IsSelected).ToList();

            // 모든 체크가 해제된 경우 경고창을 띄우고 직전 상태로 되돌림
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("최소 1개 이상의 기자재를 선택해야 합니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);

                // 방금 해제한 체크박스를 다시 체크 상태로 복구
                if (sender is CheckBox cb && cb.DataContext is LeadtimeItem item)
                {
                    item.IsSelected = true;
                }
                return;
            }

            UpdateChartDataForShip(SelectedShipNo);
        }

        private void UpdateChartDataForShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            var currentItems = GetFilteredLeadtimeList(shipNo);
            var selectedItems = currentItems.Where(x => x.IsSelected).ToList();

            // 방어 코드: 만약 선택된 항목이 없다면 차트를 갱신하지 않음
            if (selectedItems.Count == 0)
            {
                return;
            }

            currentChartLabels = selectedItems.Select(x => x.EquipmentName).ToArray();

            // 임시 목표/현재입고 수치 부여
            currentChartTargets = selectedItems.Select(x => (double)(x.RemainingDays * 30 + 200)).ToArray();
            currentChartActuals = selectedItems.Select(x => (double)(x.RemainingDays * 20 + 100)).ToArray();

            DrawLeadtimeChart();
        }

        // -----------------------------------------------------------
        // 4. 호선별 3개 도넛 차트 데이터 및 색상 동적 갱신 로직
        // -----------------------------------------------------------
        private void UpdateSupplyChartData(string shipNo)
        {
            double total = 776.0;
            double orderVal = 0, inspectVal = 0, receiveVal = 0;

            if (shipNo == "H120")
            {
                orderVal = 750;   // 96.6%
                inspectVal = 692; // 89.2%
                receiveVal = 685; // 88.3%
            }
            else if (shipNo == "H121")
            {
                orderVal = 702;   // 90.5%
                inspectVal = 520; // 67.0%
                receiveVal = 458; // 59.0%
            }
            else if (shipNo == "H122")
            {
                orderVal = 621;   // 80.0%
                inspectVal = 342; // 44.1%
                receiveVal = 241; // 31.1%
            }

            double orderPct = (orderVal / total) * 100.0;
            double inspectPct = (inspectVal / total) * 100.0;
            double receivePct = (receiveVal / total) * 100.0;

            OrderPercentText = $"{orderPct:F1}%";
            OrderSubText = $"{orderVal}건";
            OrderColor = GetStatusBrush(orderPct);

            InspectPercentText = $"{inspectPct:F1}%";
            InspectSubText = $"{inspectVal}건";
            InspectColor = GetStatusBrush(inspectPct);

            ReceivePercentText = $"{receivePct:F1}%";
            ReceiveSubText = $"{receiveVal}건";
            ReceiveColor = GetStatusBrush(receivePct);

            Brush bgBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0") ?? Brushes.LightGray);

            OrderGaugeSeries = new SeriesCollection
            {
                new PieSeries { Values = new ChartValues<double> { orderVal }, Fill = OrderColor, DataLabels = false, StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { Math.Max(0, total - orderVal) }, Fill = bgBrush, DataLabels = false, StrokeThickness = 0 }
            };

            InspectGaugeSeries = new SeriesCollection
            {
                new PieSeries { Values = new ChartValues<double> { inspectVal }, Fill = InspectColor, DataLabels = false, StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { Math.Max(0, total - inspectVal) }, Fill = bgBrush, DataLabels = false, StrokeThickness = 0 }
            };

            ReceiveGaugeSeries = new SeriesCollection
            {
                new PieSeries { Values = new ChartValues<double> { receiveVal }, Fill = ReceiveColor, DataLabels = false, StrokeThickness = 0 },
                new PieSeries { Values = new ChartValues<double> { Math.Max(0, total - receiveVal) }, Fill = bgBrush, DataLabels = false, StrokeThickness = 0 }
            };
        }

        private Brush GetStatusBrush(double percent)
        {
            if (percent >= 80.0)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80));   // 초록색 (#4CAF50)
            else if (percent >= 50.0)
                return new SolidColorBrush(Color.FromRgb(234, 179, 8));   // 머스타드색 (#EAB308)
            else
                return new SolidColorBrush(Color.FromRgb(229, 57, 53));   // 빨간색 (#E53935)
        }

        // -----------------------------------------------------------
        // 5. 3행 하단 좌측 표 모델 및 원본 리스트 / 필터 상태 변수
        // -----------------------------------------------------------
        private List<LeadtimeItem> _allLeadtimeList;
        private string _currentCategoryFilter = "전체";
        private string _currentVendorFilter = "전체";

        public class LeadtimeItem : INotifyPropertyChanged
        {
            private bool _isSelected = true; // 기본 체크 상태
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

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

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // -----------------------------------------------------------
        // 6. 3행 하단 우측 알림 리스트 모델 및 원본 리스트
        // -----------------------------------------------------------
        private List<NotificationItem> _allNotificationList;
        private string _currentNotificationFilter = "전체";

        public class NotificationItem
        {
            public string Series { get; set; }
            public string ShipNo { get; set; }
            public string Category { get; set; }
            public string Issuer { get; set; }
            public string Vendor { get; set; }
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
        // 7. 생성자 및 이벤트 핸들러
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
            LoadLeadtimeTableData();
            LoadNotificationData();
            UpdateChartDataForShip(SelectedShipNo);
            UpdateSupplyChartData(SelectedShipNo);
        }

        private void LeadtimeChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawLeadtimeChart();
        }

        // -----------------------------------------------------------
        // 8. 상단 호선 선택 카드 클릭 이벤트
        // -----------------------------------------------------------
        private void ShipCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is string shipNo)
            {
                SelectedShipNo = shipNo;
                SelectedSeriesName = shipNo == "H122" ? "A SERIES" : "A SERIES";

                // 호선 변경 시 필터 초기화
                _currentCategoryFilter = "전체";
                _currentVendorFilter = "전체";

                FilterDataByShip(shipNo);
                FilterNotifications(shipNo, _currentNotificationFilter);
                UpdateChartDataForShip(shipNo);
                UpdateSupplyChartData(shipNo);
            }
        }

        private void FilterDataByShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            var filteredList = GetFilteredLeadtimeList(shipNo);

            if (LeadtimeRiskDataGrid != null)
            {
                LeadtimeRiskDataGrid.ItemsSource = filteredList;
            }
        }

        private List<LeadtimeItem> GetFilteredLeadtimeList(string shipNo)
        {
            var query = _allLeadtimeList.Where(x => x.ShipNo == shipNo);

            if (_currentCategoryFilter != "전체")
            {
                query = query.Where(x => x.Category == _currentCategoryFilter);
            }
            if (_currentVendorFilter != "전체")
            {
                query = query.Where(x => x.Vendor == _currentVendorFilter);
            }

            return query.ToList();
        }

        // -----------------------------------------------------------
        // [구분] 및 [협력사] 컬럼 필터 버튼 클릭 및 동적 메뉴 생성 이벤트
        // -----------------------------------------------------------
        private void CategoryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu menu = new ContextMenu();
                var shipItems = _allLeadtimeList.Where(x => x.ShipNo == SelectedShipNo).ToList();
                var distinctCategories = shipItems.Select(x => x.Category).Distinct().OrderBy(x => x).ToList();

                MenuItem allItem = new MenuItem { Header = "전체 보기", Tag = "전체" };
                allItem.Click += (s, ev) => { _currentCategoryFilter = "전체"; RefreshGridAndChart(); };
                menu.Items.Add(allItem);

                foreach (var cat in distinctCategories)
                {
                    MenuItem item = new MenuItem { Header = cat, Tag = cat };
                    item.Click += (s, ev) => { _currentCategoryFilter = cat; RefreshGridAndChart(); };
                    menu.Items.Add(item);
                }

                btn.ContextMenu = menu;
                menu.PlacementTarget = btn;
                menu.IsOpen = true;
            }
        }

        private void VendorFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu menu = new ContextMenu();
                var shipItems = _allLeadtimeList.Where(x => x.ShipNo == SelectedShipNo).ToList();
                var distinctVendors = shipItems.Select(x => x.Vendor).Distinct().OrderBy(x => x).ToList();

                MenuItem allItem = new MenuItem { Header = "전체 보기", Tag = "전체" };
                allItem.Click += (s, ev) => { _currentVendorFilter = "전체"; RefreshGridAndChart(); };
                menu.Items.Add(allItem);

                foreach (var vendor in distinctVendors)
                {
                    MenuItem item = new MenuItem { Header = vendor, Tag = vendor };
                    item.Click += (s, ev) => { _currentVendorFilter = vendor; RefreshGridAndChart(); };
                    menu.Items.Add(item);
                }

                btn.ContextMenu = menu;
                menu.PlacementTarget = btn;
                menu.IsOpen = true;
            }
        }

        private void RefreshGridAndChart()
        {
            FilterDataByShip(SelectedShipNo);
            UpdateChartDataForShip(SelectedShipNo);
        }

        private void NotificationFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string filterType)
            {
                _currentNotificationFilter = filterType;
                FilterNotifications(SelectedShipNo, filterType);
            }
        }

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
        // 9. 2행 중단 차트 드로잉 로직
        // -----------------------------------------------------------
        private void DrawLeadtimeChart()
        {
            if (LeadtimeChartCanvas == null || LeadtimeChartCanvas.ActualWidth <= 0 || LeadtimeChartCanvas.ActualHeight <= 0) return;
            if (currentChartLabels == null || currentChartLabels.Length == 0) return;

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
            double paddingBottom = 50;

            double chartW = width - paddingLeft - paddingRight;
            double chartH = height - paddingTop - paddingBottom;

            Brush textBrush = Application.Current.Resources["PrimaryTextBrush"] as Brush ?? Brushes.Black;

            double maxBarValue = 600;
            double maxGapValue = 300;

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

            int count = currentChartLabels.Length;
            double slotWidth = chartW / Math.Max(1, count);

            Polyline gapPolyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                StrokeThickness = 2
            };

            for (int i = 0; i < count; i++)
            {
                double slotX = paddingLeft + (i * slotWidth);
                double centerX = slotX + (slotWidth / 2.0);

                TextBlock xLabel = new TextBlock
                {
                    Text = currentChartLabels[i],
                    FontSize = 10,
                    Foreground = textBrush,
                    FontWeight = FontWeights.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = slotWidth - 4
                };
                xLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(xLabel, centerX - (xLabel.DesiredSize.Width / 2.0));
                Canvas.SetTop(xLabel, height - paddingBottom + 8);
                LeadtimeChartCanvas.Children.Add(xLabel);

                double barWidth = Math.Min(slotWidth * 0.45, 35);

                double tVal = currentChartTargets[i];      // 목표 (전체 누적 높이 기준)
                double aVal = currentChartActuals[i];      // 현재 입고 (하단 누적)
                double gapVal = Math.Max(0, tVal - aVal);  // 잔여 필요량 (상단 누적)

                // 1. 하단: 현재 입고 막대 (초록색)
                double aHeight = (aVal / maxBarValue) * chartH;
                double aY = paddingTop + chartH - aHeight;

                Rectangle actualBar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, aHeight),
                    Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80))
                };
                Canvas.SetLeft(actualBar, centerX - (barWidth / 2.0));
                Canvas.SetTop(actualBar, aY);
                LeadtimeChartCanvas.Children.Add(actualBar);

                // 2. 상단: 목표(잔여분) 막대 (#E0E0E0 회색 적용)
                double gapHeight = (gapVal / maxBarValue) * chartH;
                double gapY = aY - gapHeight;

                Rectangle targetRemainingBar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, gapHeight),
                    Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0") ?? new SolidColorBrush(Colors.LightGray)),
                    Cursor = Cursors.Hand
                };
                Canvas.SetLeft(targetRemainingBar, centerX - (barWidth / 2.0));
                Canvas.SetTop(targetRemainingBar, gapY);
                LeadtimeChartCanvas.Children.Add(targetRemainingBar);

                // 3. 잔여 필요량 꺾은선(Polyline) 포인트 추가를 위한 위치 계산
                double lineY = paddingTop + chartH - ((tVal - aVal) / maxGapValue * chartH);
                gapPolyline.Points.Add(new Point(centerX, lineY));

                // 4. 잔여 필요량 수치 텍스트 표시
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

                // 5. 꺾은선 위 클릭 가능한 포인트(Dot) 및 툴팁 이벤트
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
                double currentY = lineY;

                dot.MouseDown += (s, ev) =>
                {
                    if (LeadtimeTooltip != null && LeadtimeTooltipText != null)
                    {
                        LeadtimeTooltipText.Text = $"{currentChartLabels[i]} 잔여 필요량: {gapVal:F0} (목표: {tVal}, 현재입고: {aVal})";
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
                Canvas.SetTop(dot, lineY - 4);
                LeadtimeChartCanvas.Children.Add(dot);
            }
            LeadtimeChartCanvas.Children.Add(gapPolyline);
        }

        // -----------------------------------------------------------
        // 10. 3행 하단 좌측 표 데이터 로드 로직
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

            FilterDataByShip(SelectedShipNo);
        }

        // -----------------------------------------------------------
        // 11. 3행 하단 우측 알림 리스트 데이터 로드 로직
        // -----------------------------------------------------------
        private void LoadNotificationData()
        {
            _allNotificationList = new List<NotificationItem>
            {
                new NotificationItem("A SERIES", "H120", "지연", "생산팀", "(주)엔케이밸브", "메인엔진 패키지", "설치 일정을 고려하여 현장 반입 조기화 요청", "2026-08-02 14:30"),
                new NotificationItem("A SERIES", "H120", "지연", "협력사", "(주)엔케이밸브", "고압 LNG 밸브", "원자재 수급 차질로 인해 납기 4일 연기 요청", "2026-08-02 11:15"),
                new NotificationItem("A SERIES", "H121", "지연", "협력사", "삼영기계", "BWTS 펌프", "주요 주물 부품 선적 지연으로 7일 지연 발생 통보", "2026-08-01 17:40"),
                new NotificationItem("A SERIES", "H121", "보류/대기", "생산팀", "KCC글로벌", "스위치보드 배전반", "공장 FAT(공장 수락 검사) 일정 조율 및 서류 검사 가속 요청", "2026-08-01 09:20"),
                new NotificationItem("A SERIES", "H121", "지연", "협력사", "KCC글로벌", "특수 방오도료", "도료 배합 승인 지연으로 인한 공급 소폭 지연 안내", "2026-07-31 16:50"),
                new NotificationItem("B SERIES", "H122", "보류/대기", "생산팀", "KCC글로벌", "거주구 판넬 자재", "선실 의장 작업 구역 선행을 위한 자재 투입 앞당김 요청", "2026-07-31 10:05"),
                new NotificationItem("B SERIES", "H122", "지연", "협력사", "비상발전기코리아", "비상발전기(Emergency Gen) 테스트", "부하 시험 중 일정 조정 안내", "2026-07-30 15:25"),
                new NotificationItem("B SERIES", "H122", "보류/대기", "협력사", "태광유압", "유압 파이프 자재", "현장 수령 물량 중 일부 규격 변경에 따른 확인 요청", "2026-07-30 08:45")
            };

            FilterNotifications(SelectedShipNo, _currentNotificationFilter);
        }

        // -----------------------------------------------------------
        // 12. INotifyPropertyChanged 구현
        // -----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}