/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using RKM.DataStage;

partial class DsJobExample
{
    private static int JobLogDetail(string[] args)
    {
        string project, job;
        int eventId;

        // Validate arguments and extract optional arguments
        if (!IsValidJobLogDetailArgs(args, out project, out job, out eventId))
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -logdetail <project> <job> <event id>");
            return DSJOB_ERROR;
        }

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
                        // Declare structure for log entry details
                        DSAPI.DSLOGDETAIL logDetail;

                        // Try getting all the log info
                        status = DSAPI.DSGetLogEntry(hJob, eventId, out logDetail);
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            PrintLogDetail(0, logDetail);
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get log details for job '{job}' event id '{eventId}'");
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

    // Prints out details of a job log entry with indentation to the console
    private static void PrintLogDetail(int indentLevel, DSAPI.DSLOGDETAIL logDetail)
    {
        string indent = new(' ', Math.Max(0, indentLevel));
        if (logDetail.eventId < 0)
        {
            Console.WriteLine($"{indent}Event Id: <unknown>");
        }
        else
        {
            Console.WriteLine($"{indent}Event Id: {logDetail.eventId}");
        }

        DateTime dt = DSAPI.Utilities.UnixTimestampToDateTime(logDetail.timestamp);
        DateTime dtLocal = DSAPI.Utilities.DateTimeToLocalTime(dt);
        Console.WriteLine($"{indent}Time\t: {dtLocal}");

        string evtType;
        switch (logDetail.type)
        {
            case (int)DSAPI.LogEventType.DSJ_LOGINFO:
                evtType = "INFO";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGWARNING:
                evtType = "WARNING";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGFATAL:
                evtType = "FATAL";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGREJECT:
                evtType = "REJECT";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGSTARTED:
                evtType = "STARTED";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGRESET:
                evtType = "RESET";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGBATCH:
                evtType = "BATCH";
                break;
            case (int)DSAPI.LogEventType.DSJ_LOGOTHER:
                evtType = "OTHER";
                break;
            default:
                evtType = "<unknown type>";
                break;
        }
        Console.WriteLine($"{indent}Type\t: {evtType}");

        string user = DSAPI.Utilities.ExtractStringFromIntPtr(logDetail.username);
        Console.WriteLine($"{indent}User\t: {user}");

        List<string> message = DSAPI.Utilities.ExtractStringListFromIntPtr(logDetail.fullMessage);
        Console.WriteLine($"{indent}Message\t:");
        PrintStringList(indentLevel + 8, message);
    }

    private static bool IsValidJobLogDetailArgs(string[] args, out string projectName, out string jobName, out int eventId)
    {
        projectName = jobName = null;
        eventId = 0;

        int argc = args.Length;
        if (argc != 3)
        {
            return false;
        }

        projectName = args[0];
        jobName = args[1];

        if (!int.TryParse(args[2], out eventId))
        {
            return false;
        }

        return true;
    }
}
