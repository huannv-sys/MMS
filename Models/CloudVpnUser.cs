using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikroTikMonitor.Models
{
    public class CloudVpnUser : INotifyPropertyChanged
    {
        private string _id;
        private string _username;
        private string _email;
        private string _status;
        private DateTime _createdOn;
        private DateTime? _lastActive;
        private bool _disabled;
        private string _comment;

        public string Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Username 
        { 
            get => _username; 
            set => SetProperty(ref _username, value); 
        }
        
        public string Email 
        { 
            get => _email; 
            set => SetProperty(ref _email, value); 
        }
        
        public string Status 
        { 
            get => _status; 
            set => SetProperty(ref _status, value); 
        }
        
        public DateTime CreatedOn 
        { 
            get => _createdOn; 
            set => SetProperty(ref _createdOn, value); 
        }
        
        public DateTime? LastActive 
        { 
            get => _lastActive; 
            set => SetProperty(ref _lastActive, value); 
        }
        
        public bool Disabled 
        { 
            get => _disabled; 
            set => SetProperty(ref _disabled, value); 
        }
        
        public string Comment 
        { 
            get => _comment; 
            set => SetProperty(ref _comment, value); 
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