using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessService
{
    public partial class StandardService : ServiceBase
    {
        FileProcessor m_FileProcessor;
        private string[] m_Args;

        static void Main(string[] args)
        {
            if (args.Contains("-p"))
            {
                using (FileProcessor mgr = new FileProcessor(args))
                    mgr.ShutdownEvent.WaitOne();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new StandardService(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        public StandardService(string[] args)
        {
            InitializeComponent();
            m_Args = args;
        }

        protected override void OnStart(string[] args)
        {
            m_FileProcessor = new FileProcessor(m_Args);
        }

        protected override void OnStop()
        {
            m_FileProcessor.Dispose();
        }
    }
}
