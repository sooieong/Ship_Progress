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
        // 3. 2행 납기 현황 차트용 데이터 캐시
        // -----------------------------------------------------------
        private string[] currentChartLabels = Array.Empty<string>();
        private double[] currentChartTargets = Array.Empty<double>();
        private double[] currentChartActuals = Array.Empty<double>();

        // 체크박스 상태 변경 시 차트 실시간 갱신 이벤트
        private void RiskItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateChartDataForShip(SelectedShipNo);
        }

        private void UpdateChartDataForShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            var currentItems = GetFilteredLeadtimeList(shipNo);

            // [변경됨] 선택(IsSelected)되었고, 상태가 '위험', '주의', '납품 예정'인 항목만 차트에 표시
            var selectedItems = currentItems
                .Where(x => x.IsSelected && (x.Status == "위험" || x.Status == "주의" || x.Status == "납품 예정"))
                .ToList();

            if (selectedItems.Count == 0)
            {
                currentChartLabels = Array.Empty<string>();
                currentChartTargets = Array.Empty<double>();
                currentChartActuals = Array.Empty<double>();
                DrawLeadtimeChart();
                return;
            }

            currentChartLabels = selectedItems.Select(x => x.EquipmentName).ToArray();
            currentChartTargets = selectedItems.Select(x => (double)x.Target).ToArray();
            currentChartActuals = selectedItems.Select(x => (double)x.CurrentStock).ToArray();

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
                orderVal = 750;
                inspectVal = 692;
                receiveVal = 685;
            }
            else if (shipNo == "H121")
            {
                orderVal = 702;
                inspectVal = 520;
                receiveVal = 458;
            }
            else if (shipNo == "H122")
            {
                orderVal = 621;
                inspectVal = 342;
                receiveVal = 241;
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
                return new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (percent >= 50.0)
                return new SolidColorBrush(Color.FromRgb(234, 179, 8));
            else
                return new SolidColorBrush(Color.FromRgb(229, 57, 53));
        }

        // -----------------------------------------------------------
        // 5. 납기 현황 DB 모델 및 데이터 리스트
        // -----------------------------------------------------------
        private List<LeadtimeItem> _allLeadtimeList;
        private string _currentCategoryFilter = "전체";
        private string _currentVendorFilter = "전체";
        private string _currentSearchKeyword = string.Empty;

        public class LeadtimeItem : INotifyPropertyChanged
        {
            private bool _isSelected = true;
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
            public int Target { get; set; }
            public int CurrentStock { get; set; }
            public int RemainingNeed => Math.Max(0, Target - CurrentStock);
            public string DeliveryDate { get; set; }
            public string RemainingDaysText { get; set; }

            public int RemainingDaysValue => int.TryParse(RemainingDaysText.Replace("일", "").Trim(), out int val) ? val : 0;

            public string Status { get; set; }

            public Brush StatusBgColor => Status switch
            {
                "위험" => new SolidColorBrush(Color.FromArgb(40, 229, 57, 53)),
                "주의" => new SolidColorBrush(Color.FromArgb(40, 251, 140, 0)),
                "납품 완료" => new SolidColorBrush(Color.FromArgb(40, 67, 160, 71)),
                _ => new SolidColorBrush(Color.FromArgb(40, 150, 150, 150))
            };

            public Brush StatusFgColor => Status switch
            {
                "위험" => new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                "주의" => new SolidColorBrush(Color.FromRgb(251, 140, 0)),
                "납품 완료" => new SolidColorBrush(Color.FromRgb(67, 160, 71)),
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };

            public LeadtimeItem(string series, string shipNo, string category, string equipmentName, string vendor, int target, int currentStock, string deliveryDate, string remainingDaysText, string status)
            {
                Series = series;
                ShipNo = shipNo;
                Category = category;
                EquipmentName = equipmentName;
                Vendor = vendor;
                Target = target;
                CurrentStock = currentStock;
                DeliveryDate = deliveryDate;
                RemainingDaysText = remainingDaysText;
                Status = status;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // -----------------------------------------------------------
        // 6. 알림 리스트 모델 및 데이터 리스트
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
        // 7. 생성자 및 초기화
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
        // 8. 호선 선택 카드 클릭 이벤트
        // -----------------------------------------------------------
        private void ShipCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is string shipNo)
            {
                SelectedShipNo = shipNo;
                SelectedSeriesName = "A SERIES";

                _currentCategoryFilter = "전체";
                _currentVendorFilter = "전체";
                _currentSearchKeyword = string.Empty;

                if (SearchTextBox != null)
                {
                    SearchTextBox.Text = string.Empty;
                }

                FilterDataByShip(shipNo);
                FilterNotifications(shipNo, _currentNotificationFilter);
                UpdateChartDataForShip(shipNo);
                UpdateSupplyChartData(shipNo);
            }
        }

        // -----------------------------------------------------------
        // 페이징 및 필터링 관련 로직
        // -----------------------------------------------------------
        private int _currentPage = 1;
        private const int _pageSize = 5; // 한 페이지당 5개 표시
        private List<LeadtimeItem> _currentFilteredList = new List<LeadtimeItem>();

        private void FilterDataByShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            // 조건에 맞는 전체 목록을 먼저 캐시
            _currentFilteredList = GetFilteredLeadtimeList(shipNo);

            // 첫 페이지로 초기화 후 페이징 적용된 데이터 바인딩
            _currentPage = 1;
            UpdatePagedDataGrid();
        }

        // 페이징 적용 및 DataGrid 갱신 메서드
        private void UpdatePagedDataGrid()
        {
            if (_currentFilteredList == null) return;

            int totalItems = _currentFilteredList.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_pageSize));

            // 현재 페이지 번호 범위 보정
            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            // 5개씩 끊어서 가져오기
            var pagedData = _currentFilteredList
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            if (LeadtimeRiskDataGrid != null)
            {
                LeadtimeRiskDataGrid.ItemsSource = pagedData;
            }

            // 페이지 정보 텍스트블록 업데이트 (예: 1 / 3)
            if (PageInfoTextBlock != null)
            {
                PageInfoTextBlock.Text = $"{_currentPage} / {totalPages}";
            }
        }

        // 이전 페이지 버튼 클릭 이벤트
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagedDataGrid();
            }
        }

        // 다음 페이지 버튼 클릭 이벤트
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling(_currentFilteredList.Count / (double)_pageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                UpdatePagedDataGrid();
            }
        }

        // 통합 필터링 메서드 (구분, 협력사, 검색어 통합 적용)
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
            if (!string.IsNullOrWhiteSpace(_currentSearchKeyword))
            {
                query = query.Where(x => x.EquipmentName != null && x.EquipmentName.Contains(_currentSearchKeyword));
            }

            return query
                .OrderByDescending(x => x.RemainingDaysValue)
                .ThenBy(x => GetStatusSortOrder(x.Status))
                .ToList();
        }

        // 실시간 검색 텍스트박스 입력 이벤트
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _currentSearchKeyword = tb.Text.Trim();
                FilterDataByShip(SelectedShipNo);
            }
        }

        // 상태별 2차 정렬 순서 부여 헬퍼 메서드
        private int GetStatusSortOrder(string status)
        {
            return status switch
            {
                "납품 예정" => 0,
                "위험" => 1,
                "주의" => 1,
                "납품 완료" => 2,
                _ => 3
            };
        }

        // -----------------------------------------------------------
        // 필터 메뉴 관리
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
        // 9. 납기 현황 차트 드로잉 (목표, 현재 입고, 잔여 필요량 기준)
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

            double maxBarValue = 400;
            double maxGapValue = 200;

            for (int i = 0; i <= 4; i++)
            {
                double yVal = i * 100;
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
                double gVal = i * 50;
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

                double barWidth = Math.Min(slotWidth * 0.45, 30);

                double tVal = currentChartTargets[i];
                double aVal = currentChartActuals[i];
                double gapVal = Math.Max(0, tVal - aVal);

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

                double lineY = paddingTop + chartH - (gapVal / maxGapValue * chartH);
                gapPolyline.Points.Add(new Point(centerX, lineY));

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

                Ellipse dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.White,
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
                        LeadtimeTooltipText.Text = $"{currentChartLabels[i]} 잔여 필요량: {gapVal} (목표: {tVal}, 현재입고: {aVal})";
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
        // 10. 납기 현황 DB 데이터 로드
        // -----------------------------------------------------------
        private void LoadLeadtimeTableData()
        {
            _allLeadtimeList = new List<LeadtimeItem>
            {
                new LeadtimeItem("A SERIES", "H120", "기관", "배관 팽창 루프 서포트", "세진중공업", 65, 10, "2026-08-13", "8일", "주의"),
                new LeadtimeItem("A SERIES", "H120", "기관", "선저 배수 플러그 소켓", "세진중공업", 70, 70, "2026-04-13", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "기관", "러그형 버터플라이 밸브", "삼진정공", 280, 280, "2026-07-26", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "기관", "갑판 드레인 스커퍼 및 배관 피팅", "성광벤드", 120, 0, "2026-09-25", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H120", "기관", "래싱 브릿지 Cross Bar", "오리엔탈정공", 320, 320, "2026-05-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "곡형 핸드 그립", "동성하이텍", 280, 280, "2026-04-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "워터 타이트 맨홀 커버", "강림중공업", 240, 240, "2026-05-27", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "아이 플레이트 (20톤 이상)", "삼우중공업", 150, 150, "2026-07-08", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "수직 사다리 (직형 A타입)", "동성하이텍", 260, 260, "2026-04-01", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "미끄럼 방지 바 (2형) 10EA", "동성하이텍", 210, 210, "2026-04-23", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "현수사다리 거치 브라켓", "강림중공업", 2, 0, "2026-08-10", "11일", "위험"),
                new LeadtimeItem("A SERIES", "H120", "선실", "거주구 창문 코밍 (사각)", "스타코 (STACO)", 52, 20, "2026-08-13", "8일", "주의"),
                new LeadtimeItem("A SERIES", "H120", "선실", "청동 소화전 각밸브", "스타코 (STACO)", 180, 180, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "A급 내화 정전기 방지 카펫", "스타코 (STACO)", 350, 350, "2026-07-12", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "주방/세탁실 미끄럼방지 타일", "스타코 (STACO)", 220, 220, "2026-07-14", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "전장 단로기 부착 브라켓", "현대일렉트릭", 260, 100, "2026-08-16", "10일", "위험"),
                new LeadtimeItem("A SERIES", "H120", "전기", "세면대/수도 배관 서포트", "대양전기공업", 50, 25, "2026-08-14", "7일", "주의"),
                new LeadtimeItem("A SERIES", "H120", "전기", "케이블 윈치 러그", "대한전선", 52, 52, "2026-05-03", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "대형 LED 투광등", "대양전기공업", 130, 130, "2026-05-07", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "비상 유도 표지등", "대양전기공업", 140, 140, "2026-05-15", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "자동 전화기 (탁상/벽걸이)", "대양전기공업", 100, 100, "2026-07-02", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "정온식/차동식 열 감지기", "한화시스템", 240, 240, "2026-07-24", "-", "납품 완료"),

                new LeadtimeItem("A SERIES", "H121", "기관", "BWTS 펌프 메인", "삼영기계", 150, 80, "2026-08-20", "15일", "주의"),
                new LeadtimeItem("A SERIES", "H121", "전기", "스위치보드 배전반", "KCC글로벌", 200, 190, "2026-08-10", "5일", "정상"),
                new LeadtimeItem("B SERIES", "H122", "기관", "비상발전기 세트", "비상발전기코리아", 100, 30, "2026-08-05", "20일", "위험")
            };

            FilterDataByShip(SelectedShipNo);
        }

        // -----------------------------------------------------------
        // 11. 알림 리스트 데이터 로드
        // -----------------------------------------------------------
        private void LoadNotificationData()
        {
            _allNotificationList = new List<NotificationItem>
            {
                new NotificationItem("A SERIES", "H120", "지연", "생산팀", "(주)엔케이밸브", "배관 팽창 루프 서포트", "설치 일정을 고려하여 현장 반입 조기화 요청", "2026-08-02 14:30"),
                new NotificationItem("A SERIES", "H120", "지연", "협력사", "강림중공업", "현수사다리 거치 브라켓", "부품 수급 차질로 인한 납기 지연 확인", "2026-08-02 11:15"),
                new NotificationItem("A SERIES", "H121", "지연", "협력사", "삼영기계", "BWTS 펌프 메인", "주요 주물 부품 선적 지연 통보", "2026-08-01 17:40"),
                new NotificationItem("B SERIES", "H122", "보류/대기", "생산팀", "비상발전기코리아", "비상발전기 세트", "부하 시험 일정 조율 요청", "2026-07-30 15:25")
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