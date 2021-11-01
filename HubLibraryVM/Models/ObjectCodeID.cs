using BaseLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GosControls.Models
{
    public class ObjectCodeID : INotifyPropertyChanged
    {
        protected string oldCodeID = null;
        [XmlIgnore]
        public virtual string CodeID { get => _codeID; protected set { oldCodeID = _codeID; _codeID = value; CodeChanged(); PoolCodeID.CodeIDAdd(this); } }
        protected bool notChangeFile = false;

        protected virtual string TypeName { get => GetType().Name; }
        protected virtual string FilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected virtual string Folder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected virtual string FolderParent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [XmlIgnore]
        public string Message { get; set; }
        protected bool isNew { get => string.IsNullOrWhiteSpace(CodeID); }
        protected bool isXmlDesserialise = false;
        public ObjectCodeID()
        {
            //XmlInstance();
        }
        public string GetFilePath() => FilePath;
        public string GetFolder() => Folder;

        protected virtual void CodeChanged() => throw new NotImplementedException();

        protected void VerifyCodeID()
        {
            if (string.IsNullOrWhiteSpace(CodeID))
            {
                ChangeCodeID();
            }
        }
        public void ChangeCodeID()
        {
            if (isGetPropertyFromFile || isXmlProcess) return;
            char c = this is ProjectInfo ? 'P' : this is ExperimentInfo ? 'E' : this is WorkInfo ? 'W' : this is DiffractometerInfo ? 'D' : this is RadiationInfo ? 'R' : this is SampleInfo ? 'S' : '_';
            CodeID = c + Numbers.GenerateCodeID(10);
        }
        protected bool isGetPropertyFromFile = false;
        protected bool IsPropertiesUpdate = false;
        public virtual void GetPropertyFromFile(string file = null)
        {
            string path;
            if (file is null)
                path = FilePath;
            else
                path = file;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;
            bool correct = false;
            try
            {
                isGetPropertyFromFile = true;
                //TODO esta é a etapa que demora mais 53ms

                using (Stream reader = new FileStream(path, FileMode.Open))
                {
                    using (XmlReader xmlreader = new XmlTextReader(reader))
                    {
                        if (xml == null)
                            XmlInstance();
                        if (!xml.CanDeserialize(xmlreader))
                            return;
                        var temp = xml.Deserialize(xmlreader);
                        correct = true;
                        PropertyInfo[] properties = temp.GetType().GetProperties();
                        foreach (PropertyInfo p in properties)
                        {
                            //var trash = p.GetCustomAttribute(typeof(XmlIgnoreAttribute));
                            if (p.GetCustomAttribute(typeof(XmlIgnoreAttribute)) is null)
                                p.SetValue(this, p.GetValue(temp));
                        }

                    }
                }

            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                if (!correct)
                    if (File.Exists(path))
                        File.Delete(path);
                isGetPropertyFromFile = false;
            }
        }
        public virtual void WriteProperty(string file = null)
        {
            if (isGetPropertyFromFile || isXmlProcess) return;

            string path;
            if (file is null)
                path = FilePath;
            else
                path = file;

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(Folder))
            {
                if (!IsPropertiesUpdate) IsPropertiesUpdate = true;
                return;
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                { return; }
            }
            catch (Exception e)
            { return; }
            using (StreamWriter writer = new StreamWriter(path))//TODO fazer estes arquivos hidden
            {
                xml.Serialize(writer, this);
            }
            //countSize();
            //GetLastModified();
            //ChildChanged(this, "Size");
        }
        public virtual void DeleteObjectFiles() => throw new NotImplementedException();
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected static XmlSerializer xmlProject = new XmlSerializer(typeof(ProjectInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        protected static XmlSerializer xmlExperiment = new XmlSerializer(typeof(ExperimentInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) });
        protected static XmlSerializer xmlWork = new XmlSerializer(typeof(WorkInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) });
        protected static XmlSerializer xmlDiffractometer = new XmlSerializer(typeof(DiffractometerInfo), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) });
        protected static XmlSerializer xmlRadiation = new XmlSerializer(typeof(RadiationInfo), new Type[] { typeof(XRayLineInfo) });
        protected static XmlSerializer xmlSample = new XmlSerializer(typeof(SampleInfo));

        protected XmlSerializer xml;
        /// <summary>
        /// Usado para verificar se é possível trabalhar o CodeID ou não
        /// </summary>
        protected bool isXmlProcess { get => xml == null; }
        public void XmlInstance()
        {
            if (xml != null)
                return;

            if (this is ProjectInfo)
                xml = xmlProject;
            else if (this is ExperimentInfo)
                xml = xmlExperiment;
            else if (this is WorkInfo)
                xml = xmlWork;
            else if (this is DiffractometerInfo)
                xml = xmlDiffractometer;
            else if (this is RadiationInfo)
                xml = xmlRadiation;
            else if (this is SampleInfo)
                xml = xmlSample;
            else
                xml = new XmlSerializer(this.GetType());
        }

        protected string _codeID;
    }
}
