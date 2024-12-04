using ServerCore;

namespace Server;

struct JobTimerElem : IComparable<JobTimerElem> {
    public int execTick;
    public Action action;

    public int CompareTo(JobTimerElem other) {
        return execTick - other.execTick;
    }
}

public class JobTimer {
    PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
    object _lock = new object();
    int _tick = 0;
    
    public static JobTimer Instance { get; } = new JobTimer();
    
    public void Push(Action action, int tickAfter = 0) {
        JobTimerElem job;
        job.execTick = System.Environment.TickCount + tickAfter;
        job.action = action;
        lock (_lock) {
            _pq.Push(job);
        }
    }
    
    public void Flush() {
        lock (_lock) {
            while (_pq.Count > 0) {
                int now = System.Environment.TickCount;
                JobTimerElem job;

                lock (_lock) {
                    if (_pq.Count == 0) {
                        break;
                    }
                    job = _pq.Peek();
                    if (job.execTick > now) {
                        break;
                    }
                    _pq.Pop();
                }
                job.action.Invoke();
            }
        }
    }
    
    
}