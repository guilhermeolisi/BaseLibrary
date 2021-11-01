using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public class XRayLineInfo : INotifyPropertyChanged, ICloneable
    {
        private float waveLength;
        private float intensity;

        public float WaveLength { get => waveLength; set { waveLength = value; RaisePropertyChanged(); } }
        public float Intensity { get => intensity; set { intensity = value; RaisePropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Clone()
        {
            return new XRayLineInfo()
            {
                Intensity = this.Intensity,
                WaveLength = this.WaveLength
            };
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
