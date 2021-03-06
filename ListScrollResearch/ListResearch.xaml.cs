﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Newtonsoft.Json;
using ListScrollResearch.CustomControls;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace ListScrollResearch
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class ListResearch : Page
    {
        public ObservableCollection<DateGroup> DateTests { get; set; }
        public ObservableCollection<DateItem> NowRenderedList { get; set; } = new ObservableCollection<DateItem>();

        public List<ListViewHeaderItem> AllListViewHeaderItems { get; set; } = new List<ListViewHeaderItem>();
        public List<ListViewHeaderItem> DisplayedHeaderItems { get; set; } = new List<ListViewHeaderItem>();

        public ListResearch()
        {
            this.InitializeComponent();

            var ss = new IncrementalLoadingCollection<DateCollection, DateGroup>();
            DateTests = DateCollection._dateGroupObservable;
        }

        private void TestListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetTodayScroll();

            var _border = VisualTreeHelper.GetChild(TestListView, 0);                   // Border
            var _scrollViewer = VisualTreeHelper.GetChild(_border, 0) as ScrollViewer;  // ScrollViewer
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
        }

        public bool IsTopScrolled { get; set; } = false;
        public bool IsBottomScrolled { get; set; } = false;
        public bool IsUnlimitedData { get; set; } = false;

        /// <summary>
        /// ListView가 Scroll이 될 때의 동작 정의
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var nowScrollViewer = sender as ScrollViewer;
            var scrollableHeight = nowScrollViewer.ScrollableHeight;
            var verticalOffset = nowScrollViewer.VerticalOffset;

            if (scrollableHeight == 0)
                return;

            int scope = 50;
            var topScope = scope;                       // 이전의 자료를 가져오기 위한 범위
            var bottomScope = scrollableHeight - scope; // 이후의 자료를 가져오기 위한 범위

            // 해당 범위 안에 들어갔을 경우 데이터를 가져온다. 현재는 무제한으로.
            if (verticalOffset < topScope && !IsTopScrolled)
            {
                if (IsUnlimitedData)
                {
                    var newDateGroup = DateCollection.GetMoreItems(0, 10, DateTests, nextDirection: false);
                    DateTests.Insert(0, newDateGroup);
                    IsTopScrolled = true;
                }
            }
            else if (verticalOffset > bottomScope && !IsBottomScrolled)
            {
                if (IsUnlimitedData)
                {
                    var newDateGroup = DateCollection.GetMoreItems(DateTests.Count - 1, 10, DateTests, nextDirection: true);
                    DateTests.Add(newDateGroup);
                    IsBottomScrolled = true;
                }
            }

            // 맨 위로 가거나 맨 아래로 가면 다시 자료를 가져오게 만들어야 함
            if (verticalOffset < topScope && IsTopScrolled)
                IsTopScrolled = false;
            else if (verticalOffset > bottomScope && IsBottomScrolled)
                IsBottomScrolled = false;
        }

        /// <summary>
        /// 맨 처음 들어왔을 때 오늘 날짜로 세팅
        /// </summary>
        public void SetTodayScroll()
        {
            var now = DateTimeOffset.Now;
            foreach (var item in TestListView.Items)
            {
                var dateItem = item as DateItem;
                if (dateItem.Date.Date == now.Date)
                {
                    TestListView.SelectedItem = item;
                    TestListView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                    break;
                }
            }
        }

        /// <summary>
        /// 메인페이지로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// TestListView에서 Content가 추가로 로딩될 때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TestListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var ss = args.Handled;                          // 현재 ContainerContentChanging을 수동으로 조작할지 여부
            var ss1 = args.InRecycleQueue;                  // 표기 여부(재활용, 화면에 표기할지 숨길지)
            var ss2 = args.Item as DateItem;                // 실제 Data
            var ss3 = args.ItemContainer as ListViewItem;   // Data를 담고 있는 Container
            var ss4 = args.ItemIndex;                       // Data의 Index
            var ss5 = args.Phase;

            CheckRecycleRenderedList(ss1, ss2, ss3);        // Now Rendered list
            OnlyDisplayedHeaderItem();
        }

        /// <summary>
        /// 현재 Render 되어 있는 ListViewItem을 별도의 ListView에 표기
        /// </summary>
        /// <param name="isRecycle"></param>
        /// <param name="dateTest"></param>
        public void CheckRecycleRenderedList(bool isRecycle, DateItem dateTest, ListViewItem nowItem)
        {
            if (isRecycle)      // 다시 가상화 되는 상태
            {
                NowRenderedList.Remove(dateTest);
            }
            else
            {
                NowRenderedList.Add(dateTest);
            }

            NowCount.Text = "Now rendered items count : " + NowRenderedList.Count.ToString();

            // PointerWheelChagnedEvent 가 발생하지 않으므로 Container에서 검색해서 Index가 ItemSource의 Count와 같으면 맨 끝으로 간주
            var renderedLastItem = NowRenderedList.OrderBy(x => x.Name).LastOrDefault();
            var endItem = DateTests.LastOrDefault().LastOrDefault() as DateItem;
            if (renderedLastItem == endItem)
            {
                //AddNewData();
            }
        }

        /// <summary>
        /// 현재 화면에 보이고 있는 ListViewHeaderItem 표시
        /// </summary>
        public void OnlyDisplayedHeaderItem()
        {
            // 초기화
            AllListViewHeaderItems = new List<ListViewHeaderItem>();
            DisplayedHeaderItems = new List<ListViewHeaderItem>();
            ContentChangeTest.Text = "";

            var _border = VisualTreeHelper.GetChild(TestListView, 0);                   // Border
            var _scrollViewer = VisualTreeHelper.GetChild(_border, 0) as ScrollViewer;  // ScrollViewer

            var aaass = _scrollViewer.ScrollableHeight;

            // ScrollView의 자식 Component 찾기
            List<ListViewHeaderItem> childrenElements = new List<ListViewHeaderItem>();
            FindChildrenElementList(_scrollViewer, ref childrenElements);
            AllListViewHeaderItems = childrenElements;

            // ListViewHeaderItem의 상태 파악 후 표기할 항목 정리
            foreach (var item in AllListViewHeaderItems)
            {
                if (item.RenderTransform is MatrixTransform)
                {
                    DisplayedHeaderItems.Remove(item);
                }
                else if (item.RenderTransform is CompositeTransform)
                {
                    DisplayedHeaderItems.Add(item);
                }
            }

            ContentChangeTest.Text += "Now Top header : ";
            foreach (var item in DisplayedHeaderItems)
            {
                var visibleHeader = FindChildrendElement<TextBlock>(item);
                ContentChangeTest.Text += visibleHeader?.Text;
            }
        }

        /// <summary>
        /// 나중에 데이터 더 추가하기 테스트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddData_Click(object sender, RoutedEventArgs e)
        {
            DateGroup addedBeforeData = new DateGroup
            {
                Key = DateTime.Now.AddDays(-2).ToString(),
                GroupName = DateTime.Now.AddDays(-2).ToString(),
                GroupHeader = DateTime.Now.AddDays(-2)
            };
            DateGroup addedData = new DateGroup
            {
                Key = DateTime.Now.AddDays(2).ToString(),
                GroupName = DateTime.Now.AddDays(2).ToString(),
                GroupHeader = DateTime.Now.AddDays(2)
            };

            for (int i = 0; i < 100; i++)
            {
                addedData.Add(new DateItem() { Name = "new_test_" + i, Date = DateTime.Now.AddDays(1) });
                addedBeforeData.Add(new DateItem() { Name = "new_before_test_" + i, Date = DateTime.Now.AddDays(1) });
            }

            //DateCollection.TestCasesGroup.Add(addedData);
            //DateCollection.TestCasesGroup.Insert(0, addedBeforeData);
        }

        /// <summary>
        /// Parent에서 원하는 형식(T)의 자식 리스트 찾기
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public List<T> FindChildrenElementList<T>(DependencyObject parentObject, ref List<T> returnList) where T : FrameworkElement
        {
            // 자식 검색
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parentObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parentObject, i);
                if (child is T wantedTypeItem)
                {
                    // List에 포함되어 있지 않은 경우에만
                    if (!returnList.Contains(wantedTypeItem))
                    {
                        returnList.Add(wantedTypeItem);
                        return returnList;
                    }
                }

                // 재귀호출
                if (returnList.Count > 0)
                    break;
                else
                    FindChildrenElementList(child, ref returnList);
            }

            return returnList;
        }

        /// <summary>
        /// FrameWork Element에서 원하는 형식의 Child 찾아서 단일 항목 리턴
        /// </summary>
        /// <typeparam name="T">원하는 형식의 Framework element(ex. TextBlock)</typeparam>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public T FindChildrendElement<T>(DependencyObject parentObject) where T : FrameworkElement
        {
            T reValue = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parentObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parentObject, i);

                if (child is T wantedTypeItem)
                    return wantedTypeItem;

                // 재귀호출
                reValue = FindChildrendElement<T>(child);
                if (reValue != null)
                    break;
            }
            return reValue;
        }

        /// <summary>
        /// 헤더에 있는 내용 클릭 시 헤더 아이템 값 받아오기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListGroupHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ShowDpPropertyButton testButton)
            {
                var contentDialog = new ContentDialog();
                contentDialog.Title = "Selected date property value";
                contentDialog.CloseButtonText = "Close";
                contentDialog.Content = testButton.SelectedDate.ToString();
                await contentDialog.ShowAsync();
            }
        }

        private void EnableUnlimitedData_Click(object sender, RoutedEventArgs e)
        {
            if (IsUnlimitedData)
            {
                IsUnlimitedData = false;
                EnableUnlimitedData.Content = "Enable Unlimited data";
            }
            else
            {
                IsUnlimitedData = true;
                EnableUnlimitedData.Content = "Disable Unlimited data";
            }
        }
    }
}
