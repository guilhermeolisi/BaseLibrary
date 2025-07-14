using System;
using System.Collections.Generic;
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
    public class ObjectToFolder : ObjectCodeID
    {
        protected override string FilePath { get { if (string.IsNullOrWhiteSpace(Folder)) return null; return Path.Combine(Folder, "Properties.xml"); } }
        protected override string Folder { get { if (string.IsNullOrWhiteSpace(FolderParent) || string.IsNullOrWhiteSpace(CodeID)) return null; return Path.Combine(FolderParent, CodeID); } }
        protected string _folder;
        protected override string FolderParent { get => GetFolderParent(); } //{ get; set; }
        /// <summary>
        /// Apenas para o xml
        /// </summary>
        public ObjectToFolder() : base() { }
        protected void InitialProcess(string folder)
        {
            //if (this is not ProjectInfoBase)
            //    FolderParent = Path.GetDirectoryName(folder);
            //notChangeFile = true;
            _folder = folder;
            _codeID = Path.GetFileName(folder);
            PoolCodeID.CodeIDAdd(this);
            //notChangeFile = false;
            //string trash = Directory.GetParent(folder).FullName;
            //string trash2 = Path.GetDirectoryName(folder);
            //string trash3 = Path.GetRelativePath(folder, Path.GetDirectoryName(folder));
            //string trash4 = Path.GetFileName(folder);
            XmlInstance();
            GetPropertyFromFile();
        }
        protected void SetParentFolder(string parentFolder)
        {
            //FolderParent = parentFolder;
            XmlInstance();
        }
        public virtual string GetFolderParent() => throw new NotImplementedException();
        protected override void CodeChanged()
        {
            try
            {
                if (notChangeFile || string.IsNullOrWhiteSpace(CodeID) || (!string.IsNullOrWhiteSpace(oldCodeID) && oldCodeID.ToUpper() == CodeID.ToUpper()) || string.IsNullOrWhiteSpace(Folder))
                    return;

                if (Directory.Exists(Path.Combine(Folder)))
                {
                    //Isto aqui seria algo muito improvável de acontecer
                    if (oldCodeID == null)
                    {
                        ChangeCodeID();
                        //ProjectNotification(new ProjectNotificationEventArgs(this) { AlertMessage = string.Format("There is other {0} with the same title ({1}) in {2}{3}", thisType, Title, TypeName, parentName) });
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(CodeID) && CodeID != Path.GetFileName(Folder))
                    {
                        Directory.Move(Folder, Path.Combine(Path.GetDirectoryName(Folder), CodeID));
                        _folder = Path.Combine(Path.GetDirectoryName(Folder), CodeID);
                        return;
                    }
                    else
                    {
                        Message = string.Format("There other {0} object with same internal identification (ID)", TypeName);
                        //Directory.Move(Path.Combine(FolderParent, oldCodeID), Path.Combine(FolderParent, oldCodeID + "_Temp_99"));
                        //oldCodeID = oldCodeID + "_Temp_99";
                    }
                }
                if (string.IsNullOrWhiteSpace(oldCodeID))
                    Directory.CreateDirectory(Path.Combine(Folder));
                else
                {
                    Directory.Move(Path.Combine(FolderParent, oldCodeID), Path.Combine(Folder));
                }
                FolderChanged();
                oldCodeID = string.Empty;
            }
            catch (Exception e) { }
            finally
            {
                if ((!notChangeFile && oldCodeID == null) || !string.IsNullOrWhiteSpace(oldCodeID))
                    _codeID = oldCodeID;
                oldCodeID = string.Empty;
            }
        }
        protected virtual void FolderChanged() => throw new NotImplementedException();
        public override void DeleteObjectFiles()
        {
            if (Directory.Exists(Folder))
                Directory.Delete(Folder, true);
        }
    }
}
