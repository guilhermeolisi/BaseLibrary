using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GosControls.Models
{
    public class SampleInfo : ObjectToFile, ICloneable, IComparable
    {

        public string Code
        {
            get => _code;
            set
            {
                if ((oldCode is not null && oldCode == value))
                    return;
                List<ObjectCodeID> objects = PoolCodeID.CodeIDRead();
                bool sameLabel = false;
                if (!isXmlProcess)
                    foreach (ObjectCodeID o in objects)
                    {
                        SampleInfo sam = o as SampleInfo;
                        if (sam is not null && sam.ToString() == value)
                        {
                            sameLabel = true;
                            break;
                        }
                    }
                if (sameLabel)
                    return;
                oldCode = _code;
                _code = value;
                if (string.IsNullOrWhiteSpace(ToString()))
                {
                    _code = oldCode;
                    return;
                }
                if (isNew && !isXmlProcess)
                    ChangeCodeID();
                WriteProperty();
                RaisePropertyChanged();
            }
        }
        string oldCode = null;
        public string Description { get => _description; set { _description = value; WriteProperty(); RaisePropertyChanged(); } }
        [XmlIgnore]
        public ProjectInfo RelatedProject
        {
            get => _relatedProject;
            set
            {
                _relatedProject = value;
                if (string.IsNullOrWhiteSpace(Folder)) 
                    Folder = _relatedProject.GetFolder();
                if (string.IsNullOrWhiteSpace(CodeID))
                {
                    XmlInstance();
                    ChangeCodeID();
                }
                RaisePropertyChanged();
            }
        }
        [XmlIgnore]
        public bool IsNew = false;
        public SampleInfo() : base() { }
        public SampleInfo(string folder) : base(folder) { }
        public SampleInfo(ProjectInfo project, string file) : base(file)
        {
            RelatedProject = project;
            //Folder = project.GetFolder();
        }
        public override string ToString() => Code;

        public object Clone()
        {
            SampleInfo sam = new()
            {
                _code = this.Code.Substring(0),
                _description = this.Description.Substring(0),
                _relatedProject = this.RelatedProject
            };
            return sam;
        }
        public int CompareTo(object obj)
        {
            if (obj is SampleInfo)
                return this.Code.ToUpper().CompareTo((obj as SampleInfo).Code.ToUpper());
            else if (obj is string)
                return this.Code.ToUpper().CompareTo((obj as string).ToUpper());
            else
                return this.Code.ToUpper().CompareTo(obj.ToString().ToUpper());
        }

        private string _code = string.Empty;
        private string _description = string.Empty;
        private ProjectInfo _relatedProject;


    }
}
