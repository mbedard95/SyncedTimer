using Microsoft.AspNetCore.Components;

namespace SyncedTimer.Pages
{
    partial class Timer
    {
        private static event EventHandler<TimerEventArgs> OnTimerChanged;
        private static event EventHandler<SegmentEventArgs> OnSegmentsChanged;
        
        private static bool IsTimerActive { get; set; }
        private static int SharedTotalSeconds { get; set; }
        private static int SegmentSeconds { get; set; }

        private static System.Threading.Timer InternalTimer = new System.Threading.Timer((state) => 
        {
            SharedTotalSeconds++;
            OnTimerChanged?.Invoke(null, new TimerEventArgs(SharedTotalSeconds, IsTimerActive));
        });

        private static System.Threading.Timer InternalSegmentTimer = new System.Threading.Timer((state) => 
        {
            SegmentSeconds++;
        });

        public static List<Segment> Segments { get; set; } = new List<Segment>();

        [Parameter]
        public string DisplayValue { get; set; } = "";

        public Task StartAsync()
        {
            StartTimer();

            SegmentSeconds = 0;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            StopTimer();

            Segments.Add(new Segment { Index = Segments.Count, Seconds = SegmentSeconds });

            OnSegmentsChanged?.Invoke(null, new SegmentEventArgs(Segments));
            
            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            StopTimer();

            SharedTotalSeconds = 0;

            OnTimerChanged?.Invoke(null, new TimerEventArgs(SharedTotalSeconds, IsTimerActive));

            return Task.CompletedTask;
        }

        private static void StartTimer()
        {
            IsTimerActive = true;
            InternalTimer.Change(1000, 1000);
            InternalSegmentTimer.Change(1000, 1000);
        }

        private static void StopTimer()
        {
            IsTimerActive = false;
            InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            InternalSegmentTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public Task RemoveSegmentAsync(int index)
        {
            Segments.RemoveAt(index);

            //rearrange indeces
            for (int i = 0; i < Segments.Count; i++)
            {
                Segments[i].Index = i;
            }

            OnSegmentsChanged?.Invoke(null, new SegmentEventArgs(Segments));

            return Task.CompletedTask;
        }

        public Task ResetSegmentsAsync()
        {
            Segments.Clear();

            OnSegmentsChanged?.Invoke(null, new SegmentEventArgs(Segments));

            return Task.CompletedTask;
        }

        protected override Task OnInitializedAsync()
        {
            OnTimerChanged += Timer_OnTimerChanged;
            OnSegmentsChanged += Timer_OnSegmentsChanged;

            // Calculate the initial value for the timer.
            this.SetDisplayValue();

            return base.OnInitializedAsync();
        }

        private void Timer_OnSegmentsChanged(object? sender, SegmentEventArgs e)
        {
            this.InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }

        private void Timer_OnTimerChanged(object? sender, TimerEventArgs e)
        {
            this.InvokeAsync(() =>
            {
                this.SetDisplayValue();
                this.StateHasChanged();
            });
        }

        private void SetDisplayValue()
        {
            this.DisplayValue = CalculateDisplayValue(SharedTotalSeconds);
        }

        private string GetTotalDisplayValue()
        {
            return CalculateDisplayValue(Segments.Select(x => x.Seconds).Sum());
        }

        public static string CalculateDisplayValue(int totalSeconds)
        {
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;

            return string.Format("{0:00}:{1:00}", mins, secs);
        }
    }

    public class TimerEventArgs : EventArgs
    {
        public int Seconds { get; set; }
        public bool IsActive { get; set; }
        public TimerEventArgs(int seconds, bool isActive)
        {
            this.Seconds = seconds;
            this.IsActive = isActive;
        }        
    }

    public class SegmentEventArgs : EventArgs
    {
        public List<Segment> Segments { get; set; }
        public SegmentEventArgs(List<Segment> segments)
        {
            this.Segments = segments;
        }
    }

    public class Segment
    {
        public int Index { get; set; }
        public int Seconds { get; set; }
    }
}