using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OpenQA.Selenium.Internal;

namespace WebGrabber
{
    public abstract class BaseTask : Entity
    {
        public Task Task { private set; get; }

        public Exception Exception { private set; get; }

        public BaseTask()
        {
            _CurrentState = State.Created;
        }

        public virtual Boolean IsPrepared()
        {
            return true;
        }

        #region Run

        protected abstract void OnRun();

        public void Run()
        {
            InitTask();

            Task.RunSynchronously();
        }

        public void AsyncRun()
        {
            InitTask();

            Task.Start();
        }

        private void InitTask()
        {
            ChangeState(State.Running);

            Task = new Task(TaskBody);
        }

        private void TaskBody()
        {
            try
            {
                OnBegin();

                OnRun();

                OnSuccess();
            }
            catch (WebOperationException exception)
            {
                OnFailure(exception, exception.Level);
            }
            catch (Exception exception)
            {
                OnFailure(exception);
            }
            finally
            {
                OnComplete();
            }
        }

        protected virtual void OnBegin()
        {
            ChangeState(State.Runned);

            "Задача [{0}] запущена".Set(Name).Log();
        }

        protected virtual void OnSuccess()
        {
            ChangeState(State.Success);

            "Задача [{0}] успешно завершена"
                .Set(Name)
                .Log(Log.Type.Information, Log.Level.Lowered);
        }

        protected virtual void OnFailure(Exception exception, Log.Level level = Log.Level.Increased)
        {
            Exception = exception;

            "Задача [{0}] заверешна с ошибкой"
                .Set(Name)
                .Log(exception, level);

            ChangeState(State.Failed);
        }

        protected virtual void OnComplete()
        {
            "Задача [{0}] завершена"
                .Set(Name)
                .Log(Log.Type.Information, Log.Level.Middle);

            ChangeState(State.Complete);
        }

        #endregion
        
        #region State

        private State _CurrentState;

        public State CurrentState
        {
            get { return _CurrentState; }
        }

        public delegate void StateChangedHandler(BaseTask task, State state);

        public event StateChangedHandler StateChanged;

        private void ChangeState(State state)
        {
            _CurrentState = state;

            RaiseStateChanged(state);
        }

        private void RaiseStateChanged(State state)
        {
            var handler = StateChanged;

            if (handler.IsNotNull())
            {
                StateChanged(this, state);
            }
        }

        public enum State
        {
            Created,

            Running,

            Runned,

            Success,

            Failed,

            Complete,
        }

        #endregion
    }
}
