using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class SameProjetcInfo : INotifyPropertyChanged
    {
        private ProjectInfoBase toImport;
        private ProjectInfoBase fromLibrary;
        private bool isReplace;
        private bool isBoth;

        public ProjectInfoBase ToImport { get => toImport; set { toImport = value; RaisePropertyChanged(); } }
        public ProjectInfoBase FromLibrary { get => fromLibrary; set { fromLibrary = value; RaisePropertyChanged(); } }
        public bool IsReplace { get => isReplace; set { isReplace = value; RaisePropertyChanged(); } }
        public bool IsBoth { get => isBoth; set { isBoth = value; RaisePropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
