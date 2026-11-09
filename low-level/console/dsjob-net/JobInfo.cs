/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using RKM.DataStage;

partial class DsJobExample
{
    private static int JobInfo(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -jobinfo <project> <job>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];

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
                        // Declare structure for job information
                        DSAPI.DSJOBINFO jobInfo;

                        // Get job information except the stage and parameter lists
                        // Those lists are handled by separate commands

                        // Report status
                        status = DSAPI.DSGetJobInfo(hJob, DSAPI.JobInfoType.DSJ_JOBSTATUS, out jobInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            string jobStatus;
                            switch (jobInfo.info.jobStatus)
                            {
                                case (int)DSAPI.JobStatus.DSJS_RUNNING: jobStatus = "RUNNING"; break;
                                case (int)DSAPI.JobStatus.DSJS_RUNOK: jobStatus = "RUN OK"; break;
                                case (int)DSAPI.JobStatus.DSJS_RUNWARN: jobStatus = "RUN with WARNINGS"; break;
                                case (int)DSAPI.JobStatus.DSJS_RUNFAILED: jobStatus = "RUN FAILED"; break;
                                case (int)DSAPI.JobStatus.DSJS_VALOK: jobStatus = "VALIDATED OK"; break;
                                case (int)DSAPI.JobStatus.DSJS_VALWARN: jobStatus = "VALIDATED with WARNINGS"; break;
                                case (int)DSAPI.JobStatus.DSJS_VALFAILED: jobStatus = "VALIDATED FAILED"; break;
                                case (int)DSAPI.JobStatus.DSJS_RESET: jobStatus = "RESET"; break;
                                case (int)DSAPI.JobStatus.DSJS_STOPPED: jobStatus = "STOPPED"; break;
                                case (int)DSAPI.JobStatus.DSJS_NOTRUNNABLE: jobStatus = "NOT COMPILED"; break;
                                case (int)DSAPI.JobStatus.DSJS_NOTRUNNING: jobStatus = "NOT RUNNING"; break;
                                default: jobStatus = "<unknown>"; break;
                            }
                            Console.WriteLine($"Job Status\t: {jobStatus} ({jobInfo.info.jobStatus})");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get status for job '{job}'");
                        }

                        // Report controller
                        status = DSAPI.DSGetJobInfo(hJob, DSAPI.JobInfoType.DSJ_JOBCONTROLLER, out jobInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine($"Job Controller\t: {DSAPI.Utilities.ExtractStringFromIntPtr(jobInfo.info.jobController)}");
                        }
                        else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                        {
                            Console.WriteLine("Job Controller\t: not available");
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get controller for job '{job}'");
                        }

                        // Report start time
                        status = DSAPI.DSGetJobInfo(hJob, DSAPI.JobInfoType.DSJ_JOBSTARTTIMESTAMP, out jobInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            DateTime dtStartTime = DSAPI.Utilities.DateTimeToLocalTime(DSAPI.Utilities.UnixTimestampToDateTime(jobInfo.info.jobStartTime));
                            Console.WriteLine(value: $"Job Start Time\t: {dtStartTime}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get start time for job '{job}'");
                        }

                        // Report wave
                        status = DSAPI.DSGetJobInfo(hJob, DSAPI.JobInfoType.DSJ_JOBWAVENO, out jobInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine($"Job Wave Number\t: {jobInfo.info.jobWaveNumber}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get wave number for job '{job}'");
                        }

                        // Report user status (if any)
                        status = DSAPI.DSGetJobInfo(hJob, DSAPI.JobInfoType.DSJ_USERSTATUS, out jobInfo);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR &&
                            status != (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE &&
                            firstError == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            firstError = status;
                        }
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.WriteLine($"User Status\t: {DSAPI.Utilities.ExtractStringFromIntPtr(jobInfo.info.userStatus)}");
                        }
                        else if (status == (int)DSAPI.ErrorCodes.DSJE_NOT_AVAILABLE)
                        {
                            Console.WriteLine("User Status\t: not available");
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get user status for job '{job}'");
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
