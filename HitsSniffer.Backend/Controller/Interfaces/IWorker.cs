using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HitsSniffer.Controller.Interfaces
{
    public interface IWorker
    {
        Timer Timer { get; }

        void StartWorking();
    }
}