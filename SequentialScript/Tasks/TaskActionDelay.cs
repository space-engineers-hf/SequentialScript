using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class TaskActionDelay : ITaskAction
    {

        private string _name => $"delay_{DateTime.Now.Ticks}";
        private DateTime? _internalStartTime;

        /// <summary>
        /// Time in miliseconds before this task is done.
        /// </summary>
        public int Delay { get; set; }

        public string ActionKey => _name;
        public DateTime? StartTime { get; set; }

        public bool IsCommandCondition
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public void Execute()
        {
            _internalStartTime = DateTime.UtcNow;
        }

        public bool Check(TaskStatusMode mode, DateTime? momento = null, StringBuilder debug = null)
        {
            bool isCompleted;
            string isCompletedText;

            if (_internalStartTime.HasValue)
            {
                var elapsedTimeSpan = (momento ?? DateTime.UtcNow) - _internalStartTime.Value;
                var remainingTimeSpan = TimeSpan.FromMilliseconds(this.Delay) - elapsedTimeSpan;

                debug?.Append($"Delay: {remainingTimeSpan:hh\\:mm\\:ss}");
                isCompleted = (remainingTimeSpan <= TimeSpan.Zero);
            }
            else
            {
                debug?.Append($"Delay: not started");
                isCompleted = false; //not started yet.
            }
            isCompletedText = (isCompleted ? "Done" : "Pending");
            debug?.Append($" ({isCompletedText})");
            return isCompleted;
        }

    }
}
