using BaseLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GosControls.Models
{
    //https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
    public static class ManagerFiles
    {
        static XmlSerializer xmlDiffractometers = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        //public static void ReadDiffractometers(string pathFile, ref ObservableCollection<DiffractometerInfo> list)
        //{
        //    if (!File.Exists(pathFile)) return;
        //    string text = FileProcess.ReadTXT(pathFile);
        //    if (string.IsNullOrWhiteSpace(text))
        //        return;
        //    using (StringReader stringReader = new StringReader(text))
        //    {
        //        using (XmlReader xmlreader = new XmlTextReader(stringReader))
        //        {
        //            //Encoding t = xmlreader.Encoding;
        //            if (xmlDiffractometers.CanDeserialize(xmlreader))
        //            {
        //                ObservableCollection<DiffractometerInfo> temp = (ObservableCollection<DiffractometerInfo>)xmlDiffractometers.Deserialize(xmlreader);
        //                foreach (DiffractometerInfo t in temp)
        //                    list.Add(t);
        //            }
        //        }
        //    }
        //}
        public static async Task<ObservableCollection<DiffractometerInfo>> ReadDiffractometersAsync(string pathFile)
        {
            ObservableCollection<DiffractometerInfo> result = new();

            if (!File.Exists(pathFile)) return result;

            string text = await FileProcess.ReadTXTAsync(pathFile);
            if (string.IsNullOrWhiteSpace(text))
                return result;
            //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
            using (StringReader stringReader = new StringReader(text))
            {
                using (XmlReader xmlreader = new XmlTextReader(text))
                {
                    if (xmlDiffractometers.CanDeserialize(xmlreader))
                    {
                        result = (ObservableCollection<DiffractometerInfo>)xmlDiffractometers.Deserialize(xmlreader);
                    }
                }
            }

            return result;
        }
        public static void WriteDiffractometers(string pathFile, ObservableCollection<DiffractometerInfo> list)
        {
            //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
            using (StreamWriter writer = new StreamWriter(pathFile))//TODO fazer estes arquivos hidden
            {
                xmlDiffractometers.Serialize(writer, list);
            }
        }
        static string diffTemp = string.Empty;
        static Task diffractometerTask;
        static bool isDiffractometerChanged = false;
        //public static async Task WriteDiffractometersAsync(string pathFile, ObservableCollection<DiffractometerInfo> list)
        //{
        //    isDiffractometerChanged = true;
        //    if (diffractometerTask == null || diffractometerTask.IsCompleted)
        //    {
        //        while (isDiffractometerChanged)
        //        {
        //            isDiffractometerChanged = false;
        //            //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
        //            using (StringWriter writer = new StringWriter())//TODO fazer estes arquivos hidden
        //            {
        //                xmlDiffractometers.Serialize(writer, list);
        //                if (writer.ToString().CompareTo(diffTemp) == 0) return;
        //                diffTemp = writer.ToString();
        //                diffractometerTask = new Task(() => FileProcess.WriteTXTAsync(pathFile, diffTemp));
        //                diffractometerTask.Start();
        //                await diffractometerTask;
        //            }
        //        }
        //    }
        //}
        static XmlSerializer xmlDiffractometer = new XmlSerializer(typeof(DiffractometerInfo), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        public static void ReadDiffractometers(string pathFolder, ref ObservableCollection<DiffractometerInfo> list)
        {
            if (!Directory.Exists(pathFolder)) 
                return;
            string[] files = Directory.GetFiles(pathFolder);
            if (files == null || files.Length == 0)
                return;
            foreach(string file in files)
            {
                DiffractometerInfo diffTemp = ReadDiffractometer(file);
                if (diffTemp != null)
                    list.Add(diffTemp);
            }
        }
        public static DiffractometerInfo ReadDiffractometer(string pathFile)
        {
            if (!File.Exists(pathFile)) 
                return null;
            string text = FileProcess.ReadTXT(pathFile);
            if (string.IsNullOrWhiteSpace(text))
            {
                File.Delete(pathFile);
                return null;
            }
            DiffractometerInfo result = null;
            using (StringReader stringReader = new StringReader(text))
            {
                using (XmlReader xmlreader = new XmlTextReader(stringReader))
                {
                    //Encoding t = xmlreader.Encoding;
                    if (xmlDiffractometer.CanDeserialize(xmlreader))
                    {
                        result = (DiffractometerInfo)xmlDiffractometer.Deserialize(xmlreader);
                    }
                }
            }
            return result;
        }
        static Queue<DiffractometerInfo> diffs = new();
        public static async Task WriteDiffractometerAsync(string pathFolder, DiffractometerInfo diff)
        {
            //isDiffractometerChanged = true;
            diffs.Enqueue(diff);
            if (diffractometerTask == null || diffractometerTask.IsCompleted)
            {
                while (diffs.Count > 0)
                {
                    //isDiffractometerChanged = false;
                    //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(DiffractometerInfo), typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
                    using (StringWriter writer = new StringWriter())//TODO fazer estes arquivos hidden
                    {
                        DiffractometerInfo diffTemp = diffs.Dequeue();
                        xmlDiffractometer.Serialize(writer, diffTemp);
                        //if (writer.ToString().CompareTo(diffTemp) == 0) return;
                        //diffTemp = writer.ToString();
                        diffractometerTask = new Task(() => FileProcess.WriteTXTAsync(Path.Combine(pathFolder, diffTemp.ToString() + ".xml"), writer.ToString()));
                        diffractometerTask.Start();
                        await diffractometerTask;
                    }
                }
            }
        }
        //static XmlSerializer xmlRadiations = new XmlSerializer(typeof(ObservableCollection<RadiationInfo>), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        //public static void ReadRadiations(string pathFile, ref ObservableCollection<RadiationInfo> list)
        //{
        //    if (!File.Exists(pathFile)) return;
        //    string text = FileProcess.ReadTXT(pathFile);
        //    if (string.IsNullOrWhiteSpace(text))
        //        return;
        //    using (StringReader stringReader = new StringReader(text))
        //    {
        //        using (XmlReader xmlreader = new XmlTextReader(stringReader))
        //        {
        //            //Encoding t = xmlreader.Encoding;
        //            if (xmlRadiations.CanDeserialize(xmlreader))
        //            {
        //                ObservableCollection<RadiationInfo> temp = (ObservableCollection<RadiationInfo>)xmlRadiations.Deserialize(xmlreader);
        //                foreach (RadiationInfo t in temp)
        //                    list.Add(t);
        //            }
        //        }
        //    }
        //}
        public static void ReadRadiations(string pathFolder, ref ObservableCollection<RadiationInfo> list)
        {
            if (!Directory.Exists(pathFolder)) 
                return;
            string[] files = Directory.GetFiles(pathFolder);
            if (files == null || files.Length == 0)
                return;

            foreach (string file in files)
            {
                RadiationInfo rad = ReadRadiation(file);
                if (rad != null)
                    list.Add(rad);
            }
        }
        //TODO acho que estes procedimentos de radiation podem ser retirados
        static XmlSerializer xmlRadiation = new XmlSerializer(typeof(RadiationInfo), new Type[] { typeof(XRayLineInfo) });
        public static RadiationInfo ReadRadiation(string pathFile)
        {
            if (!File.Exists(pathFile))
                return null;
            string text = FileProcess.ReadTXT(pathFile);
            if (string.IsNullOrWhiteSpace(text))
                return null;
            RadiationInfo result = null;
            using (StringReader stringReader = new StringReader(text))
            {
                using (XmlReader xmlreader = new XmlTextReader(stringReader))
                {
                    if (xmlRadiation.CanDeserialize(xmlreader))
                    {
                        result = (RadiationInfo)xmlRadiation.Deserialize(xmlreader);
                    }
                }
            }
            return result;
        }
        //public static async Task<ObservableCollection<RadiationInfo>> ReadRadiationsAsync(string pathFile)
        //{
        //    ObservableCollection<RadiationInfo> result = new();

        //    if (!File.Exists(pathFile)) return result;

        //    string text = await FileProcess.ReadTXTAsync(pathFile);
        //    if (string.IsNullOrWhiteSpace(text))
        //        return result;

        //    //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<RadiationInfo>), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a leitura async
        //    using (StringReader stringReader = new StringReader(text))
        //    {
        //        using (XmlReader xmlreader = new XmlTextReader(text))
        //        {
        //            if (xmlRadiations.CanDeserialize(xmlreader))
        //            {
        //                result = (ObservableCollection<RadiationInfo>)xmlRadiations.Deserialize(xmlreader);
        //            }
        //        }
        //    }

        //    return result;
        //}
        //public static void WriteRadiations(string pathFile, ObservableCollection<RadiationInfo> list)
        //{
        //    //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<DiffractometerInfo>), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
        //    using (StreamWriter writer = new StreamWriter(pathFile))//TODO fazer estes arquivos hidden
        //    {
        //        xmlRadiations.Serialize(writer, list);
        //    }
        //}
        static string radTextTemp = string.Empty;
        static Task radTask;
        static bool isRadChanged = false;
        //public static async Task WriteRadiationsAsync(string pathFile, ObservableCollection<RadiationInfo> list)
        //{
        //    isRadChanged = true;
        //    if (radTask == null || radTask.IsCompleted)
        //    {
        //        while (isRadChanged)
        //        {
        //            isRadChanged = false;
        //            //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<RadiationInfo>), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
        //            using (StringWriter writer = new StringWriter())//TODO fazer estes arquivos hidden
        //            {
        //                xmlRadiations.Serialize(writer, list);
        //                if (writer.ToString().CompareTo(radTemp) == 0) return;
        //                radTemp = writer.ToString();
        //                radTask = new Task(() => FileProcess.WriteTXTAsync(pathFile, radTemp));
        //                radTask.Start();
        //                await radTask;
        //            }
        //        }
        //    }
        //}
        static Queue<RadiationInfo> rads = new();
        public static async Task WriteRadiationAsync(string pathFolder, RadiationInfo rad)
        {
            //isRadChanged = true;
            rads.Enqueue(rad);
            if (radTask == null || radTask.IsCompleted)
            {
                while (rads.Count > 0)
                {
                    //isRadChanged = false;
                    //XmlSerializer xml = new XmlSerializer(typeof(ObservableCollection<RadiationInfo>), new Type[] { typeof(RadiationInfo), typeof(XRayLineInfo) }); //TODO fazer a gravação async
                    using (StringWriter writer = new StringWriter())//TODO fazer estes arquivos hidden
                    {
                        RadiationInfo radTemp = rads.Dequeue();
                        xmlRadiation.Serialize(writer, radTemp);
                        //if (writer.ToString().CompareTo(radTextTemp) == 0) return;
                        //radTextTemp = writer.ToString();
                        radTask = new Task(() => FileProcess.WriteTXTAsync(Path.Combine(pathFolder, radTemp.ToString() + ".xml"), writer.ToString()));
                        radTask.Start();
                        await radTask;
                    }
                }
            }
        }
    }
}
