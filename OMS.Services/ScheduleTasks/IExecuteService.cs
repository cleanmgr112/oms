using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OMS.Services.ScheduleTasks
{
    public interface IExecuteService
    {
        void Execute(string strMethod, string strClass);
    }
}