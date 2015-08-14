using System;
using System.Collections;
using System.Linq;
using System.Management.Automation.Runspaces;

namespace ServiceDeploy.Agent.PowerShell
{
    internal class Script
    {
        public static bool Execute(string script, IDictionary @params, ref string output)
        {
            using (Runspace rs = RunspaceFactory.CreateRunspace())
            {
                rs.Open();

                using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create())
                {
                    ps.Runspace = rs;

                    ps.AddScript(script);
                    ps.AddParameters(@params);

                    var outputs = ps.Invoke();
                    var errors = ps.Streams.Error;

                    if (errors.Count > 0)
                    {
                        output += string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
                        return false;
                    }

                    output += string.Join(Environment.NewLine, outputs.Select(o => o.BaseObject.ToString()));

                    return true;
                }
            }
        }
    }
}