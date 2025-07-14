using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class ProjectInfoShowProperties : INotifyPropertyChanged
    {
        public ProjectInfoBase ProjectInfo { get => _projectInfo; set { _projectInfo = value; changedInfos(); RaisePropertyChanged(); } }
        private void changedInfos()
        {
            if (ProjectInfo is null)
            {
                Title = null;
                Description = null;
                Samples.Clear();
                Diffractometers.Clear();
                CountExperiments = -1;
                CountWorks = -1;
                Size = double.NaN;
                LastModified = DateTime.MinValue;
                Start = DateTime.MinValue;
                End = DateTime.MinValue;
                Date = DateTime.MinValue;
                SamplesToXml.Clear();
                DiffractometersToXml.Clear();
                return;
            }
            Title = ProjectInfo.Title;
            Description = ProjectInfo.Description;
            foreach (SampleInfo s in ProjectInfo.Samples)
                Samples.Add(s);
            foreach (DiffractometerInfo d in ProjectInfo.Diffractometers)
                Diffractometers.Add(d);
            CountExperiments = ProjectInfo.CountExperiments;
            CountWorks = ProjectInfo.CountWorks;
            Size = ProjectInfo.Size;
            LastModified = ProjectInfo.LastModified;
            if (ProjectInfo is ProjectInfo)
            {
                Start = (ProjectInfo as ProjectInfo).Start;
                End = (ProjectInfo as ProjectInfo).End;
                Date = DateTime.MinValue;
                SamplesToXml.Clear();
                DiffractometersToXml.Clear();
            }
            else if (ProjectInfo is ExperimentInfo)
            {
                Start = DateTime.MinValue;
                End = DateTime.MinValue;
                Date = (ProjectInfo as ExperimentInfo).Date;
                SamplesToXml.Clear();
                DiffractometersToXml.Clear();
            }
            else
            {
                Start = DateTime.MinValue;
                End = DateTime.MinValue;
                Date = DateTime.MinValue;
                foreach (string s in (ProjectInfo as WorkInfo).SamplesToXml)
                    SamplesToXml.Add(s);
                foreach (string d in (ProjectInfo as WorkInfo).DiffractometersToXml)
                    DiffractometersToXml.Add(d);
            }
        }
        #region Propriedades Comum
        public string Title { get => _title; set { _title = value; RaisePropertyChanged(); } }
        public string Description { get => _description; set { _description = value; RaisePropertyChanged(); } }
        public ObservableCollection<SampleInfo> Samples
        {
            get => _samples;
            set
            {
                _samples = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<DiffractometerInfo> Diffractometers
        {
            get => _diffractometers;
            set
            {
                _diffractometers = value;
                RaisePropertyChanged();
            }
        }
        public int CountExperiments
        {
            get => _countExperiments;
            set
            {
                _countExperiments = value;
                RaisePropertyChanged();
            }
        }
        public int CountWorks
        {
            get => _countWorks;
            set
            {
                _countWorks = value;
                RaisePropertyChanged();
            }
        }
        public double Size
        {
            get => _size;
            set
            {
                _size = value;
                RaisePropertyChanged();
            }
        }
        public DateTime LastModified { get => _lastModified; private set { _lastModified = value; RaisePropertyChanged(); } }
        #endregion
        #region Projects
        public DateTime Start { get => _start; set { _start = value; RaisePropertyChanged(); } }
        public DateTime End { get => _end; set { _end = value; RaisePropertyChanged(); } }
        #endregion
        #region Experiments
        public DateTime Date { get => _date; set { _date = value; RaisePropertyChanged(); } }
        #endregion
        #region Works
        public List<string> SamplesToXml { get => _samplesToXml; set { _samplesToXml = value; } }
        public List<string> DiffractometersToXml { get => _diffractometersToXml; set { _diffractometersToXml = value; } }

        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            //ChildChanged(this, propertyName);
        }

        #region private field from properties
        protected ObservableCollection<SampleInfo> _samples = new();
        protected ObservableCollection<DiffractometerInfo> _diffractometers = new();
        private int _countExperiments;
        private int _countWorks;
        private double _size;
        private DateTime _start;
        private DateTime _end;
        private DateTime _lastModified;
        private string _description;
        private string _title;
        private ProjectInfoBase _projectInfo;
        private DateTime _date;
        private List<string> _samplesToXml = new();
        private List<string> _diffractometersToXml = new();

        #endregion
    }
}
