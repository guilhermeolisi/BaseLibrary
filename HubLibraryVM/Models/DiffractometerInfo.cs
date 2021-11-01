using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GosControls.Models
{
    public class DiffractometerInfo : ObjectToFile, ICloneable, IComparable //, IEditableObject //Para o DataGrid https://docs.microsoft.com/pt-br/dotnet/api/system.componentmodel.ieditableobject?view=net-5.0
    {
        private string _brand = string.Empty;
        private string _model = string.Empty;
        private string _institution = string.Empty;
        private string _tag = string.Empty;
        private ObservableCollection<string> radiationsToXml = new();
        private string description = string.Empty;
        private string sindarinCode = string.Empty;
        private ObservableCollection<object> radiations = new();

        private string oldBrand = null;
        private string oldModel = null;
        private string oldInstitution = null;
        private string oldTag = null;

        public string Brand { get => _brand; set { oldBrand = _brand; _brand = value; RaisePropertyChanged(); } }
        public string Model { get => _model; set { oldModel = _model; _model = value; RaisePropertyChanged(); } }
        public string Institution { get => _institution; set { oldInstitution = _institution; _institution = value; RaisePropertyChanged(); } }
        public string Tag { get => _tag; set { oldTag = _tag; _tag = value; RaisePropertyChanged(); } }
        public ObservableCollection<string> RadiationsToXml { get => radiationsToXml; set { radiationsToXml = value; RaisePropertyChanged(); } }
        public string Description { get => description; set { description = value; RaisePropertyChanged(); } }
        public string SindarinCode { get => sindarinCode; set { sindarinCode = value; RaisePropertyChanged(); } }
        [XmlIgnore]
        public ObservableCollection<object> Radiations { get => radiations; set { radiations = value; RaisePropertyChanged(); } }
        /// <summary>
        /// usado apenas para xml
        /// </summary>
        public DiffractometerInfo() : base() { }
        /// <summary>
        /// usado para criar objeto pelo usuário
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="isNew"></param>
        public DiffractometerInfo(string folder, bool isNew) : base()
        {
            SetFolder(folder);
            initial();
        }
        public DiffractometerInfo(string file) : base(file)
        {
            initial();
        }
        private void initial()
        {
            RadiationsToXml.CollectionChanged += CollectionChanged;
            Radiations.CollectionChanged += CollectionChanged;
            foreach (RadiationInfo rad in Radiations)
                rad.PropertyChanged += ChildrenPropertyChanged;
        }
        private string text;
        public override string ToString() => text; //writeString();
        //Caracteres que não são permitidos para nomes de arquivos: <>"/|\*:?
        //Acho que não vou incluir o radiation do tostring
        private string writeString()
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(Brand))
                result += Brand;
            if (!string.IsNullOrWhiteSpace(Model))
            {
                if (!string.IsNullOrWhiteSpace(result))
                    result += " ";
                result += Model;
            }
            if (!string.IsNullOrWhiteSpace(Institution))
            {
                if (!string.IsNullOrWhiteSpace(result))
                    result += "-";
                result += Institution;
            }
            if (!string.IsNullOrWhiteSpace(_tag))
            {
                if (!string.IsNullOrWhiteSpace(result))
                    result += "-";
                result += _tag;
            }

            //Verifica se tem outro objeto com o mesmo identificação
            if (!isNew && !isXmlProcess)
            {
                bool sameLabel = false;
                foreach (ObjectCodeID o in PoolCodeID.CodeIDRead())
                {
                    DiffractometerInfo diff = o as DiffractometerInfo;
                    if (diff is not null)
                        if (diff.ToString() == result)
                        {
                            sameLabel = true;
                            break;
                        }
                }
                if (sameLabel)
                {
                    if (!string.IsNullOrWhiteSpace(oldBrand))
                        _brand = oldBrand;
                    if (!string.IsNullOrWhiteSpace(oldModel))
                        _model = oldModel;
                    if (!string.IsNullOrWhiteSpace(oldInstitution))
                        _institution = oldInstitution;
                    if (!string.IsNullOrWhiteSpace(oldTag))
                        _tag = oldTag;
                }
                if (!string.IsNullOrWhiteSpace(oldBrand))
                    oldBrand = string.Empty;
                if (!string.IsNullOrWhiteSpace(oldModel))
                    oldModel = string.Empty;
                if (!string.IsNullOrWhiteSpace(oldInstitution))
                    oldInstitution = string.Empty;
                if (!string.IsNullOrWhiteSpace(oldTag))
                    oldTag = string.Empty;
            }

            text = result;
            return result;
        }
        public void AddInitialRadiation(RadiationInfo rad)
        {
            Radiations.CollectionChanged -= CollectionChanged;
            radiations.Add(rad);
            RaisePropertyChanged("Radiations");
            Radiations.CollectionChanged += CollectionChanged;
        }
        public void AddInitialRadiation(ObservableCollection<RadiationInfo> rads)
        {
            try
            {
                Radiations.CollectionChanged -= CollectionChanged;

                for (int i = 0; i < radiationsToXml.Count; i++)
                {
                    bool find = false;
                    foreach (RadiationInfo rad in rads)
                    {
                        if (radiationsToXml[i] == rad.CodeID)
                        {
                            radiations.Add(rad);
                            rad.PropertyChanged += ChildrenPropertyChanged;
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                    {
                        radiationsToXml.Remove(radiationsToXml[i]);
                        i--;
                    }
                }
            }
            finally
            {
                Radiations.CollectionChanged += CollectionChanged;
            }
        }
        private void radiationChange(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                foreach (RadiationInfo r in Radiations)
                    if (!RadiationsToXml.Contains(r.CodeID))
                    {
                        RadiationsToXml.Add(r.CodeID);
                        r.PropertyChanged += ChildrenPropertyChanged;
                    }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    RadiationsToXml.Add(((RadiationInfo)e.NewItems[i]).CodeID);
                    ((RadiationInfo)e.NewItems[i]).PropertyChanged += ChildrenPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                    RadiationsToXml.Remove(((RadiationInfo)e.OldItems[i]).CodeID);
            }
        }
        public ObservableCollection<object> GetRadiations()
        {
            return Radiations;
        }
        protected void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == RadiationsToXml)
                RaisePropertyChanged("Radiations");
            if (sender == Radiations)
                radiationChange(e);
            WriteProperty();
        }
        private void ChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //writeString();
        }
        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(Brand) || propertyName == nameof(Model) || propertyName == nameof(Institution) || propertyName == nameof(_tag))
                writeString();
            base.RaisePropertyChanged(propertyName);
            WriteProperty();
        }
        public object Clone()
        {
            DiffractometerInfo dif = new DiffractometerInfo()
            {
                Brand = this.Brand.Substring(0),
                Model = this.Model.Substring(0),
                Institution = this.Institution.Substring(0),
                Tag = this.Tag.Substring(0),
                Description = this.Description.Substring(0),
                SindarinCode = this.SindarinCode.Substring(0),
            };
            foreach (RadiationInfo rad in Radiations)
                dif.Radiations.Add(rad.Clone());
            return dif;
        }
        public int CompareTo(object obj)
        {
            if (string.IsNullOrWhiteSpace(text))
                writeString();

            if (obj is DiffractometerInfo)
                return this.text.ToUpper().CompareTo((obj as DiffractometerInfo).ToString().ToUpper());
            else if (obj is string)
                return this.text.ToUpper().CompareTo((obj as string).ToUpper());
            else
                return this.text.ToUpper().CompareTo(obj.ToString().ToUpper());
        }
    }
}
