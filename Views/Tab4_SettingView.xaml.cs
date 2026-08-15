using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ship_Progress.Views
{
    public partial class Tab4_SettingView : UserControl
    {
        private bool _isInitializing = true;

        public Tab4_SettingView()
        {
            InitializeComponent();

            // 초기화 중 이벤트 발생 방지 후 상태 반영
            Loaded += Tab4_SettingView_Loaded;
        }

        private void Tab4_SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 이벤트 중복 실행을 막기 위해 플래그 설정
            _isInitializing = true;

            // 2. 이전에 선택해둔 테마 상태를 가져옴 (기본값은 false = 라이트모드)
            bool isDark = false;
            if (Application.Current.Properties.Contains("IsDarkMode"))
            {
                isDark = (bool)Application.Current.Properties["IsDarkMode"];
            }

            // 3. 저장된 상태에 맞춰 라디오 버튼 UI 체크 상태 변경
            if (isDark)
            {
                DarkModeRadio.IsChecked = true;
                LightModeRadio.IsChecked = false;
            }
            else
            {
                LightModeRadio.IsChecked = true;
                DarkModeRadio.IsChecked = false;
            }

            // 4. 카드 테두리 업데이트
            UpdateCardBorder(isDark);

            _isInitializing = false;
        }

        // 라디오 버튼 선택 이벤트
        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || LightModeCard == null || DarkModeCard == null) return;

            bool isDark = DarkModeRadio.IsChecked == true;

            // 1. 사용자가 선택한 테마 상태를 앱 전역에 저장 (다른 탭을 갔다가 와도 기억하도록)
            Application.Current.Properties["IsDarkMode"] = isDark;

            // 2. 카드 테두리 강조 변경
            UpdateCardBorder(isDark);

            // 3. MainWindow의 ToggleTheme 메서드 호출
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ToggleTheme(isDark);
            }
        }

        /// <summary>
        /// 리소스 이름 예외(KeyNotFoundException)를 방지하도록 리소스 탐색 처리
        /// </summary>
        private void UpdateCardBorder(bool isDark)
        {
            if (LightModeCard == null || DarkModeCard == null) return;

            // 1. 주황색 브러시 안전하게 가져오기
            Brush orangeBrush = GetResourceBrush("HanwhaOrangeBrush") ?? new SolidColorBrush(Color.FromRgb(243, 115, 33));

            // 2. 기본 테두리 브러시 안전하게 가져오기 (AppBorderBrush 또는 BorderBrush 처리)
            Brush defaultBorderBrush = GetResourceBrush("AppBorderBrush")
                                    ?? GetResourceBrush("BorderBrush")
                                    ?? new SolidColorBrush(Color.FromRgb(224, 224, 224));

            if (isDark)
            {
                DarkModeCard.BorderBrush = orangeBrush;
                DarkModeCard.BorderThickness = new Thickness(2);

                LightModeCard.BorderBrush = defaultBorderBrush;
                LightModeCard.BorderThickness = new Thickness(1);
            }
            else
            {
                LightModeCard.BorderBrush = orangeBrush;
                LightModeCard.BorderThickness = new Thickness(2);

                DarkModeCard.BorderBrush = defaultBorderBrush;
                DarkModeCard.BorderThickness = new Thickness(1);
            }
        }

        /// <summary>
        /// 예외 발생 없이 동적 리소스를 안전하게 가져오는 도우미 메서드
        /// </summary>
        private Brush GetResourceBrush(string resourceKey)
        {
            try
            {
                return Application.Current.TryFindResource(resourceKey) as Brush;
            }
            catch
            {
                return null;
            }
        }

        // 라이트 모드 카드 영역 클릭 시 라디오 체크
        private void LightModeCard_Click(object sender, MouseButtonEventArgs e)
        {
            LightModeRadio.IsChecked = true;
        }

        // 다크 모드 카드 영역 클릭 시 라디오 체크
        private void DarkModeCard_Click(object sender, MouseButtonEventArgs e)
        {
            DarkModeRadio.IsChecked = true;
        }
    }
}