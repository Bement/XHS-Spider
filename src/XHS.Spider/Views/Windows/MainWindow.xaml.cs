﻿using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;
using XHS.Common.Events;
using XHS.Common.Global;
using XHS.Common.Utils;
using XHS.Spider.Helpers;
using XHS.Spider.Services;
using XHS.Spider.ViewModels;
using XHS.IService;
using EdgeSharp.Core.Infrastructure;
using XHS.Service.Log;

namespace XHS.Spider.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        private static readonly ILogger Logger = LoggerService.Get(typeof(MainWindow));
        private IEventAggregator _aggregator { get; set; }
        private readonly TaskbarIcon _notifyIcon;
        private ContextMenu _contextMenu;
        private UpdateCheckerServer updateChecker;
        private bool _initialized = false;
        private readonly ITaskBarService _taskBarService;
        private readonly IPageServiceNew _pageServiceNew;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly IWindowService _windowService;
        public MainWindowViewModel ViewModel
        {
            get;
        }

        public MainWindow(MainWindowViewModel viewModel, 
            IPageServiceNew pageService, 
            ITaskBarService taskBarService, 
            INavigationService navigationService, 
            IServiceProvider serviceProvider, 
            ISnackbarService snackbarService,
            IWindowService windowService,
            IEventAggregator aggregator)
        {
            _navigationService = navigationService;
            _windowService = windowService;
            _aggregator = aggregator;
            _taskBarService = taskBarService;
            _serviceProvider = serviceProvider;
            ViewModel = viewModel;
            DataContext = this;
            #region 通知
            _notifyIcon = new TaskbarIcon();
            _notifyIcon.TrayBalloonTipClicked += notifyIcon_TrayBalloonTipClicked;
            updateChecker = new UpdateCheckerServer();
            updateChecker.NewVersionFound += updateChecker_NewVersionFound;
            updateChecker.NewVersionNotFound += updateChecker_NewVersionNotFound;
            #endregion
            InitializeComponent();
            try
            {
                #region webView
                webView.Source = new Uri("https://www.xiaohongshu.com/explore");
                InitializeAsync();
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Error("webview初始化失败",ex);
            }
            
            SetPageService(pageService);
            _pageServiceNew = pageService;
            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarControl(RootSnackbar);
            Loaded += (_, _) => InvokeSplashScreen();
        }
        #region 通知
        private void updateChecker_NewVersionFound(object sender, EventArgs e)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                if (updateChecker.Found)
                {
                    _notifyIcon.ShowBalloonTip(
                                string.Format("{0}{1}更新",
                                        UpdateCheckerServer.Name, updateChecker.LatestVersionNumber),
                                "点击下载新版本", BalloonIcon.Info);
                }
            });
        }

        private void updateChecker_NewVersionNotFound(object sender, EventArgs e)
        {
            _notifyIcon.ShowBalloonTip($@"{UpdateCheckerServer.Name} {UpdateCheckerServer.FullVersion}",
            $@"没有找到新版本{Environment.NewLine}{UpdateCheckerServer.Version}≥{updateChecker.LatestVersionNumber}",
            BalloonIcon.Info);
        }

        private void UpdateChecker_NewVersionFoundFailed(object sender, EventArgs e)
        {
            _notifyIcon.ShowBalloonTip($@"{UpdateCheckerServer.Name} {UpdateCheckerServer.FullVersion}","检查更新失败", BalloonIcon.Info);
        }
        private void notifyIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            var url = updateChecker.LatestVersionUrl;
            if (!string.IsNullOrWhiteSpace(url))
            {
                Utils.OpenURL(url);
            }
        }
        #endregion

        private void InvokeSplashScreen()
        {
            try
            {
                if (_initialized)
                    return;

                _initialized = true;

                RootMainGrid.Visibility = Visibility.Collapsed;
                RootWelcomeGrid.Visibility = Visibility.Visible;

                _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);
                Task.Run(async () =>
                {
                    //TODO:这里预留程序启动初始化数据

                    ScriptHost.GetScriptHost(GlobalCaChe.webView, _aggregator);
                    //await Task.Delay(2000);
                    updateChecker.Check(true);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        RootWelcomeGrid.Visibility = Visibility.Hidden;
                        RootMainGrid.Visibility = Visibility.Visible;

                        Navigate(typeof(Pages.DashboardPage));

                        _taskBarService.SetState(this, TaskBarProgressState.None);
                    });

                    return true;
                });
            }
            catch (Exception ex)
            {
                Logger.Error("初始化失败",ex);
            }
            
        }
        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        private async void InitializeAsync()
        {
            GlobalCaChe.webView = this.webView;
            await webView.EnsureCoreWebView2Async(null);
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
        }
        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (GlobalCaChe.clipboardHooker != null)
            {
                GlobalCaChe.clipboardHooker.Dispose();
            }
            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
        {
            //_navigationService.GetNavigationControl().Current.PageTag;
            // This funky solution allows us to impose a negative
            // margin for Frame only for the Dashboard page, thanks
            // to which the banner will cover the entire page nicely.
            RootFrame.Margin = new Thickness(
                left: 0,
                top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
                right: 0,
                bottom: 0);
        }

        private async void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var currentUser = GlobalCaChe.CurrentUser;
            if (currentUser != null)
            {
                var navigation = _navigationService.GetNavigationControl();
                var userId= currentUser.UserId;
                var searchService = SearchService.GetSearchService(webView, _aggregator, navigation, _serviceProvider, _pageServiceNew);
                string url = $"https://www.xiaohongshu.com/user/profile/{userId}";
                searchService.SearchInput(url);
            }
            else
                _windowService.Show<ScanLogin>();
        }
    }
}