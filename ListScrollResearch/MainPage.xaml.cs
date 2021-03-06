﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace ListScrollResearch
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ListPageMove_Click(object sender, RoutedEventArgs e)
        {
            int count = 30;
            if (string.IsNullOrEmpty(SettingTestCount.Text))
                count = 30;
            else
                count = Convert.ToInt32(SettingTestCount.Text);

            if (count < 0)
                count = 10;

            DateCollection._testCount = count;

            this.Frame.Navigate(typeof(ListResearch));
        }

        private void TestPageMove_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GridViewScrollTest));
        }
    }
}
