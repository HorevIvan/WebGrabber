using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebGrabber.Tasks;

namespace WebGrabber
{
    public abstract class WebTask : EmbeddedTask
    {
        public IWebDriver Driver { private set; get; }

        public static String DefaultUrl 
        {
            get { return "data:,"; }
        }

        public static String StartUrl
        {
            get { return "data:,Start"; }
        }

        protected Boolean WebAction(Action action, Int32 sleepBefore = 0, Int32 sleepAfter = 0, Int32 repeatCount = 1, Int32 failureSleep = 800)
        {
            var identifier = Guid.NewGuid();

            for (var iteration = 0; iteration < repeatCount; iteration++)
            {
                Thread.Sleep(sleepBefore);

                try
                {
                    //"Запуск №{0} действия [{1}]".Set(iteration, identifier).Log();

                    action();

                    //"Действие [{1}] успешно завершено".Set(iteration, identifier).Log();

                    Thread.Sleep(sleepAfter);

                    return true;
                }
                catch//(Exception exception)
                {
                    //"Действия [{1}] завершилось неудачно: [{2}]"
                    //    .Set(iteration, identifier, exception.Message)
                    //    .Log(Log.Type.Warning, Log.Level.Debug);

                    Thread.Sleep(failureSleep);
                }
            }

            "Действиe завершилось неудачно после [{0}] попыток"
                .Set(repeatCount)
                .Log(Log.Type.Error, Log.Level.Increased);

            return false;
        }

        public override Boolean IsPrepared()
        {
            InitDriver();

            return Driver.IsNotNull();
        }

        private void InitDriver()
        {
            Driver = GetFreeDriver();

            if (Driver.IsNotNull())
            {
                GoToStartUrl();
            }
        }

        protected override void OnComplete()
        {
            RetrieveDriver();

            base.OnComplete();
        }

        private void RetrieveDriver()
        {
            GoToDefaultUrl();

            Driver = null;
        }

        private void GoToDefaultUrl()
        {
            Driver.Navigate().GoToUrl(DefaultUrl);
        }

        private void GoToStartUrl()
        {
            Driver.Navigate().GoToUrl(StartUrl);
        }

        private IWebDriver GetFreeDriver()
        {
            return Container.Drivers.FirstOrDefault(d => d.Url == DefaultUrl);
        }
    }
}
