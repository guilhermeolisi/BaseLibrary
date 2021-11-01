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
    public class WorkInfo : ProjectInfoBase
    {
        #region propriedades
        #region Propriedades contadas internamente
        public List<string> SamplesToXml { get => _samplesToXml; set { _samplesToXml = value; } }
        public List<string> DiffractometersToXml { get => _diffractometersToXml; set { _diffractometersToXml = value; } }
        public float Chi2 { get => _chi2; set { _chi2 = value; WriteProperty(); RaisePropertyChanged(); } }
        public float Rwp { get => _rwp; set { _rwp = value; WriteProperty(); RaisePropertyChanged(); } }
        [XmlIgnore]
        public byte CountMeasures { get => _countMeasures; set { _countMeasures = value; WriteProperty(); RaisePropertyChanged(); } }
        [XmlIgnore]
        public ObservableCollection<SampleInfo> ProjectSamples { get { if (_projectSamples is null) _projectSamples = GetProjectSamples(); return _projectSamples; } set { } }
        #endregion
        #endregion
        /// <summary>
        /// usado só para o XMLSerialize
        /// </summary>
        public WorkInfo() { }
        /// <summary>
        /// Usado para adicionar pelo usuário
        /// </summary>
        /// <param name="parent"></param>
        public WorkInfo(ExperimentInfo parent) : base(parent)
        {
            this.parent = parent;
            Children = null;
            Samples.CollectionChanged += CollectionChanged;
            Diffractometers.CollectionChanged += CollectionChanged;
        }
        /// <summary>
        /// Usado para adicionar automaticamente dos arquivos library
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        public WorkInfo(ExperimentInfo parent, string folder) : base(parent, folder)
        {
            Children = null;
            Samples.CollectionChanged += CollectionChanged;
            Diffractometers.CollectionChanged += CollectionChanged;
        }
        public void SetChi2(float value) => _chi2 = value;
        public void SetRwp(float value) => _rwp = value;
        public void SetCountMeasures(byte value) => _countMeasures = value;

        bool isGetDiffractometers = false;
        public override void TakeDiffractometers(ObservableCollection<DiffractometerInfo> diffs)
        {
            try
            {
                isGetDiffractometers = true;
                bool someChanged = false;
                for (int i = 0; i < DiffractometersToXml.Count; i++)
                {
                    bool find = false;
                    foreach (DiffractometerInfo diff in diffs)
                        if (DiffractometersToXml[i] == diff.CodeID)
                        {
                            find = true;
                            Diffractometers.Add(diff);
                            break;
                        }
                    if (!find)
                    {
                        DiffractometersToXml.Remove(DiffractometersToXml[i]);
                        if (!someChanged)
                            someChanged = true;
                        i--;
                    }
                }
                Collections.OrderObservable(diffractometers);
                if (someChanged)
                    WriteProperty();
                RaisePropertyChanged("Diffractometers");
            }
            finally
            {
                isGetDiffractometers = false;
            }
        }
        bool isGetSamples = false;
        public override void TakeSamples(ObservableCollection<SampleInfo> projectsSams)
        {
            try
            {
                isGetSamples = true;
                bool someChanged = false;
                for (int i = 0; i < SamplesToXml.Count; i++)
                {
                    bool find = false;
                    foreach (SampleInfo sam in projectsSams)
                        if (SamplesToXml[i] == sam.CodeID)
                        {
                            find = true;
                            Samples.Add(sam);
                            break;
                        }
                    if (!find)
                    {
                        SamplesToXml.Remove(SamplesToXml[i]);
                        if (!someChanged)
                            someChanged = true;
                        i--;
                    }
                }
                Collections.OrderObservable(Samples);
                if (someChanged)
                    WriteProperty();
                RaisePropertyChanged("Samples");
            }
            finally
            {
                isGetSamples = false;
            }
        }
        public override void ReplaceSamples(ObservableCollection<SampleInfo> sam)
        {
            foreach (SampleInfo s in sam)
                for (int i = 0; i < samples.Count; i++)
                    if (samples[i].ToString() == s.ToString())
                    {
                        Samples.RemoveAt(i);
                        Samples.Add(s);
                    }

        }
        bool isKillChild = false;
        public override void KillAllChildren()
        {
            isKillChild = true;
        }
        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
            if (sender == Samples && !isGetSamples && !isKillChild)
            {
                //verifica se tem sample repetido
                //for (int i = 0; i < Samples.Count; i++)
                //    for (int j = i + 1; j < Samples.Count; j++)
                //        if (Samples[i].Code == Samples[j].Code)
                //        {
                //            Samples.RemoveAt(j);
                //            j--;
                //        }
                //Atualiza o xml
                for (int i = 0; i < Samples.Count; i++)
                {
                    bool find = false;
                    for (int j = 0; j < SamplesToXml.Count; j++)
                    {
                        if (Samples[i].CodeID == SamplesToXml[j])
                        {
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                        SamplesToXml.Add(Samples[i].CodeID);
                }
                for (int i = 0; i < SamplesToXml.Count; i++)
                {
                    bool find = false;
                    for (int j = 0; j < Samples.Count; j++)
                    {
                        if (Samples[j].CodeID == SamplesToXml[i])
                        {
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                        SamplesToXml.Remove(SamplesToXml[i]);
                }
                AddProjectSample(Samples);
                RaisePropertyChanged("Samples");
                //parent.ChildChanged(this, "Samples");
                UpdateSamples();
                WriteProperty();
            }
            else if (sender == Diffractometers && !isGetDiffractometers && !isKillChild)
            {
                for (int i = 0; i < Diffractometers.Count; i++)
                {
                    bool find = false;
                    for (int j = 0; j < DiffractometersToXml.Count; j++)
                        if (DiffractometersToXml[j] == Diffractometers[i].CodeID)
                        {
                            find = true;
                            break;
                        }
                    if (!find)
                        _diffractometersToXml.Add(Diffractometers[i].CodeID);
                }
                for (int i = 0; i < DiffractometersToXml.Count; i++)
                {
                    bool find = false;
                    for (int j = 0; j < Diffractometers.Count; j++)
                        if (DiffractometersToXml[i] == Diffractometers[j].CodeID)
                        {
                            find = true;
                            break;
                        }
                    if (!find)
                        _diffractometersToXml.Remove(_diffractometersToXml[i]);
                }
                RaisePropertyChanged("Diffractometers");
                //parent.ChildChanged(this, "Diffractometers");
                UpdateDiffractometers();
                WriteProperty();
            }
        }
        ObservableCollection<SampleInfo> _projectSamples;
        private List<string> _samplesToXml = new();
        private float _chi2;
        private List<string> _diffractometersToXml = new();
        private float _rwp;
        private byte _countMeasures;

    }
}
