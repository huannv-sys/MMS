using System;
using System.ComponentModel;

namespace MikroTikMonitor.Models
{
    /// <summary>
    /// Represents a MikroTik simple queue configuration
    /// </summary>
    public class SimpleQueue : ModelBase, ITikEntity
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
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the target
        /// </summary>
        public string Target
        {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        /// <summary>
        /// Gets or sets the parent
        /// </summary>
        public string Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        /// <summary>
        /// Gets or sets the max limit
        /// </summary>
        public string MaxLimit
        {
            get => _maxLimit;
            set => SetProperty(ref _maxLimit, value);
        }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public string Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        /// <summary>
        /// Gets or sets the burst
        /// </summary>
        public string Burst
        {
            get => _burst;
            set => SetProperty(ref _burst, value);
        }

        /// <summary>
        /// Gets or sets the burst time
        /// </summary>
        public string BurstTime
        {
            get => _burstTime;
            set => SetProperty(ref _burstTime, value);
        }

        /// <summary>
        /// Gets or sets the burst threshold
        /// </summary>
        public string BurstThreshold
        {
            get => _burstThreshold;
            set => SetProperty(ref _burstThreshold, value);
        }

        /// <summary>
        /// Gets or sets whether the queue is disabled
        /// </summary>
        public bool Disabled
        {
            get => _disabled;
            set => SetProperty(ref _disabled, value);
        }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }


    }
}