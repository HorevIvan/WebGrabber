using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WebGrabber.Tasks;

namespace WebGrabber.Launcher
{
    /// <summary> http://rabota.e1.ru/
    /// </summary>
    public static class Rabota_E1_ru
    {
        private static void WaitingLoad(IWebDriver driver)
        {
            "Поиск индикатора загрузки данных".Log();

            var div = driver.FindElement(By.CssSelector("div.ra-elements-list__preloader.js-list__preloader"));

            if (div.IsNotNull())
            {
                "Индикатора загрузки данных найден".Log();

                if (div.Displayed.Not())
                {
                    "Загрузка данных завершена".Log();

                    return;
                }

                "Ожидание загрузки данных...".Log();
            }
        }

        public class Container : TasksContainer
        {
            public Container()
            {
                Name = "Поиск вакансий";

                AddTask(new PageEnumerationTask());
            }
        }

        public class PageEnumerationTask : WebTask
        {
            public String StartUrl { get; set; }

            public String PageUrl { set; get; }

            public PageEnumerationTask()
            {
                StartUrl = "http://rabota.e1.ru/";

                Name = "Поиск всех страниц с вакансиями на [{0}]".Set(StartUrl);
            }

            protected override void OnRun()
            {
                if (WebAction(GoToStart, sleepBefore: 100, sleepAfter: 1000, repeatCount: 5))
                {
                    var nextPageUrl = GetNext();

                    if (nextPageUrl.IsNull())
                    {
                        "Эта завершающая страница: {0}".Set(Driver.Url).Log();
                    }
                    else
                    {
                        Container.AddTask(new PageEnumerationTask { PageUrl = nextPageUrl });

                        "Страница добавлена для разбора: {0}".Set(Driver.Url).Log(level: Log.Level.Lowered);

                        Container.AddTask(new ParsePageTask { PageUrl = nextPageUrl });

                        "Добавлена страница для перехода: {0}".Set(Driver.Url).Log(level: Log.Level.Lowered);
                    }
                }
                else
                {
                    "Не удалось перейти на стартовую страницу".ThrowWebOperationException(Log.Level.Hight);
                }
            }

            public void GoToStart()
            {
                if (PageUrl.IsNull())
                {
                    "Переход на главную cтраницу [{0}]".Set(StartUrl).Log();

                    Driver.Navigate().GoToUrl(StartUrl);

                    "Поиск ссылки на все вакансии".Log();

                    var a = Driver.FindElement(By.CssSelector(".ra-header-logo-nav-section-title a"));

                    if (a.IsNotNull())
                    {
                        "Переход по ссылке [{0}]".Set(a.Text).Log();

                        a.Click();
                    }
                }
                else
                {
                    "Переход на страницу [{0}]".Set(PageUrl).Log();

                    Driver.Navigate().GoToUrl(PageUrl);
                }
            }

            public String GetNext()
            {
                String nextPageUrl = null;

                PageUrl = Driver.Url;

                if (WebAction(WaitingLoad, sleepBefore: 1000, sleepAfter: 200, repeatCount: 2).Not())
                {
                    "Не удалось дождаться полной загрузки страницы".ThrowWebOperationException(Log.Level.Hight);
                }

                "Поиск элемента с указанием текущей страницы".Log();

                var span = Driver.FindElement(By.CssSelector("li.ra-pagination-pages-item.ra-pagination-item.active span"));

                if (span.IsNotNull())
                {
                    "Элемент с текущей страницей найден".Log();

                    var currentPageNumber = Int32.Parse(span.Text);

                    "Текущая страница [{0}]".Set(currentPageNumber).Log();

                    "Поиск элемента со следующей страницей".Log();

                    var ul = span.Parent().Parent();

                    var lis = ul.FindElements(By.TagName("li"));

                    foreach (var li in lis)
                    {
                        if (li.Text == (currentPageNumber + 1).ToString())
                        {
                            var a = li.FindElement(By.TagName("a"));

                            if (a.IsNotNull())
                            {
                                "Элемент со следующей страницей найден".Log();

                                nextPageUrl = a.GetAttribute("href");
                            }
                        }
                    }
                }
                else
                {
                    "Найдена последняя страница".Log(Log.Type.Information, Log.Level.Increased);
                }

                return nextPageUrl;
            }

            public void WaitingLoad()
            {
                Rabota_E1_ru.WaitingLoad(Driver);
            }
        }

        public class ParsePageTask : WebTask
        {
            public String PageUrl { set; get; }

            protected override void OnRun()
            {
                "Переход на страницу [{0}]".Set(PageUrl).Log();

                if (WebAction(GoToPage, sleepBefore: 100, sleepAfter: 200, repeatCount: 5, failureSleep: 1000))
                {
                    "Переход на страницу осуществлён".Set(PageUrl).Log();

                    if (WebAction(WaitingLoad, sleepBefore: 1000, sleepAfter: 200, repeatCount: 2).Not())
                    {
                        "Не удалось дождаться полной загрузки страницы".ThrowWebOperationException(Log.Level.Hight);
                    }

                    "Поиск элементов с вакансиями".Log();

                    var divs = Driver.FindElements(By.CssSelector("div.ra-elements-list__item"));

                    "Найдено [{0}] вакансий на странице".Set(divs.Count).Log(Log.Type.Information, Log.Level.Lowered);

                    SaveToTask(ParseVacancies(divs));
                }
                else
                {
                    "Переход на страницу не удался [{0}]".Set(PageUrl).ThrowWebOperationException(Log.Level.Hight);
                }

            }

            private IEnumerable<Vacancy> ParseVacancies(IEnumerable<IWebElement> divs)
            {
                "Разбор вакансий".Log();

                var vacancies = new List<Vacancy>();

                divs.Loop(div => vacancies.Add(ParseVacancy(div)));

                "Вакансии разобраны".Log();

                return vacancies;
            }

            private Vacancy ParseVacancy(IWebElement div)
            {
                var id = div.GetAttribute("data-id");

                "Разбор вакансии [{0}]".Set(id).Log();

                var title = div.FindElement(By.TagName("h3")).Text;

                var money = div.FindElement(By.CssSelector("div.ra-elements-list__pay")).Text;

                var company = div.FindElement(By.CssSelector("div.ra-elements-list__subtitle a")).Text;

                var companyPage = div.FindElement(By.CssSelector("div.ra-elements-list__subtitle a")).GetAttribute("href");

                var location = div.FindElement(By.CssSelector("div.ra-elements-list__brief")).Text;

                div.FindElement(By.TagName("h3")).Click();

                "Раскрытие вакансии [{0}]".Set(id, title).Log();

                WebAction(() => WaitDescriptionOpen(div), 100, 300, 5, 1000);
                
                var description = div.FindElement(By.CssSelector("div.ra-elements-list__info")).Text;

                var contacts = div.FindElement(By.CssSelector("div.ra-contacts")).Text;

                return new Vacancy
                {
                    ID = id,
                    Title = title,
                    Money = money,
                    Company = company,
                    CompanyPage = companyPage,
                    Location = location,
                    Description = description,
                    Contacts = contacts,
                    DateTimeParsed = DateTime.Now,
                };
            }

            #region Save

            private static Int32 SaveNumber;

            private void SaveToTask(IEnumerable<Vacancy> vacancies)
            {
                "Формирование задачи на сохрание вакансий".Log();

                var saveTask = new WriteVacanciesTask
                {
                    Name = "Сохрание вакансий №" + SaveNumber++,
                    Vacancies = vacancies
                };

                Container.AddTask(saveTask);
            }

            #endregion

            private void WaitDescriptionOpen(IWebElement div)
            {
                div.FindElement(By.CssSelector("div.ra-elements-list__info"));
            }

            private void GoToPage()
            {
                if (Driver.Url != PageUrl)
                {
                    Driver.Navigate().GoToUrl(PageUrl);
                }
            }

            public void WaitingLoad()
            {
                Rabota_E1_ru.WaitingLoad(Driver);
            }
        }

        public class WriteVacanciesTask : EmbeddedTask
        {
            public IEnumerable<Vacancy> Vacancies { set; get; }

            public String Path { private set; get; }

            private void InitPath()
            {
                const String pathKey = "SaveTask_PathKey";

                if (Container.Values.ContainsKey(pathKey).Not())
                {
                    Path = "Grab_rabora.e1.ru_{0:yyyyMMddHHmm}.html".Set(DateTime.Now);

                    Container.Values[pathKey] = Path;

                    const String header = "<style>td{vertical-align:top;border:1px solid silver;}</style><table>";

                    Write(header);
                }

                if (Path.IsEmpty())
                {
                    Path = Container.Values[pathKey].To<String>();
                }
            }

            protected override void OnRun()
            {
                InitPath();

                foreach (var vacancy in Vacancies)
                {
                    Write(vacancy.GetHtml());
                }
            }

            #region Write

            private static readonly Object WriteLocker = new Object();

            private void Write(String str)
            {
                lock (WriteLocker)
                {
                    using (var fileStream = new StreamWriter(Path, true, Encoding.Default))
                    {
                        fileStream.Write(str);
                    }
                }
            }

            #endregion

        }

        public class Vacancy
        {
            public String ID { get; set; }

            public String Title { get; set; }

            public String Money { get; set; }

            public String Company { get; set; }

            public String CompanyPage { get; set; }

            public String Location { get; set; }

            public String Description { get; set; }

            public String Contacts { get; set; }

            public DateTime DateTimeParsed { set; get; }

            public String GetHtml()
            {
                Func<String, String> toHtml = (str) => str.Replace("\n", "<br />");

                var htmlContacts = toHtml(Contacts);

                var htmlLocation = toHtml(Location);

                var htmlDescription = toHtml(Description);

                var builder = new StringBuilder();

                builder.AppendLine("<tr>");

                builder.AppendFormat("<td>{0}</td>", ID);

                builder.AppendFormat("<td>{0}</td>", Title);

                builder.AppendFormat("<td>{0}</td>", Money);

                builder.AppendFormat("<td><a href='{1}'>{0}</a><br/>{2}<br/>{3}</td>", Company, CompanyPage, htmlContacts, htmlLocation);

                builder.AppendFormat("<td>{0}</td>", htmlDescription);

                builder.AppendFormat("<td>{0:dd/HH:mm:ss.ff}</td>", DateTimeParsed);

                builder.AppendLine("</tr>");

                return builder.ToString();
            }
        }
    }
}
