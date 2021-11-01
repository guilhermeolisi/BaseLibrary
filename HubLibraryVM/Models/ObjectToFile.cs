using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class ObjectToFile : ObjectCodeID
    {
        protected override string FilePath { get => Path.Combine(Folder, CodeID + ".xml"); }
        protected override string Folder { get; set; }
        //protected override string FolderParent { get; set; }
        /// <summary>
        /// Apenas para o xml
        /// </summary>
        public ObjectToFile() : base() { }
        public ObjectToFile(string folder) : base() { InitialProcess(folder); }
        protected void InitialProcess(string file)
        {
            Folder = Path.GetDirectoryName(file);
            _codeID = Path.GetFileNameWithoutExtension(file);
            PoolCodeID.CodeIDAdd(this);
            XmlInstance();
            GetPropertyFromFile();
        }
        public void SetFolder(string folder)
        {
            Folder = folder;
            XmlInstance();
        }
        protected override void CodeChanged()
        {
            try
            {
                if (notChangeFile || string.IsNullOrWhiteSpace(CodeID) || (!string.IsNullOrWhiteSpace(oldCodeID) && oldCodeID.ToUpper() == CodeID.ToUpper()) || string.IsNullOrWhiteSpace(Folder))
                    return;

                if (!Directory.Exists(Path.Combine(Folder)))
                    return;
                if (File.Exists(Path.Combine(Folder, oldCodeID + ".xml")))
                {
                    if (!File.Exists(FilePath))
                        File.Move(Path.Combine(Folder, oldCodeID + ".xml"), FilePath);
                    else
                    {
                        CodeChanged();
                        return;
                    }
                }
                else
                {
                    WriteProperty();
                }
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
        public override void DeleteObjectFiles()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
