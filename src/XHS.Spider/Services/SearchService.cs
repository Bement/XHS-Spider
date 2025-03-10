﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using UpdateChecker.Interfaces;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.TaskBar;
using XHS.Common.Events;
using XHS.Common.Events.Model;
using XHS.Common.Global;
using XHS.Models.Events;
using XHS.Service.Log;
using XHS.Spider.Helpers;
using XHS.Spider.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace XHS.Spider.Services
{
    public class SearchService
    {
        private static readonly Service.Log.ILogger Logger = LoggerService.Get(typeof(SearchService));

        public static SearchService searchService = null;
        private static IEventAggregator _aggregator { get; set; }
        private static INavigation _navigation;
        private static IServiceProvider _serviceProvider;
        private static IPageServiceNew _pageServiceNew;
        //private static WebView2 _webView;
        private SearchService(WebView2 webView, IEventAggregator aggregator, INavigation navigation, IServiceProvider serviceProvider, IPageServiceNew pageServiceNew) {
            //_webView = webView; 
            _aggregator = aggregator;
            _navigation = navigation;
            _serviceProvider = serviceProvider;
            _pageServiceNew = pageServiceNew;

        }
        public static SearchService GetSearchService(WebView2 webView, IEventAggregator aggregator, INavigation navigation, IServiceProvider serviceProvider, IPageServiceNew pageServiceNew)
        {
            if (searchService == null) searchService = new SearchService(webView,aggregator,navigation,serviceProvider,pageServiceNew);
            return searchService;
        }
        /// <summary>
        /// 解析支持的输入
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SearchInput(string input)
        {
            bool isSubscribeEvent = false;
            string url = input;
            if (input.Contains("user/profile"))
            {
                try
                {
                    isSubscribeEvent = true;
                }
                catch (Exception ex)
                {
                    Logger.Error("webView跳转失败：", ex);
                }
            }
            else if (input.Contains("explore"))
            {
                //这种包含两种情况，1：直接输入首页网址，这种情况不判断，2：带id的取出id，这种满足跳转条件
                string baseUrl = "https://www.xiaohongshu.com/explore";
                var id = GetId(input,baseUrl);
                if (string.IsNullOrEmpty(id)) { return; }
                else
                {
                    isSubscribeEvent = true;
                }
            }
            ////非URL默认识别为关键字搜索
            //else if (!IsUrl(input))
            //{
            //    //RedirectService<HomeExploreViewModel>.SetJumpParam(input, _serviceProvider, _pageServiceNew);
            //    //_navigation.Navigate(typeof(Views.Pages.HomeExplorePage));
            //    isSubscribeEvent = true;
            //    url = $"https://www.xiaohongshu.com/search_result/?keyword={input}&source=web_explore_feed";
            //}
            if (isSubscribeEvent)
            {
                GlobalCaChe.webView.CoreWebView2.Navigate(url);
                //订阅事件
                _aggregator.GetEvent<NavigationCompletedEvent>().Subscribe(Navigation);
            }
        }

        private static void Navigation(RedirectInfo redirectInfo)
        {
            if (redirectInfo.Url.Contains("user/profile"))
            {
                RedirectService<UserProfileViewModel>.SetJumpParam(redirectInfo.Url, _serviceProvider, _pageServiceNew);
                _navigation.Navigate(typeof(Views.Pages.UserProfilePage));
            }
            //非URL默认识别为关键字搜索
            else if (redirectInfo.Url.Contains("keyword"))
            { 
                var keyword = Regex.Match(redirectInfo.Url, "(?<=keyword=).*?(?=&source)").Value;
                RedirectService<HomeExploreViewModel>.SetJumpParam(keyword, _serviceProvider, _pageServiceNew);
                _navigation.Navigate(typeof(Views.Pages.HomeExplorePage));
            }
            else if (redirectInfo.Url.Contains("explore"))
            {
                RedirectService<NodeDetailViewModel>.SetJumpParam(redirectInfo.Url, _serviceProvider, _pageServiceNew);
                _navigation.Navigate(typeof(Views.Pages.NodeDetailPage));
            }
            //消事件注册
            _aggregator.GetEvent<NavigationCompletedEvent>().Unsubscribe(Navigation);
        }
        /// <summary>
        /// 从url中获取id
        /// </summary>
        /// <param name="input"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static string GetId(string input, string baseUrl)
        {
            if (!IsUrl(input)) { return ""; }

            string url = EnableHttps(input);
            url = DeleteUrlParam(url);
            return url.Replace(baseUrl, "");
        }

        /// <summary>
        /// 是否为网址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsUrl(string input)
        {
            return input.StartsWith("http://") || input.StartsWith("https://");
        }

        /// <summary>
        /// 将http转为https
        /// </summary>
        /// <returns></returns>
        private static string EnableHttps(string url)
        {
            if (!IsUrl(url)) { return null; }

            return url.Replace("http://", "https://");
        }

        /// <summary>
        /// 去除url中的参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string DeleteUrlParam(string url)
        {
            string[] strList = url.Split('?');

            return strList[0].EndsWith("/") ? strList[0].TrimEnd('/') : strList[0];
        }
    }
}
