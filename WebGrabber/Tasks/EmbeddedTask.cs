using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebGrabber.Tasks
{
    public abstract class EmbeddedTask : BaseTask
    {
        public TasksContainer Container { set; get; }
    }
}
