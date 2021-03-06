﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ListScrollResearch
{
    public class DateCollection : IIncrementalSource<DateGroup>
    {
        public static int _testCount { get; set; } = 30;

        public static List<DateGroup> _dateGroupList;

        public static ObservableCollection<DateGroup> _dateGroupObservable
        {
            get
            {
                var test = new ObservableCollection<DateGroup>();
                foreach (var item in _dateGroupList)
                {
                    test.Add(item);
                }
                return test;
            }
        }

        public DateCollection()
        {
            SetInitData();
        }

        public List<DateGroup> SetInitData()
        {
            _dateGroupList = new List<DateGroup>();

            DateGroup now = new DateGroup
            {
                Key = DateTime.Now.ToString(),
                GroupName = DateTime.Now.ToString(),
                GroupHeader = DateTime.Now
            };

            DateGroup before = new DateGroup
            {
                Key = DateTime.Now.AddDays(-1).ToString(),
                GroupName = DateTime.Now.AddDays(-1).ToString(),
                GroupHeader = DateTime.Now.AddDays(-1)
            };

            DateGroup after = new DateGroup
            {
                Key = DateTime.Now.AddDays(1).ToString(),
                GroupName = DateTime.Now.AddDays(1).ToString(),
                GroupHeader = DateTime.Now.AddDays(1)
            };

            for (int i = 0; i < _testCount; i++)
            {
                if (i < (_testCount / 3))
                {
                    before.Add(new DateItem() { Name = "test_" + i, Date = DateTime.Now.AddDays(-1) });
                }
                else if (i > (_testCount / 3 - 1) && i < (_testCount / 2))
                {
                    now.Add(new DateItem() { Name = "test_" + i, Date = DateTime.Now });
                }
                else
                {
                    after.Add(new DateItem() { Name = "test_" + i, Date = DateTime.Now.AddDays(1) });
                }
            }

            _dateGroupList.Add(before);
            _dateGroupList.Add(now);
            _dateGroupList.Add(after);

            return _dateGroupList;
        }

        /// <summary>
        /// 아이템 더 가져오기
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DateGroup>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var addedList = new List<DateGroup>();
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

            for (int i = 0; i < 10; i++)
            {
                addedData.Add(new DateItem() { Name = "new_test_" + i, Date = DateTime.Now.AddDays(1) });
                addedBeforeData.Add(new DateItem() { Name = "new_before_test_" + i, Date = DateTime.Now.AddDays(1) });
            }

            addedList.Add(addedData);
            addedList.Insert(0, addedBeforeData);

            // 추가되는 시간 설정(UX?)
            if (pageIndex == 0)
                await Task.Delay(500);
            else
                await Task.Delay(500);

            IEnumerable<DateGroup> addedEnumerable = addedList;
            return addedEnumerable;
        }

        /// <summary>
        /// 이전 또는 다음 자료 가져오기
        /// </summary>
        /// <param name="pageIndex">어디다가 넣을 것인지에 대한 위치 값</param>
        /// <param name="pageSize">한번에 가져올 양</param>
        /// <param name="nextDirection">이전 자료인지 다음자료인지 판별, 기본적으로 밑(다음) 값 가져오기</param>
        /// <returns></returns>
        public static DateGroup GetMoreItems
            (int pageIndex, int pageSize, ObservableCollection<DateGroup> dateGroups, bool nextDirection = true)
        {
            ObservableCollection<DateGroup> returnList = new ObservableCollection<DateGroup>();

            DateTimeOffset addedOffset = DateTimeOffset.Now;
            if (nextDirection)
            {
                var lastGroup = dateGroups.LastOrDefault();
                var lastItem = lastGroup.LastOrDefault() as DateItem;
                addedOffset = lastItem.Date.AddDays(1);
            }
            else
            {
                var firstGroup = dateGroups.FirstOrDefault();
                var firstItem = firstGroup.FirstOrDefault() as DateItem;
                addedOffset = firstItem.Date.AddDays(-1);
            }

            DateGroup addedGroup = new DateGroup
            {
                Key = addedOffset.Date.ToString(),
                GroupName = addedOffset.Date.ToString(),
                GroupHeader = addedOffset.Date
            };

            for (int i = 0; i < pageSize; i++)
            {
                addedGroup.Add(new DateItem() { Name = "new_test_" + i + "_" + addedOffset.Date.ToString("yyMMdd"), Date = addedOffset });
            }

            returnList.Add(addedGroup);

            return addedGroup;
        }
    }

    /*
     * [Data Structure]
     * DateGroup List<object>
	    - Key(*DateItem-Date와 일치해야 Group화 가능)
		    ○ DateItem
			    § Name
			    § Date(*Key와 일치해야 함)
            ○ DateItem
			    § Name
			    § Date(*Key와 일치해야 함)
            ○ DateItem
			    § Name
			    § Date(*Key와 일치해야 함)
        - Key(*DateItem-Date와 일치해야 Group화 가능)
		    ○ DateItem
			    § Name
			    § Date(*Key와 일치해야 함)
            ○ DateItem
			    § Name
			    § Date(*Key와 일치해야 함)
     */
    public class DateGroup : List<object>
    {
        public object Key { get; set; }
        public string GroupName { get; set; }
        public DateTimeOffset GroupHeader { get; set; }
    }
    public class DateItem
    {
        public DateTimeOffset Date { get; set; }
        public string Name { get; set; }
    }
}
