using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
        /// <summary>
        /// Suspends the specified thread.
        /// </summary>
        public void Suspend()
        {
            Internals.SuspendThread(this.ThreadHandle);
        }

        /// <summary>
        /// Decrements a thread's suspend count.
        /// When the suspend count is decremented to zero, the execution of the thread is resumed.
        /// </summary>
        public void Resume()
        {
            Internals.ResumeThread(this.ThreadHandle);
        }
    }
}
