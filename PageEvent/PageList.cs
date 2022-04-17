using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatusLib.PageEvent
{
    public class PageList
    {
        public class Page
        {
            public string PageName { get; set; }            //화면명
            public string Title { get; set; }               //화면제목
            public bool ShowBackButton { get; set; }        //뒤로가기 버튼 표기여부
            public bool ShowNextButton { get; set; }        //다음으로가기 버튼 표기여부
            public Dictionary<string, string> LastQueryParam { get; set; }  //마지막으로 저장된 쿼리 매개변수
        }

        public static List<Page> pageList = new List<Page>();      //화면 정보 담는 리스트
        public static Stack<Page> stackPage = new Stack<Page>();    //뒤로가기 시 사용하는 Stack

        /// <summary>
        /// 화면정보 마스터, 화면 추가시마다 메인 프로젝트에 PageList 추가해줘야함
        /// </summary>
        public PageList()
        {
            if (pageList.Count > 0) return;  
        }

        public PageList(string pageName, string title, bool showBackButton, bool showNextButton)
        {
            pageList.Add(new Page { PageName = pageName, Title = title, ShowBackButton = showBackButton, ShowNextButton = showNextButton });
        }

        /// <summary>
        /// 화면명으로 Page 클래스에 등록된 페이지 찾기
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <returns>Page클래스에 등록된 화면</returns>
        private Page GetPageList(string pageName)
        {
            Page list = pageList.Find(x => x.PageName == pageName);
            return list;
        }

        private string GetCurrentPageName(string pageName)
        {
            Page list = pageList.Find(x => x.PageName == pageName);
            return list.PageName;
        }

        /// <summary>
        /// 화면명과 매개변수로 화면제목 가져오기
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <param name="paramValue">화면 매개변수</param>
        /// <returns>화면제목</returns>
        public string GetPageTitle(string pageName)
        {
            Page list = GetPageList(pageName);       
            return list.Title; 
        }

        /// <summary>
        /// 화면제목 설정하기
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <param name="paramValue">화면 매개변수</param>
        public void SetPageTitle(string pageName, string pageTitle)
        {
            pageList.Find(x => x.PageName == pageName).Title = pageTitle;
        }

        /// <summary>
        /// 화면 내 뒤로가기 버튼 표기여부 
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <returns>뒤로가기 버튼 표기 여부</returns>
        public bool GetShowBackButton(string pageName)
        {
            Page list = GetPageList(pageName);
            return list.ShowBackButton;
        }

        /// <summary>
        /// 화면 내 다음으로 가기 버튼 표기여부 
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <returns>다음으로 가기 버튼 표기 여부</returns>
        public bool GetShowNextButton(string pageName)
        {
            Page list = GetPageList(pageName);
            return list.ShowNextButton;
        }

        /// <summary>
        /// 화면 내 마지막으로 저장된 쿼리 매개변수 가져오기
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <returns>마지막으로 저장된 쿼리 매개변수</returns>
        public Dictionary<string, string> GetPageLastQueryParam(string pageName)
        {
            Page list = GetPageList(pageName);
            return list.LastQueryParam;
        }

        /// <summary>
        /// 화면 내 마지막으로 선택된 쿼리 매개변수 설정
        /// </summary>
        /// <param name="pageName">화면명</param>
        /// <param name="paramValue">쿼리 매개변수</param>
        public void SetPageLastQueryParam(string pageName, Dictionary<string, string> paramValue)
        {
            pageList.Find(x => x.PageName == pageName).LastQueryParam = paramValue;
        }

        /// <summary>
        /// 뒤로가기에 사용할 화면정보 Stack에 Push
        /// </summary>
        /// <param name="pageName">화면명</param>
        public void PushPage(string pageName)
        {
            Page list = GetPageList(pageName);
            stackPage.Push(list);
        }

        /// <summary>
        /// 뒤로가기 버튼 클릭 시 Stack에 저장된 내용 Pop
        /// </summary>
        /// <returns>화면정보</returns>
        public Page PopPage()
        {
            return stackPage.Pop();
        }
    }
}
