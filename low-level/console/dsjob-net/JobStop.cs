/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using System;
using System.Collections.Generic;

using RKM.DataStage;

partial class DsJobExample
{
    private static int JobStop(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -stop <project> <job>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];

        int status;

        // Attempt to open the project and the job
        DSAPI.DSPROJECT hProject = DSAPI.DSOpenProject(project);
        if (hProject.HasValue)
        {
            try
            {
                DSAPI.DSJOB hJob = DSAPI.DSOpenJob(hProject, job);
                if (hJob.HasValue)
                {
                    try
                    {
                        // Now stop the job
                        status = DSAPI.DSStopJob(hJob);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine($"ERROR: Failed to stop job '{job}'");
                        }
                    }
                    finally
                    {
                        DSAPI.DSCloseJob(hJob);
                    }
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: Failed to open job '{job}'");
                    status = DSAPI.DSGetLastError();
                }
            }
            finally
            {
                DSAPI.DSCloseProject(hProject);
            }
        }
        else
        {
            Console.Error.WriteLine($"ERROR: Failed to open project '{project}'");
            status = DSAPI.DSGetLastError();
        }

        return status;
    }
}
