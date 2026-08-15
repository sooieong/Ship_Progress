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

        private int _kpiCompletedCount = 0;
        public int KpiCompletedCount { get => _kpiCompletedCount; set { _kpiCompletedCount = value; OnPropertyChanged(); } }
        private string _kpiCompletedPercent = "(0%)";
        public string KpiCompletedPercent { get => _kpiCompletedPercent; set { _kpiCompletedPercent = value; OnPropertyChanged(); } }

        private int _kpiDelayCount = 0;
        public int KpiDelayCount { get => _kpiDelayCount; set { _kpiDelayCount = value; OnPropertyChanged(); } }
        private string _kpiDelayPercent = "(0%)";
        public string KpiDelayPercent { get => _kpiDelayPercent; set { _kpiDelayPercent = value; OnPropertyChanged(); } }

        private int _kpiHoldCount = 0;
        public int KpiHoldCount { get => _kpiHoldCount; set { _kpiHoldCount = value; OnPropertyChanged(); } }
        private string _kpiHoldPercent = "(0%)";
        public string KpiHoldPercent { get => _kpiHoldPercent; set { _kpiHoldPercent = value; OnPropertyChanged(); } }

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
            if (sender is CheckBox chk && chk.DataContext is LeadtimeItem item)
            {
                var currentItems = GetFilteredLeadtimeList(SelectedShipNo);
                bool hasAnySelection = currentItems.Any(x => x.IsSelected);

                if (!hasAnySelection)
                {
                    MessageBox.Show("하나 이상의 기자재를 선택해야 합니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                    item.IsSelected = true;
                    return;
                }
            }

            UpdateChartDataForShip(SelectedShipNo);
        }

        private void UpdateChartDataForShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            var currentItems = GetFilteredLeadtimeList(shipNo);

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
            if (percent >= 70.0)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (percent >= 40.0)
                return new SolidColorBrush(Color.FromRgb(234, 179, 8));
            else
                return new SolidColorBrush(Color.FromRgb(229, 57, 53));
        }

        // -----------------------------------------------------------
        // KPI 계산 및 갱신 메서드
        // -----------------------------------------------------------
        private void UpdateKpiData(string shipNo)
        {
            if (_allLeadtimeList == null || _allNotificationList == null) return;

            // 1. 납품 완료 및 지연(위험+주의) 건수 계산 (납기 데이터 기준)
            var shipLeadtimeItems = _allLeadtimeList.Where(x => x.ShipNo == shipNo).ToList();
            KpiCompletedCount = shipLeadtimeItems.Count(x => x.Status == "납품 완료");
            KpiDelayCount = shipLeadtimeItems.Count(x => x.Status == "위험" || x.Status == "주의");

            // 2. 보류/대기 건수 계산 (알림 데이터 기준)
            var shipNotificationItems = _allNotificationList.Where(x => x.ShipNo == shipNo).ToList();
            KpiHoldCount = shipNotificationItems.Count(x => x.Category == "보류/대기");

            // 3. 전체 KPI 항목의 합산 건수 (100%의 기준)
            int totalKpiCount = KpiCompletedCount + KpiDelayCount + KpiHoldCount;

            if (totalKpiCount > 0)
            {
                // 각각의 비율 계산
                double completedPct = ((double)KpiCompletedCount / totalKpiCount) * 100.0;
                double delayPct = ((double)KpiDelayCount / totalKpiCount) * 100.0;
                double holdPct = ((double)KpiHoldCount / totalKpiCount) * 100.0;

                // 반올림했을 때 정확히 100%가 되도록 가장 큰 값에 잔여 오차를 보정할 수도 있지만,
                // 일반적인 정수 표기로 출력합니다.
                KpiCompletedPercent = $"({Math.Round(completedPct)}%)";
                KpiDelayPercent = $"({Math.Round(delayPct)}%)";
                KpiHoldPercent = $"({Math.Round(holdPct)}%)";
            }
            else
            {
                KpiCompletedPercent = "(0%)";
                KpiDelayPercent = "(0%)";
                KpiHoldPercent = "(0%)";
            }
        }

        // 🎯 검색창을 클릭(포커스)했을 때 안내 문구 지우기
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "품목명 검색...")
            {
                SearchTextBox.Text = "";
            }
            // 🎯 테마와 상관없이 무조건 검정 글씨로 강제 설정
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Black);
        }

        // 🎯 검색창에서 마우스가 벗어났을 때, 아무것도 안 적혀있으면 다시 안내 문구 채우기
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "품목명 검색...";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray); // 안내 문구는 회색
            }
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

            public bool IsEnabled => Status != "납품 완료";

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

                if (status == "납품 완료")
                {
                    _isSelected = false;
                }
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
            public string Vendor { get; set; }
            public string EquipmentName { get; set; }
            public string NoticeMessage { get; set; }
            public string RegisterDate { get; set; }

            public Brush CategoryBgBrush => Category switch
            {
                "지연" => new SolidColorBrush(Color.FromArgb(40, 251, 140, 0)),     // 연한 주황 배경 (지연)
                "보류/대기" => new SolidColorBrush(Color.FromArgb(40, 229, 57, 53)),  // 연한 빨강 배경 (보류/대기)
                _ => new SolidColorBrush(Color.FromArgb(40, 150, 150, 150))
            };

            public Brush CategoryFgBrush => Category switch
            {
                "지연" => new SolidColorBrush(Color.FromRgb(251, 140, 0)),          // 진한 주황 텍스트 (지연)
                "보류/대기" => new SolidColorBrush(Color.FromRgb(229, 57, 53)),     // 진한 빨강 텍스트 (보류/대기)
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };

            public NotificationItem(string series, string shipNo, string category, string vendor, string equipmentName, string noticeMessage, string registerDate)
            {
                Series = series;
                ShipNo = shipNo;
                Category = category;
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
            UpdateKpiData(SelectedShipNo);
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
                    SearchTextBox.Text = "품목명 검색...";
                    SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray); // 안내 문구는 회색
                }

                FilterDataByShip(shipNo);
                FilterNotifications(shipNo, _currentNotificationFilter);
                UpdateChartDataForShip(shipNo);
                UpdateSupplyChartData(shipNo);
                UpdateKpiData(shipNo);
            }
        }

        // -----------------------------------------------------------
        // 페이징 및 필터링 관련 로직
        // -----------------------------------------------------------
        private int _currentPage = 1;
        private const int _pageSize = 5;
        private List<LeadtimeItem> _currentFilteredList = new List<LeadtimeItem>();

        private void FilterDataByShip(string shipNo)
        {
            if (_allLeadtimeList == null) return;

            _currentFilteredList = GetFilteredLeadtimeList(shipNo);

            _currentPage = 1;
            UpdatePagedDataGrid();
        }

        private void UpdatePagedDataGrid()
        {
            if (_currentFilteredList == null) return;

            int totalItems = _currentFilteredList.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_pageSize));

            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            var pagedData = _currentFilteredList
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            if (LeadtimeRiskDataGrid != null)
            {
                LeadtimeRiskDataGrid.ItemsSource = pagedData;
            }

            if (PageInfoTextBlock != null)
            {
                PageInfoTextBlock.Text = $"{_currentPage} / {totalPages}";
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagedDataGrid();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling(_currentFilteredList.Count / (double)_pageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                UpdatePagedDataGrid();
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

            // 🎯 [수정] 검색 키워드가 비어있거나, 플레이스홀더 안내 문구인 경우 검색 필터 무시
            if (!string.IsNullOrWhiteSpace(_currentSearchKeyword) && _currentSearchKeyword != "품목명 검색...")
            {
                query = query.Where(x => x.EquipmentName != null && x.EquipmentName.Contains(_currentSearchKeyword));
            }

            return query
                .OrderByDescending(x => x.RemainingDaysValue)
                .ThenBy(x => GetStatusSortOrder(x.Status))
                .ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _currentSearchKeyword = tb.Text.Trim();
                FilterDataByShip(SelectedShipNo);
            }
        }

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
        // 9. 납기 현황 차트 드로잉
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

            double maxTarget = currentChartTargets.Length > 0 ? currentChartTargets.Max() : 400;
            double maxGap = 0;
            for (int i = 0; i < currentChartTargets.Length; i++)
            {
                double gap = Math.Max(0, currentChartTargets[i] - currentChartActuals[i]);
                if (gap > maxGap) maxGap = gap;
            }

            if (maxTarget <= 0) maxTarget = 100;
            if (maxGap <= 0) maxGap = 50;

            double maxBarValue = Math.Ceiling(maxTarget / 100.0) * 100;
            if (maxBarValue < 200) maxBarValue = 200;

            double maxGapValue = Math.Ceiling(maxGap / 50.0) * 50;
            if (maxGapValue < 100) maxGapValue = 100;

            for (int i = 0; i <= 4; i++)
            {
                double yVal = (maxBarValue / 4.0) * i;
                double yPos = paddingTop + chartH - (yVal / maxBarValue * chartH);

                Line gridLine = new Line
                {
                    X1 = paddingLeft,
                    Y1 = yPos,
                    X2 = width - paddingRight,
                    Y2 = yPos,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    StrokeThickness = 1
                    //Opacity = 0.5
                };
                LeadtimeChartCanvas.Children.Add(gridLine);

                TextBlock yLabel = new TextBlock { Text = $"{yVal:F0}", FontSize = 10, Foreground = textBrush, FontWeight = FontWeights.SemiBold };
                yLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(yLabel, paddingLeft - yLabel.DesiredSize.Width - 6);
                Canvas.SetTop(yLabel, yPos - 7);
                LeadtimeChartCanvas.Children.Add(yLabel);
            }

            for (int i = 0; i <= 4; i++)
            {
                double gVal = (maxGapValue / 4.0) * i;
                double yPos = paddingTop + chartH - (gVal / maxGapValue * chartH);

                TextBlock rightYLabel = new TextBlock
                {
                    Text = $"{gVal:F0}",
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
                Canvas.SetTop(valueLabel, lineY - 18);
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
                new LeadtimeItem("A SERIES", "H120", "기관", "갑판 드레인 스커퍼", "성광벤드", 120, 0, "2026-09-25", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H120", "기관", "래싱 브릿지 Cross Bar", "오리엔탈정공", 320, 320, "2026-05-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "곡형 핸드 그립", "동성하이텍", 280, 280, "2026-04-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "워터 타이트 맨홀 커버", "강림중공업", 240, 240, "2026-05-27", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "아이 플레이트", "삼우중공업", 150, 150, "2026-07-08", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "수직 사다리 A타입", "동성하이텍", 260, 260, "2026-04-01", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선체", "미끄럼 방지 바 2형", "동성하이텍", 210, 210, "2026-04-23", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "현수사다리 거치 브라켓", "강림중공업", 60, 30, "2026-08-10", "11일", "위험"),
                new LeadtimeItem("A SERIES", "H120", "선실", "거주구 창문 코밍", "STACO", 52, 20, "2026-08-13", "8일", "주의"),
                new LeadtimeItem("A SERIES", "H120", "선실", "청동 소화전 각밸브", "STACO", 180, 180, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "내화 정전기 방지 카펫", "STACO", 350, 350, "2026-07-12", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "선실", "주방/세탁실 미끄럼방지 타일", "STACO", 220, 220, "2026-07-14", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "전장 단로기 부착 브라켓", "현대일렉트릭", 260, 100, "2026-08-16", "10일", "위험"),
                new LeadtimeItem("A SERIES", "H120", "전기", "세면대/수도 배관 서포트", "대양전기공업", 50, 25, "2026-08-14", "7일", "주의"),
                new LeadtimeItem("A SERIES", "H120", "전기", "케이블 윈치 러그", "대한전선", 52, 52, "2026-05-03", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "대형 LED 투광등", "대양전기공업", 130, 130, "2026-05-07", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "비상 유도 표지등", "대양전기공업", 140, 140, "2026-05-15", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "자동 전화기", "대양전기공업", 100, 100, "2026-07-02", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H120", "전기", "정온식/차동식 열 감지기", "한화시스템", 240, 240, "2026-07-24", "-", "납품 완료"),

                new LeadtimeItem("A SERIES", "H121", "기관", "배관 팽창 루프 서포트", "세진중공업", 65, 65, "2026-05-07", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "기관", "선저 배수 플러그 소켓", "세진중공업", 70, 70, "2026-05-13", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "기관", "러그형 버터플라이 밸브", "삼진정공", 280, 280, "2026-06-06", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "기관", "갑판 드레인 스커퍼", "성광벤드", 120, 0, "2026-11-17", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H121", "기관", "래싱 브릿지 Cross Bar", "오리엔탈정공", 320, 100, "2026-08-14", "7일", "주의"),
                new LeadtimeItem("A SERIES", "H121", "선체", "곡형 핸드 그립", "동성하이텍", 280, 280, "2026-05-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선체", "워터 타이트 맨홀 커버", "강림중공업", 240, 240, "2026-06-26", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선체", "아이 플레이트", "삼우중공업", 150, 150, "2026-08-07", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선체", "수직 사다리 A타입", "동성하이텍", 260, 120, "2026-08-11", "10일", "위험"),
                new LeadtimeItem("A SERIES", "H121", "선체", "미끄럼 방지 바 2형", "동성하이텍", 210, 210, "2026-05-23", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선실", "현수사다리 거치 브라켓", "강림중공업", 60, 60, "2026-05-01", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선실", "거주구 창문 코밍", "STACO", 52, 52, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선실", "청동 소화전 각밸브", "STACO", 180, 180, "2026-06-10", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "선실", "내화 정전기 방지 카펫", "STACO", 350, 50, "2026-09-06", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H121", "선실", "주방/세탁실 미끄럼방지 타일", "STACO", 220, 20, "2026-09-08", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H121", "전기", "전장 단로기 부착 브라켓", "현대일렉트릭", 260, 260, "2026-05-03", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "전기", "세면대/수도 배관 서포트", "대양전기공업", 50, 50, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "전기", "케이블 윈치 러그", "대한전선", 52, 20, "2026-08-15", "6일", "주의"),
                new LeadtimeItem("A SERIES", "H121", "전기", "대형 LED 투광등", "대양전기공업", 130, 130, "2026-06-26", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "전기", "비상 유도 표지등", "대양전기공업", 140, 140, "2026-07-04", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H121", "전기", "자동 전화기", "대양전기공업", 100, 20, "2026-08-18", "3일", "주의"),
                new LeadtimeItem("A SERIES", "H121", "전기", "정온식/차동식 열 감지기", "한화시스템", 240, 40, "2026-10-18", "-", "납품 예정"),

                new LeadtimeItem("A SERIES", "H122", "기관", "배관 팽창 루프 서포트", "세진중공업", 65, 65, "2026-05-07", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "기관", "선저 배수 플러그 소켓", "세진중공업", 70, 70, "2026-05-13", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "기관", "러그형 버터플라이 밸브", "삼진정공", 280, 30, "2026-12-26", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "기관", "갑판 드레인 스커퍼", "성광벤드", 120, 0, "2026-11-24", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "기관", "래싱 브릿지 Cross Bar", "오리엔탈정공", 320, 320, "2026-06-20", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선체", "곡형 핸드 그립", "동성하이텍", 280, 280, "2026-05-21", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선체", "워터 타이트 맨홀 커버", "강림중공업", 240, 240, "2026-06-26", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선체", "아이 플레이트", "삼우중공업", 150, 10, "2026-12-08", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "선체", "수직 사다리 A타입", "동성하이텍", 260, 70, "2026-08-11", "10일", "위험"),
                new LeadtimeItem("A SERIES", "H122", "선체", "미끄럼 방지 바 2형", "동성하이텍", 210, 210, "2026-05-23", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선실", "현수사다리 거치 브라켓", "강림중공업", 60, 30, "2026-08-13", "8일", "주의"),
                new LeadtimeItem("A SERIES", "H122", "선실", "거주구 창문 코밍", "STACO", 52, 52, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선실", "청동 소화전 각밸브", "STACO", 180, 180, "2026-06-10", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "선실", "내화 정전기 방지 카펫", "STACO", 350, 30, "2026-12-12", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "선실", "주방/세탁실 미끄럼방지 타일", "STACO", 220, 50, "2026-12-14", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "전기", "전장 단로기 부착 브라켓", "현대일렉트릭", 260, 260, "2026-05-03", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "전기", "세면대/수도 배관 서포트", "대양전기공업", 50, 50, "2026-05-11", "-", "납품 완료"),
                new LeadtimeItem("A SERIES", "H122", "전기", "케이블 윈치 러그", "대한전선", 52, 15, "2026-08-15", "6일", "주의"),
                new LeadtimeItem("A SERIES", "H122", "전기", "대형 LED 투광등", "대양전기공업", 130, 20, "2026-10-07", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "전기", "비상 유도 표지등", "대양전기공업", 140, 0, "2026-10-15", "-", "납품 예정"),
                new LeadtimeItem("A SERIES", "H122", "전기", "자동 전화기", "대양전기공업", 100, 20, "2026-08-18", "3일", "주의"),
                new LeadtimeItem("A SERIES", "H122", "전기", "정온식/차동식 열 감지기", "한화시스템", 240, 40, "2026-12-24", "-", "납품 예정")
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
                new NotificationItem("A SERIES", "H120", "지연", "세진중공업", "배관 팽창 루프 서포트", "원부자재 수급 지연으로 인해 납기 2일 연기 요청", "2026-08-19 15:11"),
                new NotificationItem("A SERIES", "H120", "보류/대기", "대양전기공업", "세면대/수도 배관 서포트", "협력사 제작 물량 폭주로 인해 납기 조절 요청", "2026-08-20 09:02"),
                new NotificationItem("A SERIES", "H121", "지연", "동성하이텍", "수직 사다리 A타입", "원자재 입고 지연으로 인해 납기 3일 연기 요청", "2026-08-19 13:11"),
                new NotificationItem("A SERIES", "H121", "보류/대기", "대양전기공업", "자동 전화기", "부품 조달 일정으로 인해 납기 5일 연기 요청", "2026-08-18 10:22"),
                new NotificationItem("A SERIES", "H122", "지연", "동성하이텍", "수직 사다리 A타입", "원자재 입고 지연으로 인해 납기 3일 연기 요청", "2026-08-19 13:11"),
                new NotificationItem("A SERIES", "H122", "지연", "대한전선", "케이블 윈치 러그", "도면 수정 작업 반영으로 인해 납기 2일 연기 요청", "2026-08-21 09:30"),
                new NotificationItem("A SERIES", "H122", "보류/대기", "대양전기공업", "자동 전화기", "부품 조달 일정으로 인해 납기 5일 연기 요청", "2026-08-18 10:22")
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