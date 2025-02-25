﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using XHS.Common.Events;
using XHS.Common.Events.Model;
using XHS.Common.Global;
using XHS.Common.Helpers;
using XHS.Models.XHS.ApiOutputModel.Me;

namespace XHS.Spider.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        private IEventAggregator _aggregator { get; set; }
        private UserInfoModel _currentUser = new UserInfoModel();

        public UserInfoModel? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
        public MainWindowViewModel(INavigationService navigationService, IEventAggregator aggregator)
        {
            if (!_isInitialized)
            {
                _aggregator=aggregator;
                _aggregator.GetEvent<LoginCompletedCallbackEvent>().Subscribe(SetCurrentUser);
                InitializeViewModel();
            }
        }

        private void SetCurrentUser(UserInfoModel currentUser) {
            if (currentUser!=null)
            {
                GlobalCaChe.CurrentUser = currentUser;
                this.InitCurrentUser();
            }
        }

        private void InitCurrentUser() {
            if (GlobalCaChe.CurrentUser!=null)
            {
                var imageUrl = GlobalCaChe.CurrentUser.Images;
                if (imageUrl!=null)
                {
                    var url = imageUrl.Split('?')[0];
                    GlobalCaChe.CurrentUser.HeadImage = FileHelper.UrlToBitmapImage(url);
                    CurrentUser = GlobalCaChe.CurrentUser;
                }
            }
            else
            {
                var image = DrawHealper.CreateHead("未登录");
                var bit= FileHelper.BitmapToBitmapImage(image);
                CurrentUser.HeadImage = bit;
            }
        }
        private void InitializeViewModel()
        {
            ApplicationTitle = "XHS.Spider-小红书数据采集工具";
            InitCurrentUser();
            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "首页",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home24,
                    PageType = typeof(Views.Pages.DashboardPage)
                },
                new NavigationItem()
                {
                    Content = "搜索",
                    PageTag = "search",
                    Icon = SymbolRegular.Search12,
                    PageType = typeof(Views.Pages.Search)
                },
                //new NavigationItem()
                //{
                //    Content = "Cookie",
                //    PageTag = "cookie",
                //    Icon = SymbolRegular.DataHistogram24,
                //    PageType = typeof(Views.Pages.SettingCookie)
                //}
            };

            NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "设置",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings24,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };

            _isInitialized = true;
        }

    }
}
