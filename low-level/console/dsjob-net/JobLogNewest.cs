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
    public static int JobLogNewest(string[] args)
    {
        string project, job;
        DSAPI.LogEventType type;

        // Validate arguments and extract optional arguments
        if (!IsValidNewestArgs(args, out project, out job, out type))
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -lognewest <project> <job> [<event type>]");
            Console.Error.WriteLine("\t event type = INFO | WARNING | FATAL | REJECT | STARTED | RESET | BATCH | OTHER");
            return DSJOB_ERROR;
        }

        int status = 0;

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
                        // Get the newest id
                        int eventId = DSAPI.DSGetNewestLogId(hJob, type);
                        if (eventId >= 0)
                        {
                            Console.WriteLine($"Newest id = {eventId}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get newest log id for job '{job}' event type '{type}'");
                            status = DSAPI.DSGetLastError();
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

    private static bool IsValidNewestArgs(string[] args, out string projectName, out string jobName, out DSAPI.LogEventType eventType)
    {
        projectName = jobName = null;
        eventType = DSAPI.LogEventType.DSJ_LOGANY;

        int argc = args.Length;
        if (argc < 2 || argc > 3)
        {
            return false;
        }

        projectName = args[0];
        jobName = args[1];

        if (argc == 3)
        {
            switch (args[2])
            {
                case "INFO": eventType = DSAPI.LogEventType.DSJ_LOGINFO; break;
                case "WARNING": eventType = DSAPI.LogEventType.DSJ_LOGWARNING; break;
                case "FATAL": eventType = DSAPI.LogEventType.DSJ_LOGFATAL; break;
                case "REJECT": eventType = DSAPI.LogEventType.DSJ_LOGREJECT; break;
                case "STARTED": eventType = DSAPI.LogEventType.DSJ_LOGSTARTED; break;
                case "RESET": eventType = DSAPI.LogEventType.DSJ_LOGRESET; break;
                case "BATCH": eventType = DSAPI.LogEventType.DSJ_LOGBATCH; break;
                case "OTHER": eventType = DSAPI.LogEventType.DSJ_LOGOTHER; break;
                default: return false;
            }
        }

        return true;
    }
}
