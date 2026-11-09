/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET
 *
 * Based on the original dsjob C sample published by
 * Ascential Software in the DataStage Server Job Developer's Guide
 * during the DataStage 7.x era
 *
 * Rewritten and extended in C# for .NET
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using RKM.DataStage;

partial class DsJobExample
{
    // Return code for Application usage error
    private const int DSJOB_ERROR = -9999;

    // Delegate for primary command handler functions
    private delegate int CommandHandler(string[] args);

    private struct PrimaryCommand
    {
        public readonly string Name;
        public readonly CommandHandler Handler;

        public PrimaryCommand(string name, CommandHandler handler)
        {
            Name = name;
            Handler = handler;
        }
    }

    // The following array defines all the allowed/known primary command options
    private static readonly PrimaryCommand[] PrimaryCommands = new[]
    {
        new PrimaryCommand("run", JobRun),
        new PrimaryCommand("stop", JobStop),
        new PrimaryCommand("lprojects", ListProjects),
        new PrimaryCommand("ljobs", ListJobs),
        new PrimaryCommand("lstages", ListStages),
        new PrimaryCommand("llinks", ListLinks),
        new PrimaryCommand("jobinfo", JobInfo),
        new PrimaryCommand("stageinfo", JobStageInfo),
        new PrimaryCommand("linkinfo", JobLinkInfo),
        new PrimaryCommand("lparams", ListParams),
        new PrimaryCommand("paraminfo", JobParamInfo),
        new PrimaryCommand("log", JobLog),
        new PrimaryCommand("logsum", JobLogSummary),
        new PrimaryCommand("logdetail", JobLogDetail),
        new PrimaryCommand("lognewest", JobLogNewest)
    };

    // Prints application usage
    private static void ShowUsage()
    {
        Console.Error.WriteLine("Command syntax:");
        Console.Error.WriteLine("\tdsjob-net [-authfile <file.json>]");
        Console.Error.WriteLine("\t       OR [-domain <domain>][-server <server>][-user <user>][-password <password>]");
        Console.Error.WriteLine("\t\t\t<primary command> [<arguments>]");
        Console.Error.WriteLine("Valid primary command options are:");

        foreach (PrimaryCommand cmd in PrimaryCommands)
        {
            Console.Error.WriteLine($"\t-{cmd.Name}");
        }
    }

    // Main entry point
    public static int Main(string[] args)
    {
        string domain = null;
        string username = null;
        string password = null;
        string server = null;
        string authFile = null;
        bool inlineCredentialsProvided = false;

        // Parse connection arguments, stopping at the primary command
        int argpos = 0;
        while (argpos < args.Length)
        {
            string option = args[argpos].ToLowerInvariant();
            if (option != "-domain" && option != "-user" &&
                option != "-password" && option != "-server" && option != "-authfile")
            {
                break;
            }

            if (argpos + 1 >= args.Length)
            {
                goto ShowError;
            }

            string value = args[argpos + 1];
            switch (option)
            {
                case "-domain":
                    inlineCredentialsProvided = true;
                    domain = value.Equals("NONE", StringComparison.OrdinalIgnoreCase) ? null : value;
                    break;
                case "-user":
                    inlineCredentialsProvided = true;
                    username = value;
                    break;
                case "-password":
                    inlineCredentialsProvided = true;
                    password = value;
                    break;
                case "-server":
                    inlineCredentialsProvided = true;
                    server = value;
                    break;
                case "-authfile":
                    if (authFile != null)
                    {
                        Console.Error.WriteLine("ERROR: -authfile may only be specified once");
                        return DSJOB_ERROR;
                    }
                    authFile = value;
                    break;
            }

            argpos += 2;
        }

        // Credentials are either read from the file or supplied inline, never mixed
        if (authFile != null)
        {
            if (inlineCredentialsProvided)
            {
                Console.Error.WriteLine("ERROR: -authfile cannot be combined with -domain, -server, -user or -password");
                return DSJOB_ERROR;
            }

            if (!TryReadAuthFile(authFile, out AuthFileSettings settings))
            {
                return DSJOB_ERROR;
            }

            domain = string.Equals(settings.Domain, "NONE", StringComparison.OrdinalIgnoreCase) ? null : settings.Domain;
            server = settings.Server;
            username = settings.Username;
            password = settings.Password;
        }

        // Parse the primary command, then invoke it
        int argc = args.Length - argpos;
        if (argc != 0 && !string.IsNullOrEmpty(args[argpos]) &&
            (args[argpos][0] == '-' || args[argpos][0] == '/'))
        {
            string cmd = args[argpos].ToLowerInvariant().TrimStart('-', '/');
            for (int i = 0; i < PrimaryCommands.Length; i++)
            {
                if (cmd == PrimaryCommands[i].Name)
                {
                    // Setup DSAPI server parameters
                    DSAPI.DSSetServerParams(domain, username, password, server);

                    // Simplify arguments for command handling (excludes server parameters and command name)
                    string[] cmdArgs = new string[argc - 1];
                    Array.Copy(args, argpos + 1, cmdArgs, 0, argc - 1);

                    // Action request
                    int status = PrimaryCommands[i].Handler(cmdArgs);

                    // Report status to console
                    Console.Error.WriteLine($"\nStatus code = {status}");

                    if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR && status != DSJOB_ERROR)
                    {
                        List<string> errorText = DSAPI.DSGetLastErrorMsg(IntPtr.Zero);
                        if (errorText != null && errorText.Count > 0)
                        {
                            Console.Error.WriteLine("Last recorded error message =");
                            PrintStringList(0, errorText);
                        }
                    }

                    return status;
                }
            }

            // We only get here if we failed to match a valid command
            Console.Error.WriteLine("Invalid/unknown primary command switch.");
        }

    ShowError:
        ShowUsage();
        return DSJOB_ERROR;
    }
}

