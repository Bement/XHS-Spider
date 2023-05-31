﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XHS.Common.Events;
using XHS.Common.Global;
using XHS.Common.Helpers;
using XHS.Common.Http;
using XHS.IService.XHS;
using XHS.Models.XHS.InputModel;
using XHS.Service.XHS;
using XHS.Spider.Helpers;

namespace XHS.Spider.Views.Windows
{
    /// <summary>
    /// WebViewContainer.xaml 的交互逻辑
    /// </summary>
    public partial class WebViewContainer : Window
    {
        private readonly IXhsSpiderService _xhsSpiderService;
        private IEventAggregator _aggregator { get; set; }
        public WebViewContainer(IEventAggregator aggregator, IXhsSpiderService xhsSpiderService)
        {
            _xhsSpiderService = xhsSpiderService;
            _aggregator = aggregator;
            InitializeComponent();
            InitializeAsync();
            this.webView.NavigationCompleted += WebView_NavigationCompleted;
            this.webView.SourceChanged += WebView_SourceChanged;
            this.webView.ContentLoading += WebView_ContentLoading;
           
        }
        private async void InitializeAsync()
        {
            GlobalCaChe.webView = this.webView;
            await webView.EnsureCoreWebView2Async(null);
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
        }

        private void CoreWebView2_DOMContentLoaded(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CoreWebView2_HistoryChanged(object? sender, object e)
        {
            //throw new NotImplementedException();
        }

        private void WebView_ContentLoading(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void WebView_SourceChanged(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void WebView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //TODO:加载完成，通知打开页面
        }

        private async void testSearch_click(object sender, RoutedEventArgs e)
        {
            string url = "/api/sns/web/v1/search/notes";
            SearchInputModel model = new SearchInputModel()
            {
                keyword = "美女",
                search_id = AlgorithmHelper.GetSearchId()
            };
            //var data = JsonConvert.SerializeObject(model);
            var apiResult = await _xhsSpiderService.SearchNotes(model);
            //var xsxt = await webView.CoreWebView2.ExecuteScriptAsync(jscode);
            //if (!string.IsNullOrEmpty(xsxt))
            //{
            //    var sign = JsonConvert.DeserializeObject<XsXt>(xsxt);
            //    HttpClientHelper.xs = sign.Xs;
            //    HttpClientHelper.xt = sign.Xt.ToString();
            //    string result = HttpClientHelper.DoPost(url + "\n", data);
            //    MessageBox.Show(result);
            //}
        }

        private void load(object sender, RoutedEventArgs e)
        {
           var scriptHost = ScriptHost.GetScriptHost(this.webView, _aggregator);
        }

        private void test_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
