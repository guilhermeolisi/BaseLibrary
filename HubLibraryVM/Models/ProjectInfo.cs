using BaseLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GosControls.Models
{
    public class ProjectInfo : ProjectInfoBase
    {
        private DateTime start;
        public DateTime Start { get => start; set { start = value; WriteProperty(); RaisePropertyChanged(); } }
        private DateTime end;
        public DateTime End { get => end; set { end = value; WriteProperty(); RaisePropertyChanged(); } }
        private ObservableCollection<SampleInfo> projectSamples = new();
        [XmlIgnore]
        public ObservableCollection<SampleInfo> ProjectSamples { get => projectSamples; set { projectSamples = value; CollectionChanged(this.projectSamples, null); RaisePropertyChanged(); } }

        /// <summary>
        /// usado só para o XMLSerialize
        /// </summary>
        public ProjectInfo() { }
        /// <summary>
        /// Usado para adicionar pelo usuário
        /// </summary>
        /// <param name="parent"></param>
        public ProjectInfo(string projectFolder, bool teste) : base(null)
        {
            Children = new();
            SetProjectFolder(projectFolder);
            base.SetParentFolder(projectFolder);
            start = DateTime.Today;
            IsPropertiesUpdate = true;
            ProjectSamples.CollectionChanged += CollectionChanged;
        }
        /// <summary>
        /// Usado para adicionar automaticamente dos arquivos library
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        public ProjectInfo(string folder) : base(null, folder)
        {
            ProjectSamples.CollectionChanged += CollectionChanged;
            GetSamplesFromFiles();
        }

        protected string projectFolder;
        public override void SetProjectFolder(string value) => projectFolder = value;
        public override string GetProjectFolder() => projectFolder;
        public void GetSamplesFromFiles()
        {
            try
            {
                //ProjectSamples.CollectionChanged -= CollectionChanged;
                ProjectSamples.Clear();
                if (string.IsNullOrWhiteSpace(Folder))
                    return;
                string[] files = Directory.GetFiles(Folder);
                foreach (string file in files)
                {
                    if (file == FilePath)
                        continue;
                    SampleInfo s = new SampleInfo(this, file);
                    if (s is not null && !string.IsNullOrWhiteSpace(s.ToString()))
                    {
                        ProjectSamples.Add(s);
                        //s.PropertyChanged += ProjectPropertyChanged;
                    }
                }
            }
            finally
            {
                //ProjectSamples.CollectionChanged += CollectionChanged;
            }
        }
        public override void TakeSamples(ObservableCollection<SampleInfo> projectSams)
        {
            Collections.OrderObservable(projectSamples);
            RaisePropertyChanged("ProjectSamples");
            foreach (SampleInfo s in ProjectSamples)
                s.PropertyChanged += ProjectPropertyChanged;
            if (Children is not null && Children.Count > 0)
                foreach (ProjectInfoBase child in Children)
                    child.TakeSamples(ProjectSamples);
        }

        private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is SampleInfo)
                WriteProperty("Samples");
        }

        protected override ObservableCollection<SampleInfo> GetProjectSamples() => ProjectSamples;
        public override void AddProjectSample(ObservableCollection<SampleInfo> samples)
        {
            //Verifica se é necessário adicionar algum Samples no Project
            for (int i = 0; i < samples.Count; i++)
            {
                bool find = false;
                if (string.IsNullOrWhiteSpace(samples[i].Code))
                {
                    samples.Remove(samples[i]);
                    i--;
                    continue;
                }
                for (int j = 0; j < projectSamples.Count; j++)
                    if (samples[i] == projectSamples[j])
                    {
                        find = true;
                        break;
                    }
                if (!find)
                {
                    samples[i].PropertyChanged += ProjectPropertyChanged;
                    if (samples[i].RelatedProject is null)
                        samples[i].RelatedProject = this;
                    else if (samples[i].RelatedProject != this)
                    {
                        samples[i].RelatedProject = this;
                        //samples[i].ChangeCodeID();
                    }
                    projectSamples.Add(samples[i]);
                }
            }
        }
        public override void ChildChanged(ProjectInfoBase sender, params string[] propertyName)
        {
            base.ChildChanged(sender, propertyName);
            if (sender != this)
            {
                if (propertyName.Length == 0 || propertyName.Contains("Samples"))
                {

                }
            }
        }
        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
            if (sender == ProjectSamples)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (SampleInfo s in e.NewItems)
                    {
                        if (s is not null)
                        {
                            s.PropertyChanged += ProjectPropertyChanged;
                            if (s.RelatedProject is null)
                                s.RelatedProject = this;
                            if (s.RelatedProject != this)
                            {
                                s.RelatedProject = this;
                                s.ChangeCodeID();
                            }
                            s.IsNew = false;
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (SampleInfo s in e.OldItems)
                    {
                        if (s is not null)
                        {
                            s.DeleteObjectFiles();
                        }
                    }
                }
                //Não pode fazer modificações no CollectionChanged
                //foreach (SampleInfo s in ProjectSamples)
                //    if (s.IsNew)
                //    {
                //        s.PropertyChanged += ProjectPropertyChanged;
                //        if (s.RelatedProject)
                //        s.IsNew = false;
                //    }
                RaisePropertyChanged("ProjectSamples");
                WriteProperty();
            }
        }
    }
}
