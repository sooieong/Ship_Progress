using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ship_Progress
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 🎯 실행 시 기본 화면 설정 (Tab1 화면이 아직 없으므로 안내 문구 표시)
            if (MainContentViewPort != null)
            {
                SetPlaceholderText("메인 (종합 시리즈 분석) 화면 준비 중입니다.");
            }
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
            NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
        }

        // 상단 새로고침 버튼 클릭
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("데이터를 새로고침하였습니다.", "새로고침", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // [참고] 나중에 설정(tab4_Setting)에서 호출할 다크/라이트 모드 전환 함수 예시
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