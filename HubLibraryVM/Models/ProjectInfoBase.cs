using BaseLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GosControls.Models
{
    //[Mask(MaskAttribute.Alphanumeric, "Title")]
    public class ProjectInfoBase : ObjectToFolder
    {
        #region Properties
        //public string CodeID { get => codeID; set => codeID = value; }
        //[MaskAttribute("[A-Za-z0-9._ -]+")]
        //[Description("An object is only really created after writing a Title \nOnly characters alphanumeric, space, and ._-")]
        //[XmlIgnore]
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value)
                    return;
                //bool sameName = true;
                //int j = 2;
                //while (sameName)
                //{
                //    sameName = false;
                //    if (parent == null)
                //        for (int i = 0; i < PoolCodeID.CodeIDCount(); i++)
                //        {
                //            ProjectInfoBase temp = null;
                //            if (PoolCodeID.CodeIDGet(i) is ProjectInfoBase)
                //                temp = PoolCodeID.CodeIDGet(i) as ProjectInfoBase;
                //            if (temp != null)
                //                if (temp.GetParent() is null)
                //                    if (temp.ToString() == value)
                //                    {
                //                        sameName = true;
                //                        break;
                //                    }
                //        }
                //    else
                //        for (int i = 0; i < parent.Children.Count; i++)
                //        {
                //            ProjectInfoBase temp = parent.Children[i];
                //            if (temp.ToString() == value)
                //            {
                //                sameName = true;
                //                break;
                //            }
                //        }
                //    if (sameName)
                //    {
                //        if (!string.IsNullOrWhiteSpace(oldTitle))
                //            value = oldTitle;
                //        else
                //        {
                //            value = value + " (" + j + ")";
                //            j++;
                //        }
                //    }
                //}
                oldTitle = _title;
                _title = value.Trim();
                if (!isXmlProcess)
                    VerifyTitle(this);
                if (string.IsNullOrWhiteSpace(ToString()))
                {
                    _title = oldTitle;
                    return;
                }
                LastTitleChange = DateTime.Now;
                if (isNew && !isXmlProcess)
                    ChangeCodeID();
                WriteProperty();
                RaisePropertyChanged();
                ProjectNotification(new ProjectNotificationEventArgs(this) { TitleChanged = true });
            }
        }
        public DateTime LastTitleChange { get => _lastTitleChange; set => _lastTitleChange = value; }

        private string oldTitle = null;
        public string Description { get => _description; set { _description = value; WriteProperty(); RaisePropertyChanged(); } }
        [XmlIgnore]
        public string Type { get { if (string.IsNullOrWhiteSpace(_type)) return this.GetType().Name; return _type; } set { _type = value; } }

        #region Propriedades contadas internamente
        protected ObservableCollection<SampleInfo> samples = new();
        [XmlIgnore]
        public ObservableCollection<SampleInfo> Samples
        {
            get => samples;
            set
            {
                samples = value;
                RaisePropertyChanged();
            }
        }
        protected ObservableCollection<DiffractometerInfo> diffractometers = new();
        [XmlIgnore]
        public ObservableCollection<DiffractometerInfo> Diffractometers
        {
            get => diffractometers;
            set
            {
                diffractometers = value;
                RaisePropertyChanged();
            }
        }
        private int countExperiments;
        [XmlIgnore]
        public int CountExperiments
        {
            get => countExperiments;
            set
            {
                countExperiments = value;
                RaisePropertyChanged();
            }
        }
        private int countWorks;
        [XmlIgnore]
        public int CountWorks
        {
            get => countWorks;
            set
            {
                countWorks = value;
                RaisePropertyChanged();
            }
        }
        private double size;
        [XmlIgnore]
        public double Size
        {
            get => size;
            set
            {
                size = value;
                RaisePropertyChanged();
            }
        }
        [XmlIgnore]
        public DateTime LastModified { get => _lastModified; set { _lastModified = value; RaisePropertyChanged(); } }
        #endregion
        #endregion
        #region Constructors
        /// <summary>
        /// Apenas para xml
        /// </summary>
        protected ProjectInfoBase() : base() { }
        //protected ProjectInfoBase(string projectFolder) : base() { this.parent = null; base.SetParentFolder(projectFolder); Children.CollectionChanged += CollectionChanged; }
        protected ProjectInfoBase(ProjectInfoBase parent) : base() { this.parent = parent; if (parent != null) SetParentFolder(parent.GetFolder()); Children.CollectionChanged += CollectionChanged; }
        public ProjectInfoBase(ProjectInfoBase parent, string folder) : base()
        {
            this.parent = parent;

            if (this is ProjectInfo)
            {
                SetProjectFolder(Path.GetDirectoryName(folder) /*Directory.GetParent(folder).FullName*/);
            }
            InitialProcess(folder);
            //GetFolderInfos(folder);
            children = new();
            VerifyCodeID();
            GetChildren();
            GetLastModified();
            Children.CollectionChanged += CollectionChanged;
        }
        #endregion
        #region Relatives
        protected ProjectInfoBase parent;
        public ProjectInfoBase GetParent() => parent;
        public void SetParent(ProjectInfoBase value) => parent = value;
        private ObservableCollection<ProjectInfoBase> children = new();
        [XmlIgnore]
        public ObservableCollection<ProjectInfoBase> Children { get => children; set { children = value; RaisePropertyChanged(); } }
        public void GetChildren()
        {

            if (!Directory.Exists(Folder) || Children == null) return;
            if (Children.Count > 0)
                Children.Clear();
            string[] childrenFolders = Directory.GetDirectories(Folder);
            if (childrenFolders != null)
                foreach (string child in childrenFolders)
                {
                    if (!File.Exists(Path.Combine(child, "Properties.xml")))
                        continue;
                    if (this is ProjectInfo)
                        Children.Add(new ExperimentInfo((ProjectInfo)this, child));
                    else if (this is ExperimentInfo)
                        Children.Add(new WorkInfo((ExperimentInfo)this, child));
                }
            if (children.Count > 1)
                Collections.OrderObservable(Children);
        }
        public virtual void KillAllChildren()
        {
            if (Children is not null && Children.Count > 0)
                for (int i = 0; i < Children.Count; i++)
                    Children[i].KillAllChildren();
        }
        #endregion
        #region Folder
        protected override string Folder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CodeID) || string.IsNullOrWhiteSpace(FolderParent))
                    if (!string.IsNullOrWhiteSpace(_folder))
                        return _folder;
                    else
                        return null;
                return Path.Combine(FolderParent, CodeID);
            }
        }
        protected override string FolderParent { get { if (parent != null) return parent.GetFolder(); else if (this is ProjectInfo) return GetProjectFolder(); else return null; } }
        public virtual string GetProjectFolder()
        {
            return parent.GetProjectFolder();
        }
        public virtual void SetProjectFolder(string value) => parent.SetProjectFolder(value);
        public override string GetFolderParent() => parent is null ? GetProjectFolder() : parent.GetFolder();
        //public void FolderUpdate()
        //{
        //    if (this is ProjectInfo)
        //        folder = Path.Combine(GetProjectFolder(), Title);
        //    else if (this is ExperimentInfo)
        //        folder = Path.Combine(GetParent().GetFolder(), Title);
        //    else if (this is WorkInfo)
        //        folder = Path.Combine(GetParent().GetParent().GetFolder(), GetParent().Title, Title);
        //    fileInfo = Path.Combine(folder, "Properties.xml");
        //    if (Children != null)
        //        foreach (ProjectInfoBase child in Children)
        //            if (child is not null)
        //                child.FolderUpdate();
        //}
        //protected void GetFolderInfos(string folder)
        //{
        //    FileProcess.InfoFromFolderPath(folder, ref _title, ref this.folder);
        //    this.folder = folder;
        //    fileInfo = Path.Combine(this.folder, "Properties.xml");
        //}
        public int GetCountWorksStarted()
        {
            int result = 0;
            if (Children is not null)
            {
                foreach (ProjectInfoBase child in Children)
                    if (child is not null)
                        result += child.GetCountWorksStarted();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Folder) || !Directory.Exists(Folder))
                    return 0;
                if (File.Exists(Path.Combine(Folder, "WorkFile.sin")))
                    return 1;
            }
            return result;
        }
        protected override void CodeChanged()
        {
            base.CodeChanged();
            //ProjectNotification(new ProjectNotificationEventArgs(this) { TitleChanged = true });
            //VerifyCodeID();
            //if (IsPropertiesUpdate)
            //    WriteProperty();
        }
        protected override void FolderChanged()
        {
            //FolderUpdate();
        }
        #endregion
        #region Manager Properties

        public bool IsOrContains(ProjectInfoBase info)
        {
            if (info == this)
                return true;
            else if (Children is not null)
                foreach (ProjectInfoBase child in Children)
                    if (child.IsOrContains(info))
                        return true;
            return false;
        }
        public bool IsParent(ProjectInfoBase info)
        {
            if (parent is not null)
                if (parent == info)
                    return true;
                else if (parent.IsParent(info))
                    return true;
            return false;
        }
        private void countExperimentsMethod()
        {
            if (this is ProjectInfo)
                CountExperiments = Children.Count;
            else
                CountExperiments = 0;
            //ChildChanged(this,"CountExperiments");
        }
        private int countWorksMethod()
        {
            int result = 0;
            if (this is WorkInfo)
                result = 1;
            else if (Children != null)
                foreach (ProjectInfoBase child in Children)
                {
                    if (child is not null)
                        result += child.countWorksMethod(); //TODO fazer await
                }
            CountWorks = result;
            //ChildChanged(this,"CountWorks");
            return result;
        }
        private async Task<double> countSize()
        {
            double result = 0;
            if (File.Exists(FilePath))
                result += new FileInfo(FilePath).Length / 1000.0;
            if (Children is not null)
                foreach (ProjectInfoBase child in Children) //TODO verificar a possibilidade do foreach async
                {
                    if (child is not null)
                        result += child.countSize().Result;
                    //result += child.Size;
                }
            else
            {
                string[] fileText = Directory.GetFiles(Folder);
                foreach (string fileTxt in fileText) //TODO verificar a possibilidade do foreach async
                {
                    result += new FileInfo(fileTxt).Length / 1000.0;
                }
                string[] foldersText = Directory.GetDirectories(Folder);
                foreach (string folderTxt in foldersText)
                {
                    fileText = Directory.GetFiles(Folder);
                    foreach (string fileTxt in fileText) //TODO verificar a possibilidade do foreach async
                    {
                        result += new FileInfo(fileTxt).Length / 1000.0;
                    }
                }
            }
            Size = result;
            //ChildChanged(this,"Size");
            return result;
        }
        private void GetLastModified()
        {
            if (this is WorkInfo)
            {
                string path;
                if (parent is not null)
                    path = Path.Combine(GetFolder(), "Workfile.sin");
                else if (!string.IsNullOrEmpty(_folder))
                    path = Path.Combine(_folder, "Workfile.sin");
                else
                    return;
                if (File.Exists(path))
                    LastModified = File.GetLastWriteTime(path); //new FileInfo(Path.Combine(GetFolder(), "Workfile.sin")).LastWriteTime;
            }
            else if (Directory.Exists(GetFolder()))
                LastModified = Directory.GetLastWriteTime(GetFolder()); //new DirectoryInfo(GetFolder()).LastWriteTime;
        }
        //protected bool isGetPropertyFromFile = false;
        //protected static XmlSerializer xmlProject = new XmlSerializer(typeof(ProjectInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        //protected static XmlSerializer xmlExperiment = new XmlSerializer(typeof(ExperimentInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) });
        //protected static XmlSerializer xmlWork = new XmlSerializer(typeof(WorkInfo), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) });
        //public void GetPropertyFromFile()
        //{
        //    if (!File.Exists(fileInfo)) return;

        //    try
        //    {
        //        isGetPropertyFromFile = true;
        //        //TODO esta é a etapa que demora mais 53ms
        //        //XmlSerializer xml = new XmlSerializer(this.GetType(), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async

        //        XmlSerializer xml;
        //        if (this is ProjectInfo)
        //            xml = xmlProject;
        //        else if (this is ExperimentInfo)
        //            xml = xmlExperiment;
        //        else
        //            xml = xmlWork;

        //        using (Stream reader = new FileStream(fileInfo, FileMode.Open))
        //        {
        //            using (XmlReader xmlreader = new XmlTextReader(reader))
        //            {

        //                if (!xml.CanDeserialize(xmlreader)) return;
        //                var temp = xml.Deserialize(xmlreader);
        //                PropertyInfo[] properties = temp.GetType().GetProperties();
        //                foreach (PropertyInfo p in properties)
        //                    p.SetValue(this, p.GetValue(temp));

        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return;
        //    }
        //    finally
        //    {
        //        isGetPropertyFromFile = false;
        //    }
        //}
        //protected bool IsPropertiesUpdate = false;
        //private string oldTitle;
        //protected void WriteProperty([CallerMemberName] string propertyName = null)
        //{
        //    if (isGetPropertyFromFile) return;

        //    if (propertyName == "Title")
        //    {
        //        if (oldTitle != null && oldTitle == Title) { return; }
        //        if (string.IsNullOrWhiteSpace(Title)) { return; }
        //        string parentFolder;
        //        if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(folder))
        //        {
        //            if (this is ProjectInfo)
        //                parentFolder = GetProjectFolder();
        //            else if (this is ExperimentInfo)
        //            {
        //                parentFolder = Path.Combine(GetProjectFolder(), GetParent().Title);
        //            }
        //            else
        //            {
        //                parentFolder = Path.Combine(GetProjectFolder(), GetParent().GetParent().Title, GetParent().Title);
        //            }
        //        }
        //        else
        //            parentFolder = Directory.GetParent(folder).FullName;
        //        if (Directory.Exists(Path.Combine(parentFolder, Title)))
        //        {
        //            if (oldTitle == null || oldTitle.ToUpper() != Title.ToUpper())
        //            {
        //                string parentName = GetParent() != null ? " " + GetParent().Title : "";
        //                string parentType = GetParent() != null ? GetParent().GetType().Name : "Projects";
        //                if (parentType != "Projects") parentType.Remove(parentType.IndexOf("Info"), 4);
        //                string thisType = this.GetType().Name;
        //                thisType = thisType.Remove(thisType.IndexOf("Info"), 4);
        //                ProjectNotification(new ProjectNotificationEventArgs(this) { AlertMessage = string.Format("There is other {0} with the same title ({1}) in {2}{3}", thisType, Title, parentType, parentName) });
        //                return;
        //            }
        //            else
        //            {
        //                Directory.Move(Path.Combine(parentFolder, oldTitle), Path.Combine(parentFolder, oldTitle + "_Temp_99"));
        //                oldTitle = oldTitle + "_Temp_99";
        //            }
        //        }
        //        if (string.IsNullOrWhiteSpace(oldTitle))
        //            Directory.CreateDirectory(Path.Combine(parentFolder, Title));
        //        else
        //        {
        //            Directory.Move(Path.Combine(parentFolder, oldTitle), Path.Combine(parentFolder, Title));
        //        }
        //        FolderUpdate();
        //        ProjectNotification(new ProjectNotificationEventArgs(this) { TitleChanged = true });
        //        VerifyCodeID();
        //        if (IsPropertiesUpdate)
        //            WriteProperty();
        //        oldTitle = string.Empty;
        //    }
        //    else
        //    {
        //        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(folder))
        //        {
        //            if (!IsPropertiesUpdate) IsPropertiesUpdate = true;
        //            return;
        //        }
        //        XmlSerializer xml; // = new XmlSerializer(this.GetType(), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(SampleInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
        //        if (this is ProjectInfo)
        //            xml = xmlProject;
        //        else if (this is ExperimentInfo)
        //            xml = xmlExperiment;
        //        else
        //            xml = xmlWork;
        //        using (StreamWriter writer = new StreamWriter(fileInfo))//TODO fazer estes arquivos hidden
        //        {
        //            xml.Serialize(writer, this);
        //        }
        //        countSize();
        //        GetLastModified();
        //        ChildChanged(this, "Size");
        //    }
        //}
        //protected void VerifyCodeID()
        //{
        //    if (string.IsNullOrWhiteSpace(CodeID))
        //    {
        //        ChangeCodeID();
        //    }
        //}
        //public void ChangeCodeID()
        //{
        //    char c = this is ProjectInfo ? 'P' : this is ExperimentInfo ? 'E' : 'W';
        //    CodeID = c + Numbers.GenerateCodeID(10);
        //    WriteProperty();
        //}
        public void ChangeCodeIDChildren()
        {
            ChangeCodeID();
            if (Children != null)
                foreach (ProjectInfoBase child in Children)
                    child.ChangeCodeIDChildren();
        }
        public virtual void TakeDiffractometers(ObservableCollection<DiffractometerInfo> diffs)
        {
            if (Children is not null && Children.Count > 0)
                foreach (ProjectInfoBase child in Children)
                    if (child is not null)
                        child.TakeDiffractometers(diffs);
            UpdateDiffractometers();
        }
        public virtual void TakeSamples(ObservableCollection<SampleInfo> projectSams)
        {
            if (Children is not null && Children.Count > 0)
                foreach (ProjectInfoBase child in Children)
                    if (child is not null)
                        child.TakeSamples(projectSams);
            UpdateSamples();
        }
        public virtual ObservableCollection<SampleInfo> GetSamples()
        {
            return samples;
        }
        public virtual ObservableCollection<DiffractometerInfo> GetDiffractometers()
        {
            return Diffractometers;
        }
        public virtual void ReplaceSamples(ObservableCollection<SampleInfo> sam)
        {
            if (Children is not null)
                foreach (ProjectInfoBase child in Children)
                    child.ReplaceSamples(sam);
        }//public virtual string[] GetSamplesCodeID()
        //{
        //    List<string> result = new();
        //    foreach (SampleInfo s in samples)
        //        result.Add(s.CodeID);
        //    return result.ToArray();
        //}
        protected virtual ObservableCollection<SampleInfo> GetProjectSamples()
        {
            if (parent is not null)
                return parent.GetProjectSamples();
            return null;
        }
        public virtual void AddProjectSample(ObservableCollection<SampleInfo> sample)
        {
            if (parent is not null)
                parent.AddProjectSample(sample);
        }
        protected virtual void UpdateDiffractometers()
        {
            if (Children is not null && Children.Count != 0)
            {
                Diffractometers.Clear();
                foreach (ProjectInfoBase child in Children)
                {
                    if (child is not null)
                        foreach (DiffractometerInfo d in child.Diffractometers)
                            if (!Diffractometers.Contains(d))
                                Diffractometers.Add(d);
                }
                RaisePropertyChanged("Diffractometers");
            }
            if (parent is not null)
                parent.UpdateDiffractometers();
        }
        protected virtual void UpdateSamples()
        {
            if (Children is not null && Children.Count != 0)
            {
                Samples.Clear();
                foreach (ProjectInfoBase child in Children)
                {
                    if (child is not null)
                        foreach (SampleInfo s in child.Samples)
                        {
                            if (!Samples.Contains(s))
                                Samples.Add(s);
                        }
                }
                RaisePropertyChanged("Samples");
            }
            if (parent is not null)
                parent.UpdateSamples();
        }
        public virtual void UpdateChildrenInfos()
        {
            countExperimentsMethod();
            countWorksMethod();
            countSize();
            GetLastModified();
        }
        public override string ToString()
        {
            return Title;
        }
        #endregion
        #region Events
        protected virtual void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == Children)
            {
                ChildChanged(null);
            }
        }
        public virtual void ChildChanged(ProjectInfoBase sender, params string[] propertyName)
        {
            if (sender != this)
            {
                if (propertyName.Length == 0 || propertyName.Contains("CountExperiments")) { countExperimentsMethod(); } //{ RaisePropertyChanged("CountExperiments"); }
                if (propertyName.Length == 0 || propertyName.Contains("CountWorks")) { countWorksMethod(); } //{ RaisePropertyChanged("CountWorks"); }
                if (propertyName.Length == 0 || propertyName.Contains("Size")) { countSize(); } //{ RaisePropertyChanged("Size"); }
                if (propertyName.Length == 0 || propertyName.Contains("LastModified")) { GetLastModified(); } //{ RaisePropertyChanged("Size"); }
                if (propertyName.Length == 0 || propertyName.Contains("Diffractometers")) UpdateDiffractometers();
                if (propertyName.Length == 0 || propertyName.Contains("Samples")) UpdateSamples();
            }
            if (parent is not null)
                parent.ChildChanged(sender, propertyName);
        }
        public void ProjectNotification(ProjectNotificationEventArgs args)
        {
            if (isGetPropertyFromFile || isXmlProcess) return;

            if (parent is not null)
                parent.ProjectNotification(args);
            else
                ProjectNotificationEvent(args);
        }
        public event ProjectNotificationEventHandler ProjectNotificationEvent;
        public delegate void ProjectNotificationEventHandler(ProjectNotificationEventArgs args);
        //public event PropertyChangedEventHandler PropertyChanged;
        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
            if (propertyName == "CodeID")
                WriteProperty();
        }
        #endregion
        #region Static methods
        public static void VerifyTitle(ProjectInfoBase obj, bool isImport = false)
        {
            if (obj is null)
                return;

            bool sameTitle = true;
            bool changeOriginal = false;
            string titleTemp = obj.Title.Substring(0);
            int i = 1;
            ProjectInfoBase parent = obj.GetParent(); //TODO se for o método desserialize vai dar problema aqui pq nenhum objeto tem pai
            ProjectInfoBase original = null;
            while (sameTitle)
            {
                changeOriginal = false;
                sameTitle = false;
                if (parent is not null)
                {
                    if (parent.Children is null)
                        return;
                    foreach (ProjectInfoBase brother in parent.Children)
                    {
                        if (brother == obj)
                            continue;
                        if (titleTemp == brother.Title)
                        {
                            sameTitle = true;
                            //Se não for import tem que verificar qual title deve ser mudado, olhar qual foi modificado pela utlima vez
                            if (!isImport)
                            {
                                if (obj.LastTitleChange > brother.LastTitleChange)
                                {
                                    changeOriginal = true;
                                    original = brother;
                                }
                            }
                            break;
                        }
                    }
                }
                else
                {
                    foreach (ObjectCodeID brother in PoolCodeID.CodeIDRead())
                    {
                        ProjectInfoBase project = brother as ProjectInfoBase;
                        if (project is null || project == obj)
                            continue;
                        if (titleTemp == project.Title)
                        {
                            sameTitle = true;
                            //Se não for import tem que verificar qual title deve ser mudado, olhar qual foi modificado pela utlima vez
                            if (!isImport)
                            {
                                if (obj.LastTitleChange > project.LastTitleChange)
                                {
                                    changeOriginal = true;
                                    original = project;
                                }
                            }
                            break;
                        }
                    }
                }
                if (sameTitle)
                {
                    titleTemp = obj.Title;
                    if (isImport)
                    {
                        titleTemp += " - Imported";
                        if (i > 1)
                            titleTemp += " (" + i + ")";
                        i++;
                    }
                    else
                    {
                        i++;
                        titleTemp += " (" + i + ")";
                    }
                }
            }
            if (obj.Title != titleTemp)
            {
                if (isImport || !changeOriginal)
                    obj.Title = titleTemp;
                else if (original is not null)
                    original.Title = titleTemp;
            }
        }
        #endregion

        #region Fileds of Properties
        private DateTime _lastModified;
        //private string codeID;
        protected string _title;
        protected string _description;
        protected string _type;
        private DateTime _lastTitleChange;
        #endregion
    }
}
