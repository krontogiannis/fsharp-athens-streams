using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using MBrace.Azure.Runtime;
using MBrace.Azure.Store;
using MBrace.Azure.Runtime.Common;
using MBrace.Store;

namespace MBraceWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private Service _svc;

        public override void Run()
        {
            _svc.Start();
        }

        public override bool OnStart()
        {


            string customTempLocalResourcePath = RoleEnvironment.GetLocalResource("CustomTempLocalStore").RootPath;
            Environment.SetEnvironmentVariable("TMP", customTempLocalResourcePath);
            Environment.SetEnvironmentVariable("TEMP", customTempLocalResourcePath);

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            var config = Configuration.Default
                            .WithStorageConnectionString("")
                            .WithServiceBusConnectionString("");

            

            _svc = new Service(config, serviceId: RoleEnvironment.CurrentRoleInstance.Id);
            _svc.MaxConcurrentTasks = 1;
            _svc.AttachLogger(new CustomLogger(s => Trace.WriteLine(s)));




            return result;
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
