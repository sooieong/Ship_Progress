using System;
using System.Threading.Tasks;
using System.Windows;

namespace Ship_Progress
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1. 스플래시 시작 창 생성 및 표시
            SplashWindow splash = new SplashWindow();
            splash.Show();

            // 2. 초기 로딩 대기시간 (2초)
            await Task.Delay(2000);

            // 3. 메인 화면 생성 및 출력
            MainWindow mainView = new MainWindow();
            this.MainWindow = mainView;
            mainView.Show();

            // 4. 스플래시 창 닫기
            splash.Close();
        }
    }
}