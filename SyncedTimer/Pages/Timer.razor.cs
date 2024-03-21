using Microsoft.AspNetCore.Components;
using SyncedTimer.Models;

namespace SyncedTimer.Pages
{
    partial class Timer
    {
        private static event EventHandler<EventArgs> OnTimerChanged;
        private static event EventHandler<EventArgs> OnSegmentsChanged;
        
        private static bool IsTimerActive { get; set; }
        private static int SharedTotalSeconds { get; set; }
        private static int SegmentSeconds { get; set; }

        private static readonly System.Threading.Timer InternalTimer = new((state) => 
        {
            SharedTotalSeconds++;
            SegmentSeconds++;
            OnTimerChanged?.Invoke(null, new EventArgs());
        });

        public static List<Segment> Segments { get; set; } = new List<Segment>();

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

            OnSegmentsChanged?.Invoke(null, new EventArgs());   
            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            StopTimer();

            SharedTotalSeconds = 0;

            OnTimerChanged?.Invoke(null, new EventArgs());
            return Task.CompletedTask;
        }

        private static void StartTimer()
        {
            IsTimerActive = true;
            InternalTimer.Change(1000, 1000);
        }

        private static void StopTimer()
        {
            IsTimerActive = false;
            InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public Task RemoveSegmentAsync(int index)
        {
            Segments.RemoveAt(index);

            //rearrange indeces
            for (int i = 0; i < Segments.Count; i++)
            {
                Segments[i].Index = i;
            }

            OnSegmentsChanged?.Invoke(null, new EventArgs());
            return Task.CompletedTask;
        }

        public Task ResetSegmentsAsync()
        {
            Segments.Clear();

            OnSegmentsChanged?.Invoke(null, new EventArgs());
            return Task.CompletedTask;
        }

        protected override Task OnInitializedAsync()
        {
            OnTimerChanged += Timer_OnChange;
            OnSegmentsChanged += Timer_OnChange;

            return base.OnInitializedAsync();
        }

        private void Timer_OnChange(object? sender, EventArgs e)
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        private static string GetTotalDisplayValue()
        {
            return CalculateDisplayValue(Segments.Select(x => x.Seconds).Sum());
        }

        private static string CalculateDisplayValue(int totalSeconds)
        {
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;

            return string.Format("{0:00}:{1:00}", mins, secs);
        }
    }
}