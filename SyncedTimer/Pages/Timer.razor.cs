using Microsoft.AspNetCore.Components;

namespace SyncedTimer.Pages
{
    partial class Timer
    {
        private static event EventHandler<TimerEventArgs> OnTimerChanged;
        private static event EventHandler<SegmentEventArgs> OnSegmentsChanged;

        private static int SharedTotalSeconds { get; set; }
        private static int SegmentSeconds { get; set; }

        private static System.Threading.Timer InternalTimer = new System.Threading.Timer((state) => 
        {
            SharedTotalSeconds++;
            OnTimerChanged?.Invoke(null, new TimerEventArgs(SharedTotalSeconds));
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
            InternalTimer.Change(1000, 1000);
            InternalSegmentTimer.Change(1000, 1000);

            SegmentSeconds = 0;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            InternalSegmentTimer.Change(Timeout.Infinite, Timeout.Infinite);

            Segments.Add(new Segment { Index = Segments.Count, Seconds = SegmentSeconds });

            OnSegmentsChanged?.Invoke(null, new SegmentEventArgs(Segments));
            
            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            InternalSegmentTimer.Change(Timeout.Infinite, Timeout.Infinite);

            SharedTotalSeconds = 0;

            OnTimerChanged?.Invoke(null, new TimerEventArgs(SharedTotalSeconds));
            
            return Task.CompletedTask;
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

        protected override Task OnInitializedAsync()
        {
            OnTimerChanged += (o, e) =>
            {
                this.InvokeAsync(() =>
                {
                    this.SetDisplayValue();
                    this.StateHasChanged();
                });
            };

            OnSegmentsChanged += (o, e) =>
            {
                this.InvokeAsync(() =>
                {
                    this.StateHasChanged();
                });
            };

            // Calculate the initial value for the timer.
            this.SetDisplayValue();

            return base.OnInitializedAsync();
        }

        private void Timer_OnSegmentsChanged(object? sender, SegmentEventArgs e)
        {
            throw new NotImplementedException();
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
        public TimerEventArgs(int seconds)
        {
            this.Seconds = seconds;
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