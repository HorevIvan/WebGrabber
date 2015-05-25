using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebGrabber.Tasks;

namespace WebGrabber
{
    public class TasksContainer : BaseTask, IDisposable
    {
        public TimeSpan MaxTaskWaiting { set; get; }

        public ConcurrentDictionary<String, Object> Values { private set; get; }

        public TasksContainer(Int32 threadsCount = 5)
        {
            MaxTaskWaiting = TimeSpan.FromSeconds(30);

            Drivers = GetDrivers(threadsCount);

            Tasks = new List<EmbeddedTask>();

            Values = new ConcurrentDictionary<String, Object>();
        }

        #region Driver

        public  IEnumerable<IWebDriver> Drivers { private set; get; }

        private IWebDriver GetDriver()
        {
            return new ChromeDriver(); //TODO to settings
        }

        private IEnumerable<IWebDriver> GetDrivers(Int32 threadsCount)
        {
            var drivers =  new IWebDriver[threadsCount];

            for (var index = 0; index < threadsCount; index++)
            {
                drivers[index] = GetDriver();
            }

            return drivers;
        }

        #endregion

        #region Tasks

        private List<EmbeddedTask> Tasks;

        private readonly Object TasksLocker = new Object();

        public void AddTask(EmbeddedTask task)
        {
            task.Container = this;

            lock (TasksLocker)
            {
                Tasks.Add(task);
            }
        }

        public EmbeddedTask GetTask()
        {
            EmbeddedTask task = null;

            lock (TasksLocker)
            {
                if (Tasks.Any())
                {
                    task = Tasks[0];

                    Tasks.RemoveAt(0);
                }
            }

            return task;
        }

        #endregion

        public void Dispose()
        {
            Drivers.Loop(d => d.Dispose());
        }

        #region OnRun
        
        protected override void OnRun()
        {
            var lastRun = DateTime.Now;

            while (DateTime.Now - lastRun < MaxTaskWaiting)
            {
                var task = GetTask();

                if (task.IsNotNull())
                {
                    if (task.IsPrepared())
                    {
                        task.AsyncRun();

                        lastRun = DateTime.Now;
                    }
                    else
                    {
                        AddTask(task);
                    }

                    Thread.Sleep(100); //TODO to settings or property
                }
                else
                {
                    Thread.Sleep(500); //TODO to settings or property
                }
            }
        }

        #endregion
    }
}
