using MsgLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessService
{
    public class FileProcessor : IDisposable
    {
        private AutoResetEvent shutdownEvent = new AutoResetEvent(false);
        private bool m_shutdownRequested = false;
        private DateTime m_LastCheckedTime = DateTime.MinValue;
        private CancellationTokenSource m_MsgQueueCancel = new CancellationTokenSource();
        private Task m_MsqQueueTask = null;
        private BlockingCollection<MsgBase> m_MsgQueue = new BlockingCollection<MsgBase>(new ConcurrentQueue<MsgBase>());
        private Thread m_Thread;

        private ProcessConfigurations m_Configurations;
        private FileMessageClient m_MessageClient;
        private ElectionManager m_ElectionManager;

        public ProcessConfigurations Configurations
        {
            get { return m_Configurations; }
        }

        public System.Threading.AutoResetEvent ShutdownEvent
        {
            get { return shutdownEvent; }
            set { shutdownEvent = value; }
        }

        public FileProcessor()
        {

        }

        public FileProcessor(string[] args)
        {
            m_Configurations = new ProcessConfigurations(args);
            m_ElectionManager = new ElectionManager();
            m_MsqQueueTask = new Task(ProcessMessages, m_MsgQueueCancel, TaskCreationOptions.LongRunning);
            m_MsqQueueTask.Start();

            m_Thread = new Thread(OnStart);
            m_Thread.Name = "My Worker Thread";
            m_Thread.IsBackground = true;
            m_Thread.Start();
        }

        protected void OnStart()
        {
            m_MessageClient = new FileMessageClient(this);
            m_shutdownRequested = false;

            //batch proccessing
            while (!m_shutdownRequested)
            {
                DateTime nowDT = DateTime.Now;
                if (nowDT.Subtract(m_LastCheckedTime).TotalSeconds > 10)
                {
                    CheckData();
                    m_LastCheckedTime = nowDT;
                }

                Thread.Sleep(1 * 1000);
            }
        }

        private void ProcessMessages(object o)
        {
            try
            {
                foreach (MsgBase msg in m_MsgQueue.GetConsumingEnumerable(m_MsgQueueCancel.Token))
                {
                    try
                    {
                        switch (msg.GetMessageType())
                        {
                            case MsgType.CandidateDetails:
                                m_ElectionManager.HandleCandidateDetails(msg);
                                break;
                            case MsgType.VotesDetails:
                                m_ElectionManager.HandleVotes(msg);
                                break;
                            case MsgType.ElectionResultsRequest:
                                foreach (MsgBase subMsg in m_ElectionManager.HandleResoultRequest(msg))
                                {
                                    m_MessageClient.SendMessages(subMsg);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                        //report this
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //Blocking callection cancelled
            }
        }

        private void CheckData()
        {
            foreach (MsgBase m in m_MessageClient.GetMessages())
                m_MsgQueue.Add(m);
        }

        protected void OnStop()
        {
            m_shutdownRequested = true;
            m_Thread = null;
        }

        public void Dispose()
        {
            OnStop();

            shutdownEvent.Set();
            GC.SuppressFinalize(this);
        }
    }
}
