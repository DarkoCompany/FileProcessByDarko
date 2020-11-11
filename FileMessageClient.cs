using MsgLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessService
{
    public class FileMessageClient
    {
        private FileProcessor m_Parent = null;

        public FileMessageClient(FileProcessor parent)
        {
            m_Parent = parent;
        }

        public List<MsgBase> GetMessages()
        {
            List<MsgBase> results = new List<MsgBase>();
            string fileLocation = m_Parent.Configurations.FromDir;
            string fileDropLocation = m_Parent.Configurations.ToDir;
            string json = "";
            foreach (string fileName in Directory.GetFiles(fileLocation))
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    json = r.ReadToEnd();
                }

                string[] messages = json.Split(new string[] { ">>>>" }, StringSplitOptions.None);
                foreach (string msg in messages)
                {
                    results.Add(MsgBase.DecodeJSONMessage(msg));
                }

                File.Delete(fileName);
            }

            return results;
        }

        public void SendMessages(MsgBase msg)
        {
            try
            {
                if (Directory.Exists(m_Parent.Configurations.ToDir))
                {
                    string fileLocation = m_Parent.Configurations.ToDir + "\\" + msg.GetMessageType().ToString() + '_' + msg.Base_MsgUID + ".json";
                    File.WriteAllText(fileLocation, msg.EncodeToJSON());
                }
            }
            catch (Exception)
            { }
        }
    }
}
