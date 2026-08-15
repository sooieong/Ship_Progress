using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Ship_Progress.Views;

namespace Ship_Progress
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _clockTimer;         // 1초 실시간 시계용
        private DispatcherTimer _autoRefreshTimer;  // 1분 자동 새로고침용
        private DispatcherTimer _resetStatusTimer;  // 체크 표시 1.5초 후 복귀용
        private Storyboard _rotateStoryboard;        // 아이콘 회전 애니메이션

        // 🎯 멘션 관련 필드 및 인덱스 추적 변수
        private bool _isSelectingMention = false;
        private int _mentionSelectedIndex = -1; // 방향키 순차 이동용 인덱스
        private readonly List<string> _mentionList = new List<string>
        {
            "@설계팀_곽태영",
            "@설계팀_김효빈",
            "@생산팀_변수정",
            "@설계팀_이현곤",
            "@영업팀_홍성현"
        };

        public MainWindow()
        {
            InitializeComponent();

            // 🎯 앱 실행 시 기본 라이트모드 브러시를 강제로 적용하여 테두리선 누락 방지
            ToggleTheme(false);

            // 🎯 실시간 시계 타이머 시작 (1초 마다)
            StartClock();

            // 🎯 회전 애니메이션 리소스 로드
            _rotateStoryboard = (Storyboard)FindResource("RotateRefreshAnimation");

            // 🎯 1분 자동 새로고침 타이머 시작
            StartAutoRefreshTimer();

            // 🎯 실행 시 기본 화면 설정
            if (MainContentViewPort != null)
            {
                MainContentViewPort.Content = new Tab1_MainView(); // 실행 시 탭1 먼저 뜨게 설정
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

            // 다음 정각 00초 시각 계산
            DateTime nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            TimeSpan initialDelay = nextMinute - now;

            DispatcherTimer initialTimer = new DispatcherTimer
            {
                Interval = initialDelay
            };

            initialTimer.Tick += (s, e) =>
            {
                initialTimer.Stop();
                Dispatcher.InvokeAsync(async () => await ExecuteRefreshAsync());

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

        // 🚀 자동/수동 새로고침 프로세스 & UI 애니메이션 제어
        private async Task ExecuteRefreshAsync()
        {
            SetRefreshingUIState(true);
            await FetchDashboardDataAsync();
            UpdateDateTime();
            SetRefreshingUIState(false);
            ShowSuccessState();
        }

        // 데이터 로딩 비동기 로직
        private async Task FetchDashboardDataAsync()
        {
            await Task.Delay(1000);
        }

        // UI 회전 애니메이션 및 텍스트 제어
        private void SetRefreshingUIState(bool isRefreshing)
        {
            if (isRefreshing)
            {
                _resetStatusTimer?.Stop();

                RefreshIconText.Text = "🔄";
                RefreshIconText.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
                RefreshStatusText.Text = "갱신 중...";

                _rotateStoryboard.Begin(RefreshIconText, true);
            }
            else
            {
                _rotateStoryboard.Stop(RefreshIconText);
                RefreshIconRotate.Angle = 0;
            }
        }

        // 1.5초 동안 초록색 체크(✔) 아이콘 표시 후 원복
        private void ShowSuccessState()
        {
            RefreshIconText.Text = "✔";
            RefreshIconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43A047"));
            RefreshStatusText.Text = "업데이트됨";

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
            RefreshIconText.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
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
                        MainContentViewPort.Content = new Tab1_MainView();
                        break;
                    case "Tab2":
                        MainContentViewPort.Content = new Tab2_ShipView();
                        break;
                    case "Tab3":
                        MainContentViewPort.Content = new Tab3_LeadtimeView();
                        break;
                    case "Tab4":
                        MainContentViewPort.Content = new Tab4_SettingView();
                        break;
                }
            }
        }

        // 알림 버튼 클릭 이벤트
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationPopup != null)
            {
                NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
            }
        }

        // 🎯 플레이스홀더: 입력창 클릭(포커스) 시 안내 문구 제거
        private void FeedInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FeedInputTextBox.Text == "멘션(@) 및 메시지 작성...")
            {
                FeedInputTextBox.Text = "";
            }
            // 🎯 테마와 상관없이 무조건 검정 글씨로 강제 설정
            FeedInputTextBox.Foreground = new SolidColorBrush(Colors.Black);
        }

        // 🎯 플레이스홀더: 입력창 포커스 아웃 시 내용이 없으면 안내 문구 복구
        private void FeedInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FeedInputTextBox.Text))
            {
                FeedInputTextBox.Text = "멘션(@) 및 메시지 작성...";
                FeedInputTextBox.Foreground = new SolidColorBrush(Colors.Gray); // 안내 문구는 연한 회색
            }
        }

        // 🎯 텍스트 박스 입력 내용 변경 감지 (@ 멘션 필터링)
        private void FeedInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSelectingMention) return;

            string text = FeedInputTextBox.Text;
            if (text == "멘션(@) 및 메시지 작성...") return;

            int caretIndex = FeedInputTextBox.CaretIndex;
            int lastAtIndex = text.LastIndexOf('@', Math.Max(0, caretIndex - 1));

            if (lastAtIndex >= 0)
            {
                if (lastAtIndex == 0 || char.IsWhiteSpace(text[lastAtIndex - 1]))
                {
                    string query = text.Substring(lastAtIndex, caretIndex - lastAtIndex);

                    if (!query.Contains(" "))
                    {
                        var filtered = _mentionList.Where(m => m.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (filtered.Count > 0)
                        {
                            MentionListBox.ItemsSource = filtered;
                            _mentionSelectedIndex = -1;
                            MentionListBox.SelectedIndex = -1;

                            MentionPopup.PlacementTarget = FeedInputTextBox;
                            MentionPopup.IsOpen = true;
                            return;
                        }
                    }
                }
            }
            MentionPopup.IsOpen = false;
        }

        // 🎯 상하 방향키로 팝업 목록 순차 이동 및 엔터 전송 처리
        private void FeedInputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 🎯 타이핑을 시작할 때 안내 문구가 남아있다면 즉시 지우고 정상 글자색으로 변경
            if (FeedInputTextBox.Text == "멘션(@) 및 메시지 작성...")
            {
                if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Tab)
                {
                    FeedInputTextBox.Text = "";
                    FeedInputTextBox.Foreground = new SolidColorBrush(Colors.Black);
                    FeedInputTextBox.CaretIndex = FeedInputTextBox.Text.Length;
                }
            }

            if (MentionPopup.IsOpen)
            {
                int count = MentionListBox.Items.Count;

                if (e.Key == Key.Down)
                {
                    if (count > 0)
                    {
                        _mentionSelectedIndex = (_mentionSelectedIndex + 1) % count;
                        MentionListBox.SelectedIndex = _mentionSelectedIndex;

                        // 🎯 아래 방향키 이동 시 스크롤바 동기화
                        MentionListBox.ScrollIntoView(MentionListBox.SelectedItem);
                    }
                    e.Handled = true;
                    return;
                }
                else if (e.Key == Key.Up)
                {
                    if (count > 0)
                    {
                        _mentionSelectedIndex = (_mentionSelectedIndex - 1 + count) % count;
                        MentionListBox.SelectedIndex = _mentionSelectedIndex;

                        // 🎯 위 방향키 이동 시 스크롤바 동기화
                        MentionListBox.ScrollIntoView(MentionListBox.SelectedItem);
                    }
                    e.Handled = true;
                    return;
                }
                else if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    if (_mentionSelectedIndex >= 0 && _mentionSelectedIndex < count)
                    {
                        string selectedMention = MentionListBox.Items[_mentionSelectedIndex].ToString();
                        ApplySelectedMention(selectedMention);
                        e.Handled = true;
                        return;
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    MentionPopup.IsOpen = false;
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                SendFeedButton_Click(sender, e);
            }
        }

        // 🎯 선택된 멘션을 텍스트박스에 반영하는 메서드
        private void ApplySelectedMention(string selectedMention)
        {
            _isSelectingMention = true;
            int caretIndex = FeedInputTextBox.CaretIndex;
            string text = FeedInputTextBox.Text;

            int lastAtIndex = text.LastIndexOf('@', Math.Max(0, caretIndex - 1));
            if (lastAtIndex >= 0)
            {
                string newText = text.Substring(0, lastAtIndex) + selectedMention + " " + text.Substring(caretIndex);
                FeedInputTextBox.Text = newText;
                FeedInputTextBox.CaretIndex = lastAtIndex + selectedMention.Length + 1;
            }

            MentionPopup.IsOpen = false;
            _isSelectingMention = false;
            FeedInputTextBox.Focus();
        }

        // 하단 피드 전송 버튼 클릭
        private void SendFeedButton_Click(object sender, RoutedEventArgs e)
        {
            string rawText = FeedInputTextBox.Text?.Trim();

            // 안내 문구이거나 빈 값이면 전송 차단
            if (string.IsNullOrEmpty(rawText) || rawText == "멘션(@) 및 메시지 작성...")
            {
                return;
            }

            string currentTime = DateTime.Now.ToString("HH:mm");

            TextBlock newFeedText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 6)
            };
            newFeedText.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");

            Run deptRun = new Run("[PM] ") { FontWeight = FontWeights.Bold };
            newFeedText.Inlines.Add(deptRun);

            string[] words = rawText.Split(' ');
            foreach (var word in words)
            {
                if (word.StartsWith("@"))
                {
                    Run mentionRun = new Run(word + " ")
                    {
                        FontWeight = FontWeights.Bold
                    };
                    mentionRun.SetResourceReference(Run.ForegroundProperty, "MentionTextBrush");
                    newFeedText.Inlines.Add(mentionRun);
                }
                else
                {
                    newFeedText.Inlines.Add(new Run(word + " "));
                }
            }

            Run timeRun = new Run($"({currentTime})");
            timeRun.SetResourceReference(Run.ForegroundProperty, "SubTextBrush");
            newFeedText.Inlines.Add(timeRun);

            if (FeedListStackPanel.Children.Count > 0)
            {
                Separator separator = new Separator();
                separator.SetResourceReference(Separator.BackgroundProperty, "BorderBrush");
                separator.Margin = new Thickness(0, 2, 0, 6);

                FeedListStackPanel.Children.Insert(0, separator);
                FeedListStackPanel.Children.Insert(0, newFeedText);
            }
            else
            {
                FeedListStackPanel.Children.Add(newFeedText);
            }

            // 전송 후 입력창 초기화 및 플레이스홀더 복구
            FeedInputTextBox.Clear();
            FeedInputTextBox.Text = "멘션(@) 및 메시지 작성...";
            FeedInputTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            FeedScrollViewer?.ScrollToTop();
        }

        // 다크/라이트 모드 전환 함수
        public void ToggleTheme(bool isDarkMode)
        {
            if (isDarkMode)
            {
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                Application.Current.Resources["SidebarBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252526"));
                Application.Current.Resources["MainContentBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212"));
                Application.Current.Resources["CardBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D"));
                Application.Current.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));

                Application.Current.Resources["PrimaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["SubTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

                Application.Current.Resources["MentionTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64B5F6"));
            }
            else
            {
                Application.Current.Resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["SidebarBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F5F7"));
                Application.Current.Resources["MainContentBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEEF2"));
                Application.Current.Resources["CardBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                Application.Current.Resources["MentionTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E88E5"));

                Application.Current.Resources["PrimaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                Application.Current.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                Application.Current.Resources["SubTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
            }
        }
    }
}