using BaseLibrary;
using GosControls.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GosControls.ViewModels
{
    public class HubLibraryViewModel : INotifyPropertyChanged
    {
        #region Geral
        //public string FolderProjects { get => _folderProjects; set => _folderProjects = value; }
        //public string FileRadiations { get => _fileRadiations; set => _fileRadiations = value; }
        //public string FileDiffractometers { get => _fileDiffractometers; set => _fileDiffractometers = value; }
        /// <summary>
        /// Pasta onde ficam os trabalhos ativos do usuário atual
        /// </summary>
        public string FolderUser { get => _folderLibrary; set { _folderLibrary = value; FoldersVerify(); } }
        public string FolderLibrary { get => Path.Combine(FolderUser, "MyLibrary"); }
        /// <summary>
        /// Arquivo com a biblioteca de radiações
        /// </summary>
        public string FolderProjects { get => Path.Combine(FolderLibrary, "Projects"); }
        public string FolderDiffractometers { get => Path.Combine(FolderLibrary, "Diffractometers"); }
        public string FolderRadiations { get => Path.Combine(FolderLibrary, "Radiations"); }
        //public string FolderApplicationData { get => _folderAplicationData; set => _folderAplicationData = value; }
        public string FolderApplicationData { get; set; }
        public Queue<string> LogTexts = new();
        public HubLibraryViewModel()
        {
            //Diffractometers.CollectionChanged += CollectionChanged;
            //Radiations.CollectionChanged += CollectionChanged;
        }
        private void FoldersVerify()
        {
            if (!Directory.Exists(FolderLibrary)) Directory.CreateDirectory(FolderLibrary);
            if (!Directory.Exists(FolderProjects)) Directory.CreateDirectory(FolderProjects);
            if (!Directory.Exists(FolderDiffractometers)) Directory.CreateDirectory(FolderDiffractometers);
            if (!Directory.Exists(FolderRadiations)) Directory.CreateDirectory(FolderRadiations);
        }
        #endregion
        #region Projects
        public ObservableCollection<ProjectInfo> Projects { get => _projects; set { _projects = value; RaisePropertyChanged(); } }
        public ProjectInfoBase ProjectSelected { get => _projectSelected; set { _projectSelected = value; projectChanged(); ProjectShow.ProjectInfo = ProjectSelected; RaisePropertyChanged(); } }
        public ProjectInfoShowProperties ProjectShow { get => _projectShow; set { _projectShow = value; RaisePropertyChanged(); } }
        private void projectChanged()
        {
            if (!IsProjectsLastEnable && _projectSelected != null)
            {
                IsProjectsLastEnable = true;
            }
            if (_projectSelected == null)
            {
                //bindingProjectUpdate();
            }
            else if (_projectSelected is ProjectInfo)
            {
                ProjectSelectedTitle = "Project \"" + _projectSelected.Title + "\" Selected";
                //bindingProjectUpdate();
            }
            else if (_projectSelected is ExperimentInfo)
            {
                ProjectSelectedTitle = "Experiment \"" + _projectSelected.Title + "\" Selected";
                //bindingProjectUpdate();
            }
            else if (_projectSelected is WorkInfo)
            {
                ProjectSelectedTitle = "Work \"" + _projectSelected.Title + "\" Selected";
                ProcessWorkActive((WorkInfo)_projectSelected);
                //bindingProjectUpdate();
            }
            PropertiesProjectUpdate();
        }
        private bool isLibrarySearch = false;
        public void ProjectSearchGridArrange(bool isSearch)
        {
            isLibrarySearch = isSearch;
            PropertiesProjectUpdate();
        }
        bool isGetLibrary = false;
        public async Task GetLibrary()
        {
            try
            {
                PoolCodeID.CodeIDClear();

                isGetLibrary = true;
                zeraAllWorksVariable();

                //Radiations
                string[] files = Directory.GetFiles(FolderRadiations);
                if (files != null && files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        RadiationInfo rad = new RadiationInfo(file);
                        if (rad != null && !string.IsNullOrWhiteSpace(rad.ToString()))
                            radiations.Add(rad);
                    }
                    Collections.OrderObservable(radiations);
                    RaisePropertyChanged("Radiations");
                }
                //Diffractometers
                files = Directory.GetFiles(FolderDiffractometers);
                if (files != null && files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        DiffractometerInfo diff = new DiffractometerInfo(file);
                        if (diff != null && !string.IsNullOrWhiteSpace(diff.ToString()))
                            diffractometers.Add(diff);
                        diff.AddInitialRadiation(Radiations);
                    }
                    Collections.OrderObservable(diffractometers);
                    RaisePropertyChanged("Diffractometers");
                }

                //Works
                await getProjects();
                IsProjectsLastEnable = false;
            }
            catch (Exception e) { _ = e; }
            finally
            {
                isGetLibrary = false;
            }
            VerifyAllTitles(Projects);
        }
        private void VerifyAllTitles(IList list)
        {
            if (list is null)
                return;
            foreach (ProjectInfoBase obj in list)
            {
                if (obj is null)
                    continue;
                ProjectInfoBase.VerifyTitle(obj);
                VerifyAllTitles(obj.Children);
            }
        }
        private async Task getProjects(List<ProjectInfo> projectsToImport = null, string folderToImport = null)
        {
            Queue<ProjectInfo> projectTemp = new();
            Task task;
            if (projectsToImport is null)
            {
                task = new Task(() =>
                {

                    string[] projectsList = Directory.GetDirectories(FolderProjects);
                    //TODO usar um loop parallel
                    //TODO o gargalo do processamento está neste loop, verificar o que está demorando neste processo
                    foreach (string p in projectsList)
                    {
                        ProjectInfo temp = new ProjectInfo(p);
                        //Projects.Add(); //O projeto testes é o que mais demora
                        temp.UpdateChildrenInfos();
                        temp.TakeDiffractometers(Diffractometers);
                        temp.TakeSamples(null);
                        projectTemp.Enqueue(temp);
                    }
                });
            }
            else
            {
                task = new Task(() =>
                {
                    string[] projectsList = Directory.GetDirectories(folderToImport);
                    //TODO usar um loop parallel
                    foreach (string p in projectsList)
                    {
                        projectsToImport.Add(new ProjectInfo(p));
                        projectsToImport.Last().UpdateChildrenInfos();
                        projectsToImport.Last().TakeDiffractometers(Diffractometers);
                        projectsToImport.Last().TakeSamples(null);
                    }
                });
            }
            if (task != null)
            {
                task.Start();
                await task;
                if (projectsToImport is null)
                {
                    while (projectTemp.Count > 0)
                        Projects.Add(projectTemp.Dequeue());
                    Collections.OrderObservable(Projects);
                }
            }
        }
        public async Task DeleteProject(ProjectInfoBase info)
        {
            ProjectInfoBase parentInfo = info.GetParent();
            if (parentInfo is not null && parentInfo.Children.Contains(info))
                parentInfo.Children.Remove(info);
            else
                Projects.Remove((ProjectInfo)info);
            info.DeleteObjectFiles();
            info = null;
        }
        private void zeraAllWorksVariable()
        {
            //ProcessWorkActive(null);
            ProjectSelected = null;
            //Mata todos os projects para não alterar os Diffratometers, samples e radiations
            //zerar o Diffractometers e radiations antes disso
            if (Projects.Count > 0)
            {
                for (int i = 0; i < Projects.Count; i++)
                {
                    Projects[i].KillAllChildren();
                }
            }
            Projects.Clear();
            if (Diffractometers.Count > 0)
                Diffractometers.Clear();
            if (Radiations.Count > 0)
                Radiations.Clear();
        }
        #endregion
        #region Manipuladores da View Library
        private void PropertiesProjectUpdate()
        {
            RaisePropertyChanged("ProjectGridWidth");
            RaisePropertyChanged("ExperimentGridWidth");
            RaisePropertyChanged("WorkGridWidth");
            RaisePropertyChanged("IsPropertyGridVisibility");
            RaisePropertyChanged("ProjectsPanelOrientation");
            RaisePropertyChanged("ExperimentsPanelOrientation");
            RaisePropertyChanged("WorksPanelOrientation");
            RaisePropertyChanged("IsProjesHomeEnable");
            RaisePropertyChanged("IsProjectAddEnable");
            RaisePropertyChanged("IsProjectRemoveEnable");
            RaisePropertyChanged("IsLibraryUpdateEnable");
            RaisePropertyChanged("IsPropertyProjectVisible");
            RaisePropertyChanged("IsPropertyExperimentVisible");
            RaisePropertyChanged("IsPropertyWorkVisible");
            RaisePropertyChanged("IsWorkActive");
            RaisePropertyChanged("IsGridSidarinVisible");
            //menuItemEnableCheck();
        }
        private string projectSelectedTitle;
        public string ProjectSelectedTitle { get => projectSelectedTitle; private set { projectSelectedTitle = value; RaisePropertyChanged(); } }
        /// <summary>
        /// NegativeInfinity = Auto e PositiveInfinity = Star
        /// </summary>
        public double ProjectGridWidth { get { if (isLibrarySearch) return -1; else if (_projectSelected is null) return -1; else return double.PositiveInfinity; } }
        /// <summary>
        /// NegativeInfinity = Auto e PositiveInfinity = Star
        /// </summary>
        public double ExperimentGridWidth { get { if (isLibrarySearch) return -1; else if (_projectSelected is null) return 0; else if (_projectSelected is ProjectInfo) return -1; else return double.PositiveInfinity; } }
        /// <summary>
        /// NegativeInfinity = Auto e PositiveInfinity = Star
        /// </summary>
        public double WorkGridWidth { get { if (isLibrarySearch) return -1; else if (_projectSelected is null || _projectSelected is ProjectInfo) return 0; else return -1; } }
        /// <summary>
        /// Null é collapsed, true é visible e false é hiden
        /// </summary>
        public bool? IsPropertyGridVisibility { get { if (_projectSelected is null) return null; else return true; } }
        /// <summary>
        /// True é vertical e false é horizontal
        /// </summary>
        public bool ProjectsPanelOrientation { get { if (isLibrarySearch) return false; else if (_projectSelected is null) return false; else return true; } }
        /// <summary>
        /// True é vertical e false é horizontal
        /// </summary>
        public bool ExperimentsPanelOrientation { get { if (isLibrarySearch) return false; else if (_projectSelected is ProjectInfo) return false; else return true; } }
        /// <summary>
        /// True é vertical e false é horizontal
        /// </summary>
        public bool WorksPanelOrientation { get => false; }
        public bool IsProjesHomeEnable { get { if (isLibrarySearch) return false; else if (_projectSelected is null) return false; else return true; } }
        private bool isProjesLastEnable;
        public bool IsProjectsLastEnable { get { if (isLibrarySearch) return false; else return isProjesLastEnable; } set { isProjesLastEnable = value; RaisePropertyChanged(); } }
        public bool IsProjectAddEnable
        {
            get
            {
                if (isLibrarySearch) return false;
                else if (_projectSelected is null) return true;
                else if (_projectSelected is not WorkInfo) return !string.IsNullOrWhiteSpace(_projectSelected.GetFolder());
                else return _projectSelected.GetParent() is not null && !string.IsNullOrWhiteSpace(_projectSelected.GetParent().GetFolder());
            }
        } //{ if (projectSelected is not WorkInfo) return true; else return false; }
        public bool IsProjectRemoveEnable { get { if (_projectSelected is null) return false; else return true; } }
        public bool IsProjectCopyEnable { get { if (_projectSelected is not null && !string.IsNullOrWhiteSpace(_projectSelected.Title)) return true; return true; } }
        public bool IsLibraryUpdateEnable { get { if (isLibrarySearch) return false; else return true; } }
        public bool? IsPropertyProjectVisible { get { if (_projectSelected is ProjectInfo) return true; else return false; } }
        public bool? IsPropertyExperimentVisible { get { if (_projectSelected is ExperimentInfo) return true; else return false; } }
        public bool? IsPropertyWorkVisible { get { if (_projectSelected is WorkInfo) return true; else return false; } }
        #endregion
        #region WorkActive
        private WorkInfo workActive;
        public WorkInfo GetWorkActive() => workActive;
        public void ProcessWorkActive(in WorkInfo selected)
        {
            if (selected is null)
            {
                workActive = null;
                //SinFile = string.Empty;
                WorkActiveTitle = string.Empty;
            }
            else
            {
                if (workActive == selected && !Directory.Exists(workActive.GetFolder()) && File.Exists(Path.Combine(workActive.GetFolder(), "Workfile.sin"))) return;

                workActive = selected;
                if (string.IsNullOrWhiteSpace(workActive.GetFolder()) || !Directory.Exists(workActive.GetFolder()))
                {
                    //SinFile = string.Empty;
                    WorkActiveTitle = string.Empty;
                    return;
                }
                WorkActiveTitleChanged();
            }
            PropertiesProjectUpdate();
            //resultsClear();
        }
        public void WorkActiveTitleChanged(in WorkInfo selected = null)
        {
            if (selected is not null && workActive != selected)
                ProcessWorkActive(selected);
            if (!File.Exists(Path.Combine(workActive.GetFolder(), "Workfile.sin")))
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(Path.Combine(workActive.GetFolder(), "Workfile.sin")))
                    {
                        stream.Write("");
                    }
                }
                catch { workActive = null; return; }
            }
            WorkActiveTitle = workActive.Title;
            PropertiesProjectUpdate();
        }
        private string _workActiveTitle = string.Empty;
        public string WorkActiveTitle { get => _workActiveTitle; private set { _workActiveTitle = value; RaisePropertyChanged(); } }
        public bool? IsWorkActive
        {
            get
            {
                if (workActive is null)
                    return null;
                else if (string.IsNullOrWhiteSpace(workActive.GetFolder()))
                    return false;
                else
                    return true;
            }
        }
        #endregion
        #region Diffractometers
        private ObservableCollection<DiffractometerInfo> diffractometers = new();
        public ObservableCollection<DiffractometerInfo> Diffractometers { get => diffractometers; set { diffractometers = value; RaisePropertyChanged(); } }
        private DiffractometerInfo diffractometerSelected;
        public DiffractometerInfo DiffractometerSelected { get => diffractometerSelected; set { diffractometerSelected = value; RaisePropertyChanged(); } }
        public void AddDiffractometer(DiffractometerInfo diff)
        {
            Diffractometers.Add(diff);
            Diffractometers.Last().PropertyChanged += ModelPropertyChangedEvent;
            Collections.OrderObservable(diffractometers);
        }
        public async void DiffractometersFileUpdate(DiffractometerInfo diff)
        {
            if (diff == null)
                return;
            //if (string.IsNullOrWhiteSpace(diff.Brand) || string.IsNullOrWhiteSpace(diff.Model))
            //{
            //    diffractometers.Remove(diff);
            //    return;
            //}
            await ManagerFiles.WriteDiffractometerAsync(FolderDiffractometers, diff);
        }
        public async Task RemoveDiffratometer(System.Collections.IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ((DiffractometerInfo)list[i]).DeleteObjectFiles();
                Diffractometers.Remove(list[i] as DiffractometerInfo);
            }
        }
        #endregion
        #region Radiations
        public void AddRadiation(RadiationInfo rad)
        {
            Radiations.Add(rad);
            //Radiations.Last().PropertyChanged += ModelPropertyChangedEvent;
            //Radiations.Last().Lines.CollectionChanged += CollectionChanged;
            Collections.OrderObservable(radiations);
        }
        private ObservableCollection<RadiationInfo> radiations = new();
        public ObservableCollection<RadiationInfo> Radiations { get => radiations; set { radiations = value; RaisePropertyChanged(); } }
        private RadiationInfo radiationSelected;
        public RadiationInfo RadiationSelected { get => radiationSelected; set { radiationSelected = value; RaisePropertyChanged(); } }
        public async Task RemoveRadiation(System.Collections.IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ((RadiationInfo)list[i]).DeleteObjectFiles();
                Radiations.Remove(list[i] as RadiationInfo);
            }
        }
        public async void RadiationsFileUpdate(RadiationInfo rad)
        {
            if (rad == null)
                return;
            //if (string.IsNullOrWhiteSpace(rad.Label) || rad.Lines.Count == 0)
            //{
            //    radiations.Remove(rad);
            //    return;
            //}
            //else
            await ManagerFiles.WriteRadiationAsync(FolderRadiations, rad);
        }
        #endregion
        #region Events
        private void ModelPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DiffractometerInfo)
            {
                DiffractometerInfo diffTemp = sender as DiffractometerInfo;
                if (diffTemp is not null)
                {
                    //Não pode ser em branco nem o Brand nem o Model
                    if (!string.IsNullOrWhiteSpace(diffTemp.Brand) && !string.IsNullOrWhiteSpace(diffTemp.Model))
                        DiffractometersFileUpdate(diffTemp);
                }
                if (e.PropertyName == nameof(DiffractometerInfo.Brand) || e.PropertyName == nameof(DiffractometerInfo.Model) || e.PropertyName == nameof(DiffractometerInfo.Institution) || e.PropertyName == nameof(DiffractometerInfo.Tag))
                {
                    string[] files = Directory.GetFiles(FolderDiffractometers);
                    if (files != null && files.Length > 0)
                    {
                        foreach (string file in files)
                        {
                            bool find = false;
                            foreach (DiffractometerInfo diff in Diffractometers)
                            {
                                if (diff.ToString() == Path.GetFileNameWithoutExtension(file))
                                {
                                    find = true;
                                    break;
                                }
                            }
                            if (!find)
                                File.Delete(file);
                        }
                    }
                }
                //else
                //    DiffractometersFileUpdate();
            }
            //else if (sender is RadiationInfo)
            //{
            //    RadiationInfo radTemp = sender as RadiationInfo;
            //    if (radTemp is null)
            //    { }
            //    if (radTemp is not null)
            //    {
            //        if ((!string.IsNullOrWhiteSpace(radTemp.Label)/* && radTemp.Lines.Count > 0*/))
            //            RadiationsFileUpdate(radTemp);
            //    }
            //    if (e.PropertyName == nameof(RadiationInfo.Label))
            //    {
            //        string[] files = Directory.GetFiles(FolderRadiations);
            //        if (files != null && files.Length > 0)
            //        {
            //            foreach (string file in files)
            //            {
            //                bool find = false;
            //                foreach (RadiationInfo rad in Radiations)
            //                {
            //                    if (rad.ToString() == Path.GetFileNameWithoutExtension(file))
            //                    {
            //                        find = true;
            //                        break;
            //                    }
            //                }
            //                if (!find)
            //                    File.Delete(file);
            //            }
            //        }
            //    }
            //    //else
            //    //    RadiationsFileUpdate();
            //}
            //else if (sender is XRayLineInfo)
            //{
            //    RadiationInfo radTemp = null;
            //    XRayLineInfo xRayTemp = sender as XRayLineInfo;
            //    if (xRayTemp is not null)
            //    {
            //        foreach (RadiationInfo temp in Radiations)
            //            if (temp.Lines.Contains(xRayTemp))
            //            {
            //                radTemp = temp;
            //                break;
            //            }
            //    }
            //    if (radTemp is not null)
            //    {
            //        if ((!string.IsNullOrWhiteSpace(radTemp.Label) && radTemp.Lines.Count > 0))
            //            RadiationsFileUpdate(radTemp);
            //    }
            //}
        }
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e is null)
                return;
            //if (e.Action == NotifyCollectionChangedAction.Add)
            //{
            //    foreach (object o in e.NewItems)
            //    {
            //        INotifyPropertyChanged inot = o as INotifyPropertyChanged;
            //        if (inot is not null)
            //        {
            //            inot.PropertyChanged += ModelPropertyChangedEvent;
            //            //EventInfo eventInfo = inot.GetType().GetEvent("PropertyChangedEventHandler");
            //            //MethodInfo methodInfo = inot.GetType().GetMethod("PropertyChanged");
            //            //if (eventInfo is not null && methodInfo is not null)
            //            //    eventInfo.AddEventHandler(inot, Delegate.CreateDelegate(eventInfo.EventHandlerType, inot, methodInfo));
            //        }
            //    }
            //}
            if (isGetLibrary)
                return;
            if (sender == Diffractometers)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    //for (int i = 0; i < e.NewItems.Count; i++)
                    //    DiffractometersFileUpdate(e.NewItems[i] as DiffractometerInfo);
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        string fileTemp = Path.Combine(FolderDiffractometers, ((DiffractometerInfo)e.OldItems[i]).ToString() + ".xml");
                        if (File.Exists(fileTemp))
                            File.Delete(fileTemp);
                    }
            }
            else if (sender == Radiations)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        //RadiationsFileUpdate(e.NewItems[i] as RadiationInfo);
                        //(e.NewItems[i] as RadiationInfo).Lines.CollectionChanged += CollectionChanged;

                    }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        string fileTemp = ((RadiationInfo)e.OldItems[i]).GetFilePath();
                        if (File.Exists(fileTemp))
                            File.Delete(fileTemp);
                    }
            }
            //else if (sender is List<XRayLineInfo>)
            //{
            //    RadiationInfo radTemp = null;
            //    ObservableCollection<XRayLineInfo> linesTemp = sender as ObservableCollection<XRayLineInfo>;
            //    foreach (RadiationInfo r in Radiations)
            //        if (r.Lines == linesTemp)
            //            radTemp = r;
            //    if (e.Action == NotifyCollectionChangedAction.Add)
            //        for (int i = 0; i < e.NewItems.Count; i++)
            //        {
            //            (e.NewItems[i] as XRayLineInfo).PropertyChanged += ModelPropertyChangedEvent;
            //        }
            //    if (radTemp != null)
            //        RadiationsFileUpdate(radTemp);
            //}
            //ModelPropertyChangedEvent(sender, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region File
        public async Task ExportSelectedProject(string zipPath, ProjectInfoBase[] projectsToExport)
        {
            string log = await FileMethods.ExportSelectedProject(zipPath, projectsToExport, FolderApplicationData);
            if (!string.IsNullOrWhiteSpace(log))
                AddLogText(log);
        }
        public ObservableCollection<SameProjetcInfo> SameWorksImport { get => _sameWorksImport; private set => _sameWorksImport = value; }
        public void SetSameWorksImportBinding(ObservableCollection<SameProjetcInfo> value) => SameWorksImport = value;
        public async Task ImportProject(string zipFilePath, bool isReturn)
        {
            if (string.IsNullOrWhiteSpace(zipFilePath) && SameWorksImport.Count == 0)
                return;

            List<SameProjetcInfo> SameTemp = new();
            if (isReturn)
                SameTemp.AddRange(SameWorksImport);
            string imported = await FileMethods.ImportProject(zipFilePath, isReturn, FolderApplicationData, FolderProjects, SameTemp);
            if (!isReturn)
            {
                foreach (SameProjetcInfo same in SameTemp)
                    SameWorksImport.Add(same);
                //if (!string.IsNullOrWhiteSpace(imported))
                //    AddLogText(string.Format("Some objects were imported: {0}", imported));
            }
            else
            {
                SameWorksImport.Clear();
                //if (!string.IsNullOrWhiteSpace(imported))
                //    AddLogText(string.Format("Some objects were imported: {0}", imported));
            }
            if (!string.IsNullOrWhiteSpace(imported))
                AddLogText(string.Format("Some objects were imported: {0}", imported));
            RaisePropertyChanged("SameWorksImport");
        }
        public async Task ProcessFileDrop(string[] files)
        {
            if (files is null || files.Length == 0)
                return;

            List<SameProjetcInfo> sameWorks = new();
            List<string> imported;
            List<string> log;
            (imported, log) = await FileMethods.ProcessFilesToImport(files, FolderApplicationData, FolderProjects, sameWorks);
            if (imported != null && imported.Count > 0)
            {
                string temp = string.Empty;
                foreach (string imp in imported)
                {
                    temp += imp;
                    if (imp != imported.Last())
                        temp += "; ";
                }
                AddLogText(string.Format("Some objects were imported: {0}", temp));
            }
            if (sameWorks.Count > 0)
                foreach (SameProjetcInfo same in sameWorks)
                    SameWorksImport.Add(same);
            if (log != null && log.Count > 0)
                foreach (string s in log)
                    AddLogText(s);
        }
        private void AddLogText(string message)
        {
            LogTexts.Enqueue(message);
        }
        #endregion
        #region Backup file process
        string bakSinPath = null, bakPath = null, bakWorkFolder = null;
        public void FindBackup()
        {
            if (!string.IsNullOrWhiteSpace(bakPath))
                return;

            string[] projectsDirectories = Directory.GetDirectories(FolderProjects);
            foreach (string pro in projectsDirectories)
            {
                string[] experimentsDirectoreies = Directory.GetDirectories(pro);
                foreach (string exp in experimentsDirectoreies)
                {
                    string[] worksDirectoreies = Directory.GetDirectories(exp);
                    foreach (string wor in worksDirectoreies)
                    {
                        string bakPathTemp = Path.Combine(wor, "Workfile.bak");
                        string sinPathTemp = Path.Combine(wor, "Workfile.sin");
                        if (File.Exists(bakPathTemp))
                        {
                            if (string.IsNullOrWhiteSpace(bakPath))
                            {
                                if (!File.Exists(sinPathTemp) || File.GetCreationTime(bakPathTemp) >= File.GetCreationTime(sinPathTemp))
                                {
                                    if (string.IsNullOrWhiteSpace(bakPath))
                                    {
                                        //TODO verificar primeiro se os textos são iguais
                                        string backSinText = FileProcess.ReadTXT(sinPathTemp);
                                        string backText = FileProcess.ReadTXT(bakPathTemp);
                                        if (backSinText.ToString() != backText.ToString())
                                        {
                                            bakPath = bakPathTemp.Substring(0);
                                            bakWorkFolder = wor.Substring(0);
                                        }
                                    }
                                }
                                else
                                    File.Delete(bakPathTemp);
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(bakPath) || File.GetCreationTime(bakPathTemp) > File.GetCreationTime(bakPath))
                                {
                                    if (File.Exists(bakPath))
                                        File.Delete(bakPath);
                                    bakPath = bakPathTemp.Substring(0);
                                    bakWorkFolder = wor.Substring(0);
                                }
                                else
                                {
                                    File.Delete(bakPathTemp);
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(bakPath))
            {
                bakSinPath = Regex.Replace(bakPath, @"bak$", "sin", RegexOptions.IgnoreCase);
            }
            //if (!string.IsNullOrWhiteSpace(bakPath))
            //    menuItemEnableCheck();
        }
        public (string bakSinPath, string bakPath, string bakWorkFolder) GetBakPath() => (bakSinPath, bakPath, bakWorkFolder);
        public void BackupProcess(bool isBakChoice)
        {
            if (isBakChoice)
            {
                File.Move(bakPath, bakSinPath, true);
            }
            else
            {
                File.Delete(bakPath);
            }
            bakSinPath = null;
            bakPath = null;
            bakWorkFolder = null;
            //menuItemEnableCheck();
        }
        #endregion
        #region variables of Properties
        private ObservableCollection<SameProjetcInfo> _sameWorksImport = new();
        private ObservableCollection<ProjectInfo> _projects = new();
        private ProjectInfoBase _projectSelected;
        private ProjectInfoShowProperties _projectShow = new();
        private string _folderLibrary;
        #endregion
    }
}
