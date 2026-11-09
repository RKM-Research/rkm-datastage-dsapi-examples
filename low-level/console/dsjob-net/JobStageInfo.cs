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
    private static int JobStageInfo(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -stageinfo <project> <job> <stage>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];
        string stage = args[2];

        int status;
        int firstError = (int)DSAPI.ErrorCodes.DSJE_NOERROR;

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

                        // Get stage information except the links, which have a separate command

                        // Report stage type
                        status = DSAPI.DSGetStageInfo(hJob, stage, DSAPI.StageInfoType.DSJ_STAGETYPE, out stageInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            // Convert the native stage type name to a managed string using the DSAPI utility

                            string stageTypeName = DSAPI.Utilities.ExtractStringFromIntPtr(stageInfo.info.typeName);
                            Console.WriteLine($"Stage type\t: {stageTypeName}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get type for job '{job}' stage '{stage}'");
                        }

                        // Report stage input row number
                        status = DSAPI.DSGetStageInfo(hJob, stage, DSAPI.StageInfoType.DSJ_STAGEINROWNUM, out stageInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine($"In Row Number\t: {stageInfo.info.inRowNum}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get input row number for job '{job}' stage '{stage}'");
                        }

                        // Report stage last error (if any)
                        status = DSAPI.DSGetStageInfo(hJob, stage, DSAPI.StageInfoType.DSJ_STAGELASTERR, out stageInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine("Stage Last Error\t:");
                            PrintLogDetail(1, stageInfo.info.lastError);
                        }
                        else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                        {
                            Console.WriteLine("Stage Last Error\t: <none>");
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get last error for job '{job}' stage '{stage}'");
                        }

                        // Preserve the first failed information request in the command exit code
                        if (firstError != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            status = firstError;
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
