using BaseLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class FileMethods
    {
        public static async Task<string> ExportSelectedProject(string zipPath, ProjectInfoBase[] projectsToExport, string nimlothFolderApplication)
        {
            //https://docs.microsoft.com/pt-br/dotnet/standard/io/how-to-compress-and-extract-files

            if (string.IsNullOrWhiteSpace(zipPath))
                return null;

            string pathToExport = Path.Combine(nimlothFolderApplication, "ToExport");
            string pathToExportPro = Path.Combine(pathToExport, "Projects");
            string pathToExportDiff = Path.Combine(pathToExport, "Diffractometers");
            string pathToExportRad = Path.Combine(pathToExport, "Radiations");
            string objectsWithoutWorks = string.Empty;

            Task task = new(() =>
            {
                string pathTemp;// = pathToExportPro.Substring(0);
                if (Directory.Exists(pathToExport))
                    Directory.Delete(pathToExport, true);
                Directory.CreateDirectory(pathToExport);
                Directory.CreateDirectory(pathToExportPro);
                Queue<DiffractometerInfo> diffs = new();
                Queue<RadiationInfo> rads = new();
                Queue<SampleInfo> sams = new();

                bool hasWorks = false;
                if (projectsToExport != null || projectsToExport.Length > 0)
                    for (int i = 0; i < projectsToExport.Length; i++)
                    {
                        if (projectsToExport[i] is not null)
                        {
                            //Verifica se há algum work active
                            List<WorkInfo> worksToExport = new();
                            if (projectsToExport[i] is not WorkInfo)
                            {
                                List<ProjectInfoBase> projects = new() { projectsToExport[i] };
                                while (projects.Count != 0)
                                {
                                    ProjectInfoBase projectTemp = projects.Last();
                                    projects.Remove(projectTemp);
                                    if (projectTemp.Children != null && projectTemp.Children.Count > 0)
                                    {
                                        foreach (ProjectInfoBase child in projectTemp.Children)
                                            if (child is WorkInfo)
                                                worksToExport.Add(child as WorkInfo);
                                            else
                                                projects.Add(child);
                                    }
                                }
                            }
                            else
                                worksToExport.Add(projectsToExport[i] as WorkInfo);
                            if (worksToExport.Count == 0)
                                objectsWithoutWorks += (string.IsNullOrWhiteSpace(objectsWithoutWorks) ? "" : "; ") + projectsToExport[i].Title;
                            else
                            {
                                if (!hasWorks)
                                    hasWorks = true;
                                //Copia todos os works para a pasta temporária que será utilizada para zipar
                                while (worksToExport.Count > 0)
                                {
                                    pathTemp = pathToExportPro.Substring(0);
                                    List<ProjectInfoBase> projectsPath = new() { worksToExport.Last() };
                                    //Pegar os parents até o ProjectInfo
                                    while (projectsPath.Last().GetParent() is not null)
                                    {
                                        projectsPath.Add(projectsPath.Last().GetParent());
                                    }
                                    //Crias os diretórios dos projects e experiments e works
                                    for (int k = projectsPath.Count - 1; k >= 0; k--)
                                    {
                                        pathTemp = Path.Combine(pathTemp, projectsPath[k].CodeID);
                                        Directory.CreateDirectory(pathTemp);
                                        Directory.SetCreationTime(pathTemp, Directory.GetCreationTime(projectsPath[k].GetFolder()));
                                        Directory.SetLastWriteTime(pathTemp, Directory.GetLastWriteTime(projectsPath[k].GetFolder()));
                                        string propertyPath = Path.Combine(projectsPath[k].GetFolder(), "Properties.xml");
                                        if (File.Exists(propertyPath))
                                        {
                                            File.Copy(propertyPath, Path.Combine(pathTemp, "Properties.xml"), true);
                                            File.SetCreationTime(Path.Combine(pathTemp, "Properties.xml"), File.GetCreationTime(propertyPath));
                                            File.SetLastWriteTime(Path.Combine(pathTemp, "Properties.xml"), File.GetLastWriteTime(propertyPath));
                                        }
                                    }
                                    //Copia a pasta completa do work
                                    BaseLibrary.FileMethods.DirectoryCopy(worksToExport.Last().GetFolder(), pathTemp, true, true);
                                    ObservableCollection<SampleInfo> sTemp = worksToExport.Last().GetSamples();
                                    if (sTemp is not null)
                                        foreach (SampleInfo s in sTemp)
                                            if (!sams.Contains(s))
                                                sams.Enqueue(s);
                                    ObservableCollection<DiffractometerInfo> dTemp = worksToExport.Last().GetDiffractometers();
                                    if (dTemp is not null)
                                        foreach (DiffractometerInfo d in dTemp)
                                            if (!diffs.Contains(d))
                                                diffs.Enqueue(d);
                                    worksToExport.Remove(worksToExport.Last());
                                }
                            }
                        }
                    }
                if (hasWorks)
                {
                    if (diffs.Count > 0)
                    {
                        if (Directory.Exists(pathToExportDiff))
                            Directory.Delete(pathToExportDiff, true);
                        Directory.CreateDirectory(pathToExportDiff);
                        while (diffs.Count > 0)
                        {
                            DiffractometerInfo diff = diffs.Dequeue();
                            ObservableCollection<object> rs = diff.GetRadiations();
                            foreach (object r in rs)
                                if (!rads.Contains(r))
                                    rads.Enqueue((RadiationInfo)r);
                            diff.WriteProperty(Path.Combine(pathToExportDiff, diff.CodeID + ".xml"));
                        }
                        if (rads.Count > 0)
                        {
                            if (Directory.Exists(pathToExportRad))
                                Directory.Delete(pathToExportRad, true);
                            Directory.CreateDirectory(pathToExportRad);

                            while (rads.Count > 0)
                            {
                                RadiationInfo rad = rads.Dequeue();
                                rad.WriteProperty(Path.Combine(pathToExportRad, rad.CodeID + ".xml"));
                            }
                        }
                    }
                    if (sams.Count > 0)
                    {
                        while (sams.Count > 0)
                        {
                            SampleInfo s = sams.Dequeue();
                            s.WriteProperty(Path.Combine(pathToExportPro, s.RelatedProject.CodeID, s.CodeID + ".xml"));
                        }
                    }
                    //string zipPathTemp = Path.Combine(Path.GetDirectoryName(zipPath), Path.GetFileNameWithoutExtension(zipPath));
                    //int j = 2;
                    //while (File.Exists(zipPathTemp + ".exported"))
                    //{
                    //    zipPathTemp = Path.Combine(Path.GetDirectoryName(zipPath), Path.GetFileNameWithoutExtension(zipPath) + " (" + j + ")");
                    //    j++;
                    //}
                    //zipPathTemp += ".exported";

                    //ZipFile.CreateFromDirectory(pathToExport, zipPathTemp);
                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(pathToExport, zipPath);

                }
                if (Directory.Exists(pathToExport))
                    Directory.Delete(pathToExport, true);
            });
            task.Start();
            await task;
            if (!string.IsNullOrWhiteSpace(objectsWithoutWorks))
                return string.Format("Some object don't have a works to be exported: {0}", objectsWithoutWorks);
            else
                return null;
        }
        public static async Task<string> ImportProject(string zipFilePath, bool isReturn, string nimlothFolderApplication, string nimlothFolderProjects, List<SameProjetcInfo> sameWorks)
        {
            string extractedFolder = Path.Combine(nimlothFolderApplication, "Extracted");
            //if (!isReturn)
            //{

            //}
            string pathPro = Path.Combine(extractedFolder, "Projects");
            string pathDiff = Path.Combine(extractedFolder, "Diffractometers");
            string pathRad = Path.Combine(extractedFolder, "Radiations");
            //Não se pode atualizar um observablecolleciton com trheads diferente da view
            bool? isReplace = null;
            string imported = string.Empty;
            Task task = new(() =>
            {
                if (!isReturn)
                {
                    if (Directory.Exists(extractedFolder))
                        Directory.Delete(extractedFolder, true);
                    ZipFile.ExtractToDirectory(zipFilePath, extractedFolder, true);

                    if (!Directory.Exists(pathPro))
                        return;
                    string[] projectEntries = Directory.GetDirectories(pathPro);
                    if (projectEntries is null || projectEntries.Length == 0)
                    {
                        if (FilesUnidentified == null)
                            FilesUnidentified = new();
                        FilesUnidentified.Add(zipFilePath);
                        return;
                    }

                    foreach (string projectEntry in projectEntries)
                    {
                        string projectName = Path.GetFileName(projectEntry); // new DirectoryInfo(projectEntry).Name;
                        string projectPath = Path.Combine(nimlothFolderProjects, projectName);
                        if (!Directory.Exists(projectPath))
                        {
                            Directory.Move(projectEntry, projectPath);
                            VerifyCodeIDChildrens(projectPath);
                        }
                        else
                        {
                            string[] experimentEntries = Directory.GetDirectories(projectEntry);
                            foreach (string experimentEntry in experimentEntries)
                            {
                                string experimentName = Path.GetFileName(experimentEntry);//experimentEntry.Remove(0, projectEntry.Length+1);
                                string experimentPath = Path.Combine(projectPath, experimentName);
                                if (!Directory.Exists(experimentPath))
                                {
                                    if (VerifySameCodeID(experimentName))
                                    {
                                        experimentName = 'E' + Numbers.GenerateCodeID(10);
                                        experimentPath = Path.Combine(projectPath, experimentName);
                                    }
                                    Directory.Move(experimentEntry, experimentPath);
                                    VerifyCodeIDChildrens(projectPath);
                                }
                                else
                                {
                                    string[] workEntries = Directory.GetDirectories(experimentEntry);
                                    foreach (string workEntry in workEntries)
                                    {
                                        string workName = Path.GetFileName(workEntry); //workEntry.Remove(0, experimentEntry.Length+1);
                                        string workPath = Path.Combine(experimentPath, workName);
                                        string workPathTemp = string.Empty;
                                        if (!Directory.Exists(workPath))
                                        {
                                            if (VerifySameCodeID(workName))
                                            {
                                                workName = 'W' + Numbers.GenerateCodeID(10);
                                                workPath = Path.Combine(experimentPath, workName);
                                            }
                                            workPathTemp = workPath;
                                        }
                                        else
                                        {
                                            if (isReplace == null)
                                            {
                                                bool replace;
                                                if (File.Exists(Path.Combine(workPath, "Workfile.sin")))
                                                {
                                                    if (File.Exists(Path.Combine(workEntry, "Workfile.sin")))
                                                        replace = File.GetLastWriteTime(Path.Combine(workPath, "Workfile.sin")) < File.GetLastWriteTime(Path.Combine(workEntry, "Workfile.sin"));
                                                    else
                                                        replace = false;
                                                }
                                                else
                                                {
                                                    if (File.Exists(Path.Combine(workEntry, "Workfile.sin")))
                                                        replace = true;
                                                    else
                                                        continue;
                                                }
                                                //resultWorksError.Add(string.Format("{0}-{1}-{2}", projectName, experimentName, workName));
                                                sameWorks.Add(new SameProjetcInfo() { ToImport = new WorkInfo(null, workEntry), FromLibrary = new WorkInfo(null, workPath), IsReplace = replace, IsBoth = false });
                                            }
                                            else if (isReplace == false)
                                            {
                                                string folderTemp = workPath;
                                                int i = 2;
                                                while (Directory.Exists(folderTemp))
                                                {
                                                    folderTemp = workPath + " (" + i + ")";
                                                }
                                                workPathTemp = folderTemp;
                                            }
                                            else
                                                workPathTemp = workPath;
                                        }
                                        if (!string.IsNullOrWhiteSpace(workPathTemp))
                                        {
                                            if (Directory.Exists(workPathTemp))
                                                Directory.Delete(workPathTemp, true);
                                            BaseLibrary.FileMethods.DirectoryCopy(workEntry, workPathTemp, true, true);
                                            Directory.Delete(workEntry, true);
                                            WorkInfo work = new WorkInfo();
                                            work.GetPropertyFromFile(Path.Combine(workPathTemp, "Properties.xml"));
                                            string name = work.Title;
                                            imported += (string.IsNullOrWhiteSpace(imported) ? "" : "; ") + name;
                                        }
                                    }
                                }
                            }
                        }
                        //TODO deve-se importar Sample e Diffractometer apenas dos works que foram importados e não todos os works
                        string[] samples = Directory.GetFiles(projectEntry);
                        foreach (string sample in samples)
                        {
                            if (Path.GetFileNameWithoutExtension(sample).ToUpper() == "PROPERTIES")
                                continue;
                            string pathSam = Path.Combine(projectPath, Path.GetFileName(sample));
                            if (!File.Exists(pathSam))
                                File.Move(sample, pathSam);
                        }
                    }
                    //TODO deve-se importar Sample e Diffractometer apenas dos works que foram importados e não todos os works
                    if (Directory.Exists(pathDiff))
                    {
                        string[] diffs = Directory.GetFiles(pathDiff);
                        string folderDiff = Path.Combine(Path.GetDirectoryName(nimlothFolderProjects), "Diffractometers");
                        foreach (string diff in diffs)
                        {
                            string diffFilePath = Path.Combine(folderDiff, Path.GetFileName(diff));
                            if (!File.Exists(diffFilePath))
                                File.Move(diff, diffFilePath);
                        }
                    }
                    if (Directory.Exists(pathRad))
                    {
                        string[] rads = Directory.GetFiles(pathRad);
                        string folderRad = Path.Combine(Path.GetDirectoryName(nimlothFolderProjects), "Radiations");
                        foreach (string rad in rads)
                        {
                            string radFilePath = Path.Combine(folderRad, Path.GetFileName(rad));
                            if (!File.Exists(radFilePath))
                                File.Move(rad, radFilePath);
                        }
                    }
                }
                else
                {
                    foreach (SameProjetcInfo info in sameWorks)
                    {
                        string folder = Path.GetDirectoryName(info.FromLibrary.GetFolder());
                        string pathTemp = string.Empty;
                        if (info.IsBoth)
                        {
                            string folderTemp = info.FromLibrary.GetFolder();
                            //int i = 2;
                            if (Directory.Exists(folderTemp))
                            {
                                info.ToImport.ChangeCodeID();
                                pathTemp = Path.Combine(Path.GetDirectoryName(info.FromLibrary.GetFolder()), Path.GetFileName(info.ToImport.GetFolder()));
                                if (info.ToImport.Title == info.FromLibrary.Title)
                                    info.ToImport.Title += "Imported";
                                //folderTemp = info.FromLibrary.GetFolder() + " (" + i + ")";
                                //i++;
                            }
                            //pathTemp = folderTemp;
                        }
                        else if (info.IsReplace)
                        {
                            pathTemp = info.FromLibrary.GetFolder();
                        }
                        if (!string.IsNullOrWhiteSpace(pathTemp))
                        {
                            imported += (string.IsNullOrWhiteSpace(imported) ? "" : "; ") + info.ToImport.Title;

                            if (Directory.Exists(pathTemp))
                                Directory.Delete(pathTemp, true);
                            //Directory.Move(info.ToImport.GetFolder(), pathTemp);
                            BaseLibrary.FileMethods.DirectoryCopy(info.ToImport.GetFolder(), pathTemp, true, true);
                            Directory.Delete(info.ToImport.GetFolder(), true);
                        }
                    }
                    if (Directory.Exists(extractedFolder))
                        Directory.Delete(extractedFolder, true);
                }
            });

            task.Start();
            await task;
            return imported;
        }
        public static bool VerifySameCodeID(ObjectCodeID obj)
        {
            if (obj is not null)
            {
                foreach (ObjectCodeID o in PoolCodeID.CodeIDRead())
                    if (obj.CodeID == o.CodeID)
                        return true;
            }
            return false;
        }
        public static bool VerifySameCodeID(string codeID)
        {
            if (!string.IsNullOrWhiteSpace(codeID))
            {
                foreach (ObjectCodeID o in PoolCodeID.CodeIDRead())
                    if (codeID == o.CodeID)
                        return true;
            }
            return false;
        }
        public static void VerifyCodeIDChildrens(string folderParent)
        {
            string[] folders = Directory.GetDirectories(folderParent);
            if (folders is null || folders.Length == 0)
                return;
            string codeIDParent = Path.GetFileName(folderParent);
            foreach (string folder in folders)
            {
                string codeID = Path.GetFileName(folder);
                bool same = false;
                foreach (ObjectCodeID o in PoolCodeID.CodeIDRead())
                    if (codeID == o.CodeID)
                    {
                        same = true;
                        break;
                    }
                if (same)
                {
                    string newPath = Path.Combine(folder[0] + Numbers.GenerateCodeID(10));
                    Directory.Move(folder, newPath);
                    VerifyCodeIDChildrens(newPath);
                }
            }
        }
        
        private static List<string> FilesUnidentified { get; set; }
        public static bool HasExportedfile { get; private set; }
        //public static List<string> imported { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="workActive"></param>
        /// <param name="filesWork"></param>
        /// <param name="type"></param>
        /// <param name="isReplace"></param>
        /// <returns>SindarinTXTImport, SinText, imported</returns>
        public static async Task<(List<string> Imported, List<string> log)> ProcessFilesToImport(string[] files, string nimlothFolderApplication, string nimlothFolderProjects, List<SameProjetcInfo> sameWorks)
        {
            if (FilesUnidentified != null && FilesUnidentified.Count > 0)
                FilesUnidentified.Clear();

            string newFilePath = string.Empty;
            List<string> imported = new();
            List<string> log = new();
            HasExportedfile = false;
            //Tenta processar primeiro como Data file
            foreach (string file in files)
            {
                if (BaseLibrary.FileMethods.CheckZipFile(file))
                {
                    imported.Add(await ImportProject(file, false, nimlothFolderApplication, nimlothFolderProjects, sameWorks));
                    if (!string.IsNullOrWhiteSpace(imported.Last()))
                    {
                        if (!HasExportedfile)
                            HasExportedfile = true;
                    }
                    else
                        imported.Remove(imported.Last());
                }
                else
                {
                    //Erro, não foi encontrado um tipo adequado
                    if (FilesUnidentified is null)
                        FilesUnidentified = new();
                    FilesUnidentified.Add(file);
                }
            }
            if (FilesUnidentified != null && FilesUnidentified.Count > 0)
            {
                string temp = string.Empty;
                string filesname = string.Empty;
                foreach (string file in FilesUnidentified)
                {
                    filesname += Path.GetFileName(file);
                    if (file != FilesUnidentified.Last())
                        filesname += "; ";
                }
                temp += string.Format("Some files could't be recognized: {0}", filesname);
                log.Add(temp);
            }
            return (imported, log);
        }
    }
}
