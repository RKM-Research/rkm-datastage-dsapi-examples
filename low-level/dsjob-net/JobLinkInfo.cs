/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using RKM.DataStage;

partial class DsJobExample
{
    private static int JobLinkInfo(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 4)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -linkinfo <project> <job> <stage> <link>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];
        string stage = args[2];
        string link = args[3];

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
                        // Declare structure for link information
                        DSAPI.DSLINKINFO linkInfo;

                        // Try getting all the link info

                        // Report row counter
                        status = DSAPI.DSGetLinkInfo(hJob, stage, link, DSAPI.LinkInfoType.DSJ_LINKROWCOUNT, out linkInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine($"Link Row Count\t: {linkInfo.info.rowCount}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get link row count for job '{job}' stage '{stage}' link '{link}'");
                        }

                        // Report last error (if any)
                        status = DSAPI.DSGetLinkInfo(hJob, stage, link, DSAPI.LinkInfoType.DSJ_LINKLASTERR, out linkInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine("Link Last Error\t:");
                            PrintLogDetail(1, linkInfo.info.lastError);
                        }
                        else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                        {
                            Console.WriteLine("Link Last Error\t: <none>");
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get link last error for job '{job}' stage '{stage}' link '{link}'");
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
