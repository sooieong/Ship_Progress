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
            // 뷰가 로드될 때 MainWindow의 상태 확인
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // 다크모드 체크 여부 (필요 시 라디오 버튼 상태 조절)
                bool isDark = DarkModeRadio.IsChecked == true;
                UpdateCardBorder(isDark);
            }
            _isInitializing = false;
        }

        // 라디오 버튼 선택 이벤트
        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || LightModeCard == null || DarkModeCard == null) return;

            bool isDark = DarkModeRadio.IsChecked == true;

            // 1. 카드 테두리 강조 변경
            UpdateCardBorder(isDark);

            // 2. MainWindow의 ToggleTheme 메서드 안전하게 호출
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // MainWindow에 구현된 ToggleTheme 메소드 호출
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