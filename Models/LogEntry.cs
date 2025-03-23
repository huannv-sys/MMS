using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class LogEntry : INotifyPropertyChanged
    {
        private string _id;
        private DateTime _time;
        private string _topics;
        private string _message;
        private string _facility;
        private string _severity;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public DateTime Time 
        { 
            get => _time; 
            set => SetProperty(ref _time, value); 
        }
        
        public string Topics 
        { 
            get => _topics; 
            set => SetProperty(ref _topics, value); 
        }
        
        public string Message 
        { 
            get => _message; 
            set => SetProperty(ref _message, value); 
        }
        
        public string Facility 
        { 
            get => _facility; 
            set => SetProperty(ref _facility, value); 
        }
        
        public string Severity 
        { 
            get => _severity; 
            set => SetProperty(ref _severity, value); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}