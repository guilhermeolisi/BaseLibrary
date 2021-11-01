using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class RadiationInfo : ObjectToFile, ICloneable, IComparable
    {
        private string _label = string.Empty;
        private ObservableCollection<XRayLineInfo> _lines = new();
        private string _sindarinCode = string.Empty;

        public string Label
        {
            get => _label;
            set
            {
                if ((oldLabel is not null && oldLabel == value))
                    return;
                List<ObjectCodeID> objects = PoolCodeID.CodeIDRead();
                bool sameLabel = false;
                if (!isXmlProcess)
                    foreach (ObjectCodeID o in objects)
                    {
                        RadiationInfo rad = o as RadiationInfo;
                        if (rad is not null && rad.ToString() == value)
                        {
                            sameLabel = true;
                            break;
                        }
                    }
                if (sameLabel)
                    return;
                oldLabel = _label;
                _label = value;
                if (string.IsNullOrWhiteSpace(ToString()))
                {
                    _label = oldLabel;
                    return;
                }
                if (isNew && !isXmlProcess)
                    ChangeCodeID();
                WriteProperty();
                RaisePropertyChanged();
            }
        }
        string oldLabel = null;
        public ObservableCollection<XRayLineInfo> Lines { get => _lines; set { _lines = value; RaisePropertyChanged(); } }
        public string SindarinCode { get => _sindarinCode; set { _sindarinCode = value; WriteProperty(); RaisePropertyChanged(); } }
        /// <summary>
        /// usado para xml
        /// </summary>
        public RadiationInfo() : base() { }
        /// <summary>
        /// usado para criar objeto pelo usuário
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="isNew"></param>
        public RadiationInfo(string folder, bool isNew) : base()
        {
            SetFolder(folder);
            Lines.CollectionChanged += CollectionChanged;
        }
        public RadiationInfo(string file) : base(file)
        {
            Lines.CollectionChanged += CollectionChanged;
        }

        public override void GetPropertyFromFile(string file = null)
        {
            base.GetPropertyFromFile(file);
            if (Lines.Count > 0)
                foreach (XRayLineInfo xr in Lines)
                    xr.PropertyChanged += PropertyChanged;
        }
        protected void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == Lines)
                RaisePropertyChanged("Lines");
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (XRayLineInfo xRay in e.NewItems)
                {
                    xRay.PropertyChanged += PropertyChanged;
                }
            }
            WriteProperty();
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WriteProperty();
        }

        //Caracteres que não são permitidos para nomes de arquivos: <>"/|\*:?
        public override string ToString()
        {
            return Label;
        }
        public object Clone()
        {
            RadiationInfo rad = new RadiationInfo()
            {
                Label = this.Label.Substring(0),
                SindarinCode = this.SindarinCode.Substring(0),
            };
            foreach (XRayLineInfo xr in Lines)
                rad.Lines.Add((XRayLineInfo)xr.Clone());
            return rad;
        }
        public int CompareTo(object obj)
        {
            if (obj is RadiationInfo)
                return this.Label.ToUpper().CompareTo((obj as RadiationInfo).Label.ToUpper());
            else if (obj is string)
                return this.Label.ToUpper().CompareTo((obj as string).ToUpper());
            else
                return this.Label.ToUpper().CompareTo(obj.ToString().ToUpper());
        }
    }
}
