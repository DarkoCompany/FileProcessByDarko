using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessService
{
    public class ProcessConfigurations
    {
        private readonly string m_FromDir = "";
        private readonly string m_ToDir = "";

        public string FromDir
        {
            get { return m_FromDir; }
        }

        public string ToDir
        {
            get { return m_ToDir; }
        }

        public ProcessConfigurations(string[] args)
        {
            if (args.Length == 3)
            {
                m_FromDir = args[1];
                m_ToDir = args[2];
            }
            else
            {
                m_FromDir = ConfigurationManager.AppSettings["FromDir"].ToString();
                m_ToDir = ConfigurationManager.AppSettings["ToDir"].ToString();
            }
        }
    }
}
