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
    private static int ListProjects(string[] args)
    {
        // Validate arguments and extract optional arguments
        if (args.Length != 0)
        {
            Console.Error.WriteLine("Invalid arguments: dsjob-net -lprojects");
            return DSJOB_ERROR;
        }

        int status = 0;

        // Get the list of projects
        List<string> projectList = DSAPI.DSGetProjectList();
        if (projectList != null)
        {
            PrintStringList(0, projectList);
        }
        else
        {
            Console.Error.WriteLine("ERROR: Failed to list projects.");
            status = DSAPI.DSGetLastError();
        }

        return status;
    }
}
