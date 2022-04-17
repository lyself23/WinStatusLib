using System;
using System.Collections.Generic;
using System.Linq;
using WinStatusLib.PageEvent;
using static WinStatusLib.PageEvent.PageList;

namespace WinStatusLib.PageEvent
{
    public delegate void PageEventHandler(object sender, PageEventArgs e);
    public delegate void TimerEventFiredDelegate();

    public class PageEventArgs : EventArgs
    {
        public string PageName { get; set; }                    //화면명
        public string Title { get; set; }                       //화면제목
        public bool ShowBackButton { get; set; }                //뒤로가기 버튼 표기 여부
        public bool ShowNextButton { get; set; }                //다음으로가기 버튼 표기 여부
        public Dictionary<string, string> Param { get; set; }   //마지막으로 저장된 매개변수
        public PageEventArgs()
        {

        }
    }

    public class PageEvent
    {
        public event PageEventHandler PageMove;

        PageList page = new PageList();
        PageEventArgs args = new PageEventArgs();

        public void ChangePageTitle(string pageName, string pageTitle)
        {
            page.SetPageTitle(pageName, pageTitle);
        }

        /// <summary>
        /// 화면 이동
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <param name="lastQueryParam">마지막으로 등록된쿼리 매개변수</param>
        public void MovePage(string pageName, Dictionary<string, string> lastQueryParam)
        {
            string paramVal = lastQueryParam == null ? "" : lastQueryParam.ElementAt(0).Value;
            page.SetPageLastQueryParam(pageName, lastQueryParam);

            if (PageMove != null)
            {
                args.PageName = pageName;
                args.Title = page.GetPageTitle(pageName);
                args.ShowBackButton = page.GetShowBackButton(pageName);
                args.ShowNextButton = page.GetShowNextButton(pageName);
                args.Param = page.GetPageLastQueryParam(pageName);

                PageMove(this, args);
            }
        }

        /// <summary>
        /// 뒤로가기 위한 화면정보 Stack에 Push
        /// </summary>
        /// <param name="pageName">화면명</param>
        public void PushPage(string pageName)
        {
            page.PushPage(pageName);
        }

        /// <summary>
        /// 이전 화면으로 이동
        /// </summary>
        public void PreviousPage()
        {
            if (PageMove != null)
            {
                Page list = page.PopPage();
                args.PageName = list.PageName;
                args.Title = list.Title;
                args.ShowBackButton = list.ShowBackButton;
                args.ShowNextButton = list.ShowNextButton;
                args.Param = list.LastQueryParam;

                PageMove(this, args);
            }
        }
    }
}
