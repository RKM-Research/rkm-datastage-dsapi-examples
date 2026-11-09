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
    private static int ListLinks(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -llinks <project> <job> <stage>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];
        string stage = args[2];

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
                        // Declare structure for stage information
                        DSAPI.DSSTAGEINFO stageInfo;

                        // Get the list of stage links
                        status = DSAPI.DSGetStageInfo(hJob, stage, DSAPI.StageInfoType.DSJ_LINKLIST, out stageInfo);
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            PrintStringList(0, DSAPI.Utilities.ExtractStringListFromIntPtr(stageInfo.info.linkList));
                        }
                        else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                        {
                            Console.Write("<none>");
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get stage links for job '{job}' stage '{stage}'");
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
