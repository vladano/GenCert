using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CrashReporterWPF.Net
{
    /// <summary>
    /// ReportEx
    /// </summary>
    public class ReportEx
    {
        /// <summary>
        /// Reads from XML file.
        /// </summary>
        /// <param name="pFileName">Name of the p file.</param>
        /// <returns></returns>
       public static ReportEx ReadFromXMLFile(string pFileName)
        {
            try
            {
                ReportEx rito = new ReportEx();
                if (File.Exists(pFileName))
                {
                    using (XmlReader reader = XmlReader.Create(pFileName))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(ReportEx));
                        rito = (ReportEx)deserializer.Deserialize(reader);
                        reader.Close();
                    }
                }
                else
                {
                    throw new FileNotFoundException(pFileName);
                }
                return (rito);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

       /// <summary>
       /// Saves to XML file.
       /// </summary>
       /// <param name="pFileName">Name of the p file.</param>
        public void SaveToXMLFile(string pFileName)
        {
            try
            {
                XmlWriterSettings xstt = new XmlWriterSettings()
                {
                    ConformanceLevel = ConformanceLevel.Document,
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = new string(' ', 4),
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.None,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = false
                };

                using (XmlWriter writer = XmlWriter.Create(pFileName, xstt))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ReportEx));
                    serializer.Serialize(writer, this);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        
        private string mMessageForUser;
        /// <summary>
        /// Gets or sets the Property MessageForUser.
        /// </summary>
        /// <value>The Property MessageForUser.</value>
        public string MessageForUser
        {
            get { return mMessageForUser; }
            set { mMessageForUser = value; }
        }

              
        private string mWindows_Version;
        /// <summary>
        /// Gets or sets the Property Windows_Version.
        /// </summary>
        /// <value>The Property Windows_Version.</value>
        public string Windows_Version
        {
            get { return mWindows_Version; }
            set { mWindows_Version = value; }
        }
        private string mError_Message;
        /// <summary>
        /// Gets or sets the Property Error_Message.
        /// </summary>
        /// <value>The Property Error_Message.</value>
        public string Error_Message
        {
            get { return mError_Message; }
            set { mError_Message = value; }
        }
        private string mInnerException;
        /// <summary>
        /// Gets or sets the Property InnerException.
        /// </summary>
        /// <value>The Property InnerException.</value>
        public string InnerException
        {
            get { return mInnerException; }
            set { mInnerException = value; }
        }


        private string mApplicationVersione;
        /// <summary>
        /// Gets or sets the Property ApplicationVersione.
        /// </summary>
        /// <value>The Property ApplicationVersione.</value>
        public string ApplicationVersione
        {
            get { return mApplicationVersione; }
            set { mApplicationVersione = value; }
        }
        private string mApplicationName;
        /// <summary>
        /// Gets or sets the Property ApplicationName.
        /// </summary>
        /// <value>The Property ApplicationName.</value>
        public string ApplicationName
        {
            get { return mApplicationName; }
            set { mApplicationName = value; }
        }


        private DateTime mDataEx;
        /// <summary>
        /// Gets or sets the Property DataEx.
        /// </summary>
        /// <value>The Property DataEx.</value>
        public DateTime DataEx
        {
            get { return mDataEx; }
            set { mDataEx = value; }
        }
        private string mSource;
        /// <summary>
        /// Gets or sets the Property Source.
        /// </summary>
        /// <value>The Property Source.</value>
        public string Source
        {
            get { return mSource; }
            set { mSource = value; }
        }
        private string mStack_Trace;
        /// <summary>
        /// Gets or sets the Property Stack_Trace.
        /// </summary>
        /// <value>The Property Stack_Trace.</value>
        public string Stack_Trace
        {
            get { return mStack_Trace; }
            set { mStack_Trace = value; }
        }


        private string mImagePath;
        /// <summary>
        /// Gets or sets the Property ImagePath.
        /// </summary>
        /// <value>The Property ImagePath.</value>
        [XmlIgnore]
        public string ImagePath
        {
            get { return mImagePath; }
            set { mImagePath = value; }
        }
        private string mException_Type;
        /// <summary>
        /// Gets or sets the Property Exception_Type.
        /// </summary>
        /// <value>The Property Exception_Type.</value>
        public string Exception_Type
        {
            get { return mException_Type; }
            set { mException_Type = value; }
        }
    }
}
