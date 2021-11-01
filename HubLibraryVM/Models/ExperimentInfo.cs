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
    public class ExperimentInfo : ProjectInfoBase
    {
        private DateTime date;
        public DateTime Date { get => date; set { date = value; WriteProperty(); RaisePropertyChanged(); } }

        /// <summary>
        /// usado só para o XMLSerialize
        /// </summary>
        public ExperimentInfo() { }
        /// <summary>
        /// Usado para adicionar pelo usuário
        /// </summary>
        /// <param name="parent"></param>
        public ExperimentInfo(ProjectInfo parent) : base(parent)
        {
            this.parent = parent; Children = new();
            date = DateTime.Today;
        }
        /// <summary>
        /// Usado para adicionar automaticamente dos arquivos library
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        public ExperimentInfo(ProjectInfo parent, string folder) : base(parent, folder) { }

        protected override void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged(sender, e);
        }
        public override void ChildChanged(ProjectInfoBase sender, params string[] propertyName)
        {
            base.ChildChanged(sender, propertyName);
            if (propertyName.Length == 0 || propertyName.Contains("SamplesList"))
                RaisePropertyChanged("SamplesList");
        }
    }
}
