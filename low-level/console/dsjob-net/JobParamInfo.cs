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
    private static int JobParamInfo(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -paraminfo <project> <job> <param>");
            return DSJOB_ERROR;
        }
        string project = args[0];
        string job = args[1];
        string param = args[2];

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
                        // Declare structure for parameter information
                        DSAPI.DSPARAMINFO paramInfo;

                        // Get the parameter information
                        status = DSAPI.DSGetParamInfo(hJob, param, out paramInfo);
                        if (status == (int)DSAPI.ErrorCodes.DSJE_NOERROR)
                        {
                            string type = null;
                            switch (paramInfo.paramType)
                            {
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_STRING: type = "String"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_ENCRYPTED: type = "Encrypted"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_INTEGER: type = "Integer"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_FLOAT: type = "Float"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_PATHNAME: type = "Pathname"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_LIST: type = "List"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_DATE: type = "Date"; break;
                                case (int)DSAPI.ParamType.DSJ_PARAMTYPE_TIME: type = "Time"; break;
                                default: type = "<unknown type>"; break;
                            }

                            Console.WriteLine($"Type\t\t: {type} ({paramInfo.paramType})");
                            Console.WriteLine($"Help Text\t: {DSAPI.Utilities.ExtractStringFromIntPtr(paramInfo.helpText)}");
                            Console.WriteLine($"Prompt\t\t: {DSAPI.Utilities.ExtractStringFromIntPtr(paramInfo.paramPrompt)}");
                            Console.WriteLine($"Prompt At Run\t: {paramInfo.promptAtRun}");
                            Console.Write("Default Value\t:");
                            if (paramInfo.paramType == (int)DSAPI.ParamType.DSJ_PARAMTYPE_ENCRYPTED)
                            {
                                Console.WriteLine(" <redacted>");
                            }
                            else
                            {
                                PrintParamValue(paramInfo.defaultValue);
                            }
                            Console.Write("Original Default:");
                            if (paramInfo.paramType == (int)DSAPI.ParamType.DSJ_PARAMTYPE_ENCRYPTED)
                            {
                                Console.WriteLine(" <redacted>");
                            }
                            else
                            {
                                PrintParamValue(paramInfo.desDefaultValue);
                            }
                            if (paramInfo.paramType == (int)DSAPI.ParamType.DSJ_PARAMTYPE_LIST)
                            {
                                Console.WriteLine("List Values\t:");
                                PrintStringList(2, DSAPI.Utilities.ExtractStringListFromIntPtr(paramInfo.listValues));

                                Console.WriteLine("Original List\t:");
                                PrintStringList(2, DSAPI.Utilities.ExtractStringListFromIntPtr(paramInfo.desListValues));
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR: Failed to get param info for job '{job}' parameter '{param}'");
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

    // Prints a parameter value to the console
    private static void PrintParamValue(DSAPI.DSPARAM param)
    {
        string value = null;
        switch (param.paramType)
        {
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_STRING:
                value = DSAPI.Utilities.ExtractStringFromIntPtr(param.paramValue.pString);
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_ENCRYPTED:
                value = "<redacted>";
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_INTEGER:
                value = param.paramValue.pInt.ToString();
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_FLOAT:
                value = param.paramValue.pFloat.ToString();  // %G in C
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_PATHNAME:
                value = DSAPI.Utilities.ExtractStringFromIntPtr(param.paramValue.pPath);
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_LIST:
                value = DSAPI.Utilities.ExtractStringFromIntPtr(param.paramValue.pListValue);
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_DATE:
                value = DSAPI.Utilities.ExtractStringFromIntPtr(param.paramValue.pDate);
                break;
            case (int)DSAPI.ParamType.DSJ_PARAMTYPE_TIME:
                value = DSAPI.Utilities.ExtractStringFromIntPtr(param.paramValue.pTime);
                break;
        }
        Console.WriteLine($" {value}");
    }
}
