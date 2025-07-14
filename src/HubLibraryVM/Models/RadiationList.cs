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
    public class RadiationList : INotifyPropertyChanged
    {
        private ObservableCollection<RadiationInfo> Radiations = new();

        public RadiationList()
        {
            Radiations.Add(new RadiationInfo() { Label = "Cu-a1a2" });
            //Radiations[Radiations.Count - 1].Lines.Add(((float)1.54, (float)1.0));
            //Radiations[Radiations.Count - 1].Lines.Add(((float)1.54, (float)0.5));
        }

        public ObservableCollection<RadiationInfo> Radiations1 { get => Radiations; set { Radiations = value; RaisePropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
