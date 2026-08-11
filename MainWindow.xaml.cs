using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ship_Progress
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _clockTimer;

        public MainWindow()
        {
            InitializeComponent();

            // 🎯 실시간 시계 타이머 설정 (1초마다 갱신)
            StartClock();

            // 🎯 실행 시 기본 화면 설정 (Tab1 화면이 아직 없으므로 안내 문구 표시)
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

        // 날짜 및 시간 텍스트 업데이트 (YYYY-MM-DD HH:mm:ss)
        private void UpdateDateTime()
        {
            if (DateTimeTextBlock != null)
            {
                DateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        // 상단 새로고침 버튼 클릭 (중복 제거 및 통합 완료)
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. 시간 최신화
            UpdateDateTime();

            // 2. 새로고침 완료 안내 메시지 표시
            MessageBox.Show($"데이터가 새로고침되었습니다.\n(기준 시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss})",
                            "새로고침 완료",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        // 사이드바 탭 선택 이벤트
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && MainContentViewPort != null)
            {
                string tabTag = radioButton.Tag?.ToString();

                // 🎯 각 브랜치(feature/tab1_Main, feature/tab2_Ship 등)에서 
                // 해당 View를 작성한 뒤 아래 주석을 풀고 연결하시면 됩니다!
                switch (tabTag)
                {
                    case "Tab1": // feature/tab1_Main 브랜치에서 작업 예정
                        // MainContentViewPort.Content = new Tab1_MainView();
                        SetPlaceholderText("메인 (종합 시리즈 분석) 화면 준비 중입니다.");
                        break;

                    case "Tab2": // feature/tab2_Ship 브랜치에서 작업 예정
                        // MainContentViewPort.Content = new Tab2_ShipView();
                        SetPlaceholderText("공정 분석 화면 준비 중입니다.");
                        break;

                    case "Tab3": // feature/tab3_Leadtime 브랜치에서 작업 예정
                        // MainContentViewPort.Content = new Tab3_LeadtimeView();
                        SetPlaceholderText("납기 관리 화면 준비 중입니다.");
                        break;

                    case "Tab4": // feature/tab4_Setting 브랜치에서 작업 예정
                        // MainContentViewPort.Content = new Tab4_SettingView();
                        SetPlaceholderText("시스템 설정 화면 준비 중입니다.");
                        break;
                }
            }
        }

        // 💡 화면이 아직 없는 탭을 클릭했을 때 표시해 줄 임시 텍스트 생성 함수
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
            // 알림 팝업 창의 열림/닫힘 상태를 토글
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
                // 1. 현재 시간 가져오기 (HH:mm 포맷)
                string currentTime = DateTime.Now.ToString("HH:mm");

                // 2. 새로운 피드용 TextBlock 생성
                TextBlock newFeedText = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 0, 6),
                    Foreground = (Brush)Application.Current.Resources["PrimaryTextBrush"]
                };

                // 예시용 내 부서 정보
                Run deptRun = new Run("[PM] ") { FontWeight = FontWeights.Bold };
                newFeedText.Inlines.Add(deptRun);

                // 3. 작성된 텍스트 파싱 (@멘션 처리 및 시간 추가)
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

                // 4. 실시간 작성 시간 추가
                Run timeRun = new Run($"({currentTime})")
                {
                    Foreground = (Brush)Application.Current.Resources["SubTextBrush"]
                };
                newFeedText.Inlines.Add(timeRun);

                // 5. 구분선(Separator) 생성 및 최신 피드 상단 추가
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

                // 6. 입력창 초기화
                FeedInputTextBox.Clear();

                // 7. 스크롤을 맨 위로 이동
                FeedScrollViewer?.ScrollToTop();
            }
        }

        // [참고] 다크/라이트 모드 전환 함수 예시
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