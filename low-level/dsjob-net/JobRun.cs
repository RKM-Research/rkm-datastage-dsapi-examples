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
    private struct JobRunAttributes
    {
        public int warningLimit;
        public int rowLimit;
        public DSAPI.RunModeType mode;
        public List<string> parameters;
        public bool waitForJob;
    }

    private static int JobRun(string[] args)
    {
        string project, job;
        JobRunAttributes attributes;

        // Validate arguments and extract optional arguments
        if (!IsValidRunArgs(args, out project, out job, out attributes))
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -run");
            Console.Error.WriteLine("\t\t\t[-mode <NORMAL | RESET | VALIDATE>]");
            Console.Error.WriteLine("\t\t\t[-param <name>=<value>]");
            Console.Error.WriteLine("\t\t\t[-warn <n>]");
            Console.Error.WriteLine("\t\t\t[-rows <n>]");
            Console.Error.WriteLine("\t\t\t[-wait]");
            Console.Error.WriteLine("\t\t\t<project> <job>");
            return DSJOB_ERROR;
        }

        DSAPI.DSPROJECT hProject = DSAPI.DSOpenProject(project);
        if (!hProject.HasValue)
        {
            Console.Error.WriteLine($"ERROR: Failed to open project '{project}'");
            return DSAPI.DSGetLastError();
        }

        try
        {
            DSAPI.DSJOB hJob = DSAPI.DSOpenJob(hProject, job);
            if (!hJob.HasValue)
            {
                Console.Error.WriteLine($"ERROR: Failed to open job '{job}'");
                return DSAPI.DSGetLastError();
            }

            try
            {
                int status = DSAPI.DSLockJob(hJob);
                if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                {
                    Console.Error.WriteLine($"ERROR: Failed to lock job '{job}'");
                    return status;
                }

                try
                {
                    if (attributes.warningLimit >= 0)
                    {
                        status = DSAPI.DSSetJobLimit(hJob, DSAPI.JobLimitType.DSJ_LIMITWARN, attributes.warningLimit);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine($"ERROR: Setting warning limit for job '{job}'");
                        }
                    }

                    if (attributes.rowLimit != 0 && status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                    {
                        status = DSAPI.DSSetJobLimit(hJob, DSAPI.JobLimitType.DSJ_LIMITROWS, attributes.rowLimit);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine($"ERROR: Setting row limit for job '{job}'");
                        }
                    }

                    for (int i = 0; i < attributes.parameters.Count &&
                         status == (int)DSAPI.ErrorCodes.DSJE_NOERROR; i++)
                    {
                        status = SetParam(hJob, attributes.parameters[i]);
                    }

                    if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                    {
                        status = DSAPI.DSRunJob(hJob, attributes.mode);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine($"ERROR: Failed to run job '{job}'");
                        }
                    }

                    if (attributes.waitForJob && status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                    {
                        Console.WriteLine("Waiting for job...");
                        status = DSAPI.DSWaitForJob(hJob);
                        if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            Console.Error.WriteLine($"ERROR: Failed to wait for job '{job}'");
                        }
                    }

                    return status;
                }
                finally
                {
                    DSAPI.DSUnlockJob(hJob);
                }
            }
            finally
            {
                DSAPI.DSCloseJob(hJob);
            }
        }
        finally
        {
            DSAPI.DSCloseProject(hProject);
        }
    }

    // Sets job parameter based on "name=value" string
    private static int SetParam(DSAPI.DSJOB hJob, string paramNameValue)
    {
        if (paramNameValue == null)
        {
            throw new ArgumentNullException(nameof(paramNameValue));
        }

        int index = paramNameValue.IndexOf('=');
        if (index <= 0)
        {
            Console.Error.WriteLine("ERROR: Invalid parameter format; expected name=value");
            return DSJOB_ERROR;
        }

        string paramName = paramNameValue.Substring(0, index);
        string paramValue = paramNameValue.Substring(index + 1);

        // Get the parameter information which tells us what type it is
        DSAPI.DSPARAMINFO paramInfo;
        int status = DSAPI.DSGetParamInfo(hJob, paramName, out paramInfo);
        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
        {
            // Use the wrapper utilities to allocate native memory for string parameters
            // The allocated memory is released in the finally block
            IntPtr globalStringPtr = IntPtr.Zero;

            try
            {
                // Construct the parameter value for the server
                // Numeric values are validated and string values are passed through
                DSAPI.DSPARAM paramData = new DSAPI.DSPARAM();
                paramData.paramType = paramInfo.paramType;
                switch (paramInfo.paramType)
                {
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_STRING:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pString = globalStringPtr;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_ENCRYPTED:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pEncrypt = globalStringPtr;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_INTEGER:
                        if (!int.TryParse(paramValue, out int iValue))
                        {
                            Console.Error.WriteLine($"ERROR: Invalid integer for parameter '{paramName}'");
                            return DSJOB_ERROR;
                        }
                        paramData.paramValue.pInt = iValue;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_FLOAT:
                        if (!float.TryParse(paramValue, out float fValue))
                        {
                            Console.Error.WriteLine($"ERROR: Invalid float for parameter '{paramName}'");
                            return DSJOB_ERROR;
                        }
                        paramData.paramValue.pFloat = fValue;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_PATHNAME:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pPath = globalStringPtr;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_LIST:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pListValue = globalStringPtr;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_DATE:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pDate = globalStringPtr;
                        break;
                    case (int)DSAPI.ParamType.DSJ_PARAMTYPE_TIME:
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pTime = globalStringPtr;
                        break;
                    default:
                        paramData.paramType = (int)DSAPI.ParamType.DSJ_PARAMTYPE_STRING;
                        globalStringPtr = DSAPI.Utilities.StringToHGlobalAnsi(paramValue);
                        paramData.paramValue.pString = globalStringPtr;
                        break;
                }

                // Try setting the parameter
                status = DSAPI.DSSetParam(hJob, paramName, ref paramData);
                if (status != (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                {
                    Console.Error.WriteLine($"ERROR: Failed setting parameter '{paramName}' (status {status})");
                }
            }
            finally
            {
                // Free the allocated native memory, including on parameter-setting errors
                DSAPI.Utilities.FreeHGlobal(globalStringPtr);
            }
        }
        else
        {
            Console.Error.WriteLine($"ERROR: Failure {status} getting information for parameter '{paramName}'");
        }

        return status;
    }

    private static bool IsValidRunArgs(string[] args, out string projectName, out string jobName, out JobRunAttributes jobRunAttributes)
    {
        projectName = jobName = null;
        jobRunAttributes.warningLimit = -1;
        jobRunAttributes.rowLimit = 0;
        jobRunAttributes.mode = DSAPI.RunModeType.DSJ_RUNNORMAL;
        jobRunAttributes.parameters = new List<string>();
        jobRunAttributes.waitForJob = false;

        int argc = args.Length;

        int i;
        for (i = 0; i < argc && !string.IsNullOrEmpty(args[i]) &&
            (args[i][0] == '-' || args[i][0] == '/'); i++)
        {
            string option = args[i].TrimStart('-', '/');

            if (option == "wait")
            {
                jobRunAttributes.waitForJob = true;
            }
            else
            {
                if (++i >= argc)
                {
                    return false;
                }
                string optionArg = args[i];

                switch (option)
                {
                    case "mode":
                        switch (optionArg)
                        {
                            case "NORMAL":
                                jobRunAttributes.mode = DSAPI.RunModeType.DSJ_RUNNORMAL;
                                break;
                            case "RESET":
                                jobRunAttributes.mode = DSAPI.RunModeType.DSJ_RUNRESET;
                                break;
                            case "VALIDATE":
                                jobRunAttributes.mode = DSAPI.RunModeType.DSJ_RUNVALIDATE;
                                break;
                            default:
                                return false;
                        }
                        break;
                    case "param":
                        int sepIndex = optionArg.IndexOf('=');
                        if (sepIndex > 0 && sepIndex < optionArg.Length)
                        {
                            jobRunAttributes.parameters.Add(optionArg);
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case "warn":
                        if (!int.TryParse(optionArg, out jobRunAttributes.warningLimit))
                        {
                            return false;
                        }
                        break;
                    case "rows":
                        if (!int.TryParse(optionArg, out jobRunAttributes.rowLimit))
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            }
        }

        // Must be two parameters left... project and job
        if (i + 2 != argc)
        {
            return false;
        }

        projectName = args[i];
        jobName = args[i + 1];

        return true;
    }
}
