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
    public static int ListJobs(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -ljobs <project>");
            return DSJOB_ERROR;
        }
        string project = args[0];

        int status;

        // Attempt to open the project
        DSAPI.DSPROJECT hProject = DSAPI.DSOpenProject(project);
        if (hProject.HasValue)
        {
            try
            {
                // Declare structure for project information
                DSAPI.DSPROJECTINFO projectInfo;

                // Get the list of jobs
                status = DSAPI.DSGetProjectInfo(hProject, DSAPI.ProjectInfoType.DSJ_JOBLIST, out projectInfo);
                if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                {
                    PrintStringList(0, DSAPI.Utilities.ExtractStringListFromIntPtr(projectInfo.info.jobList));
                }
                else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                {
                    Console.WriteLine("<none>");
                    status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: Failed to get job list for project '{project}'");
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
