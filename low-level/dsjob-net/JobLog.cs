/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using RKM.DataStage;
using System.Text;

partial class DsJobExample
{
    private static int JobLog(string[] args)
    {
        string project, job;
        DSAPI.LogEventType type;

        // Validate arguments and extract optional arguments
        if (!IsValidLogArgs(args, out project, out job, out type))
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -log");
            Console.Error.WriteLine("\t\t\t[-info | -warn]");
            Console.Error.WriteLine("\t\t\t<project> <job>");
            Console.Error.WriteLine("Log message is read from stdin.");
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
                        StringBuilder message = new StringBuilder();

                        // Read the message from stdin
                        Console.WriteLine("Enter message text; finish with Ctrl+Z then Enter on Windows, or Ctrl+D on Unix");

                        do
                        {
                            int x = Console.Read();
                            if (x == -1 || x == 4)
                            {
                                break;
                            }
                            message.Append(Convert.ToChar(x));
                        }
                        while (true);

                        Console.WriteLine("Message read.");

                        // Add message to the log
                        status = DSAPI.DSLogEvent(hJob, type, message.ToString());
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine("ERROR: Failed adding log entry");
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

    private static bool IsValidLogArgs(string[] args, out string projectName, out string jobName, out DSAPI.LogEventType eventType)
    {
        projectName = jobName = null;
        eventType = DSAPI.LogEventType.DSJ_LOGINFO;

        int argc = args.Length;

        int i;
        for (i = 0; i < argc && !string.IsNullOrEmpty(args[i]) &&
            (args[i][0] == '-' || args[i][0] == '/'); i++)
        {
            string option = args[i].TrimStart('-', '/');

            // Note: not mutually exclusive check on info or warn
            switch (option)
            {

                case "info": eventType = DSAPI.LogEventType.DSJ_LOGINFO; break;
                case "warn": eventType = DSAPI.LogEventType.DSJ_LOGWARNING; break;
                default: return false;
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
