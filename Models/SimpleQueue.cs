using System;
using System.ComponentModel;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a MikroTik simple queue configuration
    /// </summary>
    public class SimpleQueue : INotifyPropertyChanged, ITikEntity
    {
        private string _id;
        private string _name;
        private string _target;
        private string _parent;
        private string _maxLimit;
        private string _priority;
        private string _burst;
        private string _burstTime;
        private string _burstThreshold;
        private bool _disabled;
        private string _comment;

        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Gets or sets the target
        /// </summary>
        public string Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    _target = value;
                    OnPropertyChanged(nameof(Target));
                }
            }
        }

        /// <summary>
        /// Gets or sets the parent
        /// </summary>
        public string Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        /// <summary>
        /// Gets or sets the max limit
        /// </summary>
        public string MaxLimit
        {
            get => _maxLimit;
            set
            {
                if (_maxLimit != value)
                {
                    _maxLimit = value;
                    OnPropertyChanged(nameof(MaxLimit));
                }
            }
        }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public string Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        /// <summary>
        /// Gets or sets the burst
        /// </summary>
        public string Burst
        {
            get => _burst;
            set
            {
                if (_burst != value)
                {
                    _burst = value;
                    OnPropertyChanged(nameof(Burst));
                }
            }
        }

        /// <summary>
        /// Gets or sets the burst time
        /// </summary>
        public string BurstTime
        {
            get => _burstTime;
            set
            {
                if (_burstTime != value)
                {
                    _burstTime = value;
                    OnPropertyChanged(nameof(BurstTime));
                }
            }
        }

        /// <summary>
        /// Gets or sets the burst threshold
        /// </summary>
        public string BurstThreshold
        {
            get => _burstThreshold;
            set
            {
                if (_burstThreshold != value)
                {
                    _burstThreshold = value;
                    OnPropertyChanged(nameof(BurstThreshold));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the queue is disabled
        /// </summary>
        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    OnPropertyChanged(nameof(Disabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}