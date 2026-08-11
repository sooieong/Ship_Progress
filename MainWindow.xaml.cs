using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Ship_Progress
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _clockTimer;        // 1초 실시간 시계용
        private DispatcherTimer _autoRefreshTimer;  // 1분 자동 새로고침용
        private DispatcherTimer _resetStatusTimer;  // 체크 표시 1.5초 후 복귀용
        private Storyboard _rotateStoryboard;       // 아이콘 회전 애니메이션

        public MainWindow()
        {
            InitializeComponent();

            // 🎯 실시간 시계 타이머 시작 (1초 마다)
            StartClock();

            // 🎯 회전 애니메이션 리소스 로드
            _rotateStoryboard = (Storyboard)FindResource("RotateRefreshAnimation");

            // 🎯 1분 자동 새로고침 타이머 시작
            StartAutoRefreshTimer();

            // 🎯 실행 시 기본 화면 설정
            if (MainContentViewPort != null)
            {
                SetPlaceholderText("메인 (종합 시리즈 분석) 화면 준비 중입니다.");
            }
        }

        // 실시간 시계 구동 메서드
        private void StartClock()
        {
            UpdateDateTime(); // 최초 실행 시 즉시 업데이트

            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => UpdateDateTime();
            _clockTimer.Start();
        }

        // ⏱️ 매분 정각 00초에 맞춰 자동 새로고침 실행
        private void StartAutoRefreshTimer()
        {
            DateTime now = DateTime.Now;

            // 다음 정각 00초 시각 계산 (예: 현재 10:04:25라면 다음은 10:05:00)
            DateTime nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);

            // 다음 정각 00초까지 남은 시간 계산 (예: 35초)
            TimeSpan initialDelay = nextMinute - now;

            // 1단계: 첫 정각 00초가 될 때까지 기다리는 일회성 타이머
            DispatcherTimer initialTimer = new DispatcherTimer
            {
                Interval = initialDelay
            };

            initialTimer.Tick += (s, e) =>
            {
                initialTimer.Stop(); // 일회성 타이머 종료

                // 첫 정각(00초) 시점에 1회 새로고침 수행
                Dispatcher.InvokeAsync(async () => await ExecuteRefreshAsync());

                // 2단계: 정각 맞춘 이후부터 정확히 1분(60초) 간격으로 반복 실행하는 타이머 시작
                _autoRefreshTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(1)
                };
                _autoRefreshTimer.Tick += (s2, e2) =>
                {
                    Dispatcher.InvokeAsync(async () => await ExecuteRefreshAsync());
                };
                _autoRefreshTimer.Start();
            };

            initialTimer.Start();
        }

        // 날짜 및 시간 텍스트 업데이트 (YYYY-MM-DD HH:mm:ss)
        private void UpdateDateTime()
        {
            if (DateTimeTextBlock != null)
            {
                DateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        // 🔄 상단 새로고침 버튼 클릭 (수동 실행)
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRefreshAsync();
        }

        // 🚀 핵심: 자동/수동 새로고침 프로세스 & UI 애니메이션 제어
        private async Task ExecuteRefreshAsync()
        {
            // 1. [상태: Refreshing] 회전 애니메이션 시작 및 UI 갱신
            SetRefreshingUIState(true);

            // 2. 비동기 데이터 갱신 작업 (1초 비동기 대기 시뮬레이션)
            await FetchDashboardDataAsync();

            // 3. 시간 최신화
            UpdateDateTime();

            // 4. [상태: Success] 회전 멈추고 초록색 체크 아이콘으로 전환
            SetRefreshingUIState(false);
            ShowSuccessState();
        }

        // 데이터 로딩 비동기 로직 (DB / API 연동 영역)
        private async Task FetchDashboardDataAsync()
        {
            // 실제 데이터 바인딩이나 DB 조회 작업을 진행하시면 됩니다.
            await Task.Delay(1000);
        }

        // UI 회전 애니메이션 및 텍스트 제어
        private void SetRefreshingUIState(bool isRefreshing)
        {
            if (isRefreshing)
            {
                // 복귀 타이머가 작동 중이었다면 중지
                _resetStatusTimer?.Stop();

                RefreshIconText.Text = "🔄";
                RefreshIconText.Foreground = (Brush)Application.Current.Resources["PrimaryTextBrush"];
                RefreshStatusText.Text = "갱신 중...";

                // 회전 애니메이션 시작
                _rotateStoryboard.Begin(RefreshIconText, true);
            }
            else
            {
                // 회전 애니메이션 중지
                _rotateStoryboard.Stop(RefreshIconText);
                RefreshIconRotate.Angle = 0; // 회전 각도 초기화
            }
        }

        // 1.5초 동안 초록색 체크(✔) 아이콘 표시 후 원복
        private void ShowSuccessState()
        {
            RefreshIconText.Text = "✔";
            RefreshIconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43A047")); // 초록색
            RefreshStatusText.Text = "업데이트됨";

            // 1.5초 후 기본 UI 상태로 복귀시키는 타이머
            _resetStatusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            _resetStatusTimer.Tick += (s, e) =>
            {
                ResetRefreshUI();
                _resetStatusTimer.Stop();
            };
            _resetStatusTimer.Start();
        }

        // 기본 UI 상태(Idle)로 복귀
        private void ResetRefreshUI()
        {
            RefreshIconText.Text = "🔄";
            RefreshIconText.Foreground = (Brush)Application.Current.Resources["PrimaryTextBrush"];
            RefreshStatusText.Text = "새로고침";
        }

        // 사이드바 탭 선택 이벤트
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && MainContentViewPort != null)
            {
                string tabTag = radioButton.Tag?.ToString();

                switch (tabTag)
                {
                    case "Tab1":
                        SetPlaceholderText("메인 (종합 시리즈 분석) 화면 준비 중입니다.");
                        break;

                    case "Tab2":
                        SetPlaceholderText("공정 분석 화면 준비 중입니다.");
                        break;

                    case "Tab3":
                        SetPlaceholderText("납기 관리 화면 준비 중입니다.");
                        break;

                    case "Tab4":
                        SetPlaceholderText("시스템 설정 화면 준비 중입니다.");
                        break;
                }
            }
        }

        // 임시 텍스트 생성 함수
        private void SetPlaceholderText(string message)
        {
            MainContentViewPort.Content = new TextBlock
            {
                Text = message,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)Application.Current.Resources["PrimaryTextBrush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        // 알림 버튼 클릭 이벤트
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationPopup != null)
            {
                NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
            }
        }

        // 하단 피드 전송 버튼 클릭
        private void SendFeedButton_Click(object sender, RoutedEventArgs e)
        {
            string rawText = FeedInputTextBox.Text?.Trim();

            if (!string.IsNullOrEmpty(rawText))
            {
                string currentTime = DateTime.Now.ToString("HH:mm");

                TextBlock newFeedText = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 6),
                    Foreground = (Brush)Application.Current.Resources["PrimaryTextBrush"]
                };

                Run deptRun = new Run("[PM] ") { FontWeight = FontWeights.Bold };
                newFeedText.Inlines.Add(deptRun);

                string[] words = rawText.Split(' ');
                foreach (var word in words)
                {
                    if (word.StartsWith("@"))
                    {
                        Run mentionRun = new Run(word + " ")
                        {
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2")),
                            FontWeight = FontWeights.Bold
                        };
                        newFeedText.Inlines.Add(mentionRun);
                    }
                    else
                    {
                        newFeedText.Inlines.Add(new Run(word + " "));
                    }
                }

                Run timeRun = new Run($"({currentTime})")
                {
                    Foreground = (Brush)Application.Current.Resources["SubTextBrush"]
                };
                newFeedText.Inlines.Add(timeRun);

                if (FeedListStackPanel.Children.Count > 0)
                {
                    Separator separator = new Separator
                    {
                        Background = (Brush)Application.Current.Resources["BorderBrush"],
                        Margin = new Thickness(0, 2, 0, 6)
                    };
                    FeedListStackPanel.Children.Insert(0, separator);
                    FeedListStackPanel.Children.Insert(0, newFeedText);
                }
                else
                {
                    FeedListStackPanel.Children.Add(newFeedText);
                }

                FeedInputTextBox.Clear();
                FeedScrollViewer?.ScrollToTop();
            }
        }

        // 다크/라이트 모드 전환 함수
        public void ToggleTheme(bool isDarkMode)
        {
            if (isDarkMode)
            {
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                Application.Current.Resources["SidebarBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252526"));
                Application.Current.Resources["MainContentBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212"));
                Application.Current.Resources["PrimaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            }
            else
            {
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["SidebarBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F5F7"));
                Application.Current.Resources["MainContentBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEEF2"));
                Application.Current.Resources["PrimaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
            }
        }
    }
}