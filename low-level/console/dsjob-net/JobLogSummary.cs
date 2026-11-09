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
    private static int JobLogSummary(string[] args)
    {
        string project, job;
        DSAPI.LogEventType type;
        int max;

        // Validate arguments and extract optional arguments
        if (!IsValidLogSummaryArgs(args, out project, out job, out type, out max))
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -logsum");
            Console.Error.WriteLine("\t\t\t[-type <INFO | WARNING | FATAL | REJECT | STARTED | RESET | BATCH | OTHER>]");
            Console.Error.WriteLine("\t\t\t[-max <n>]");
            Console.Error.WriteLine("\t\t\t<project> <job>");
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
                        // Declare structure for returning log event
                        DSAPI.DSLOGEVENT logEvent;

                        // Make the first call to establish the log info
                        status = DSAPI.DSFindFirstLogEntry(hJob, type, 0, 0, max, out logEvent);
                        while (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            PrintLogSummary(logEvent);

                            // Go on to next entry
                            status = DSAPI.DSFindNextLogEntry(hJob, out logEvent);
                        }

                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOMORE)
                        {
                            status = (int)DSAPI.ErrorCodes.DSJE_NOERROR;
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get log summary for job '{job}' event type '{type}'");
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

    // Prints a log event to the console
    private static void PrintLogSummary(DSAPI.DSLOGEVENT logEvent)
    {
        string eventType = null;
        switch (logEvent.type)
        {
            case (int)DSAPI.LogEventType.DSJ_LOGINFO: eventType = "INFO"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGWARNING: eventType = "WARNING"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGFATAL: eventType = "FATAL"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGREJECT: eventType = "REJECT"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGSTARTED: eventType = "STARTED"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGRESET: eventType = "RESET"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGBATCH: eventType = "BATCH"; break;
            case (int)DSAPI.LogEventType.DSJ_LOGOTHER: eventType = "OTHER"; break;
            default: eventType = "<unknown>"; break;
        }
        Console.Write($"{logEvent.eventId}\t{eventType}");

        DateTime dt = DSAPI.Utilities.UnixTimestampToDateTime(logEvent.timestamp);
        DateTime dtLocal = DSAPI.Utilities.DateTimeToLocalTime(dt);
        Console.WriteLine($"\t{dtLocal}");

        string eventMessage = DSAPI.Utilities.ExtractStringFromIntPtr(logEvent.message);
        Console.WriteLine($"\t{eventMessage}");
    }

    private static bool IsValidLogSummaryArgs(string[] args, out string projectName, out string jobName, out DSAPI.LogEventType eventType, out int maxNumber)
    {
        projectName = jobName = null;
        eventType = DSAPI.LogEventType.DSJ_LOGANY;
        maxNumber = 0;

        int argc = args.Length;

        int i;
        for (i = 0; i < argc && !string.IsNullOrEmpty(args[i]) &&
            (args[i][0] == '-' || args[i][0] == '/'); i++)
        {
            string option = args[i].TrimStart('-', '/');
            if (++i >= argc)
            {
                return false;
            }
            string optionArg = args[i];

            if (option == "type")
            {
                switch (optionArg)
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
            else if (option == "max")
            {
                if (!Int32.TryParse(optionArg, out maxNumber))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        if (i + 2 != argc)
        {
            return false;
        }

        projectName = args[i];
        jobName = args[i + 1];

        return true;
    }
}
