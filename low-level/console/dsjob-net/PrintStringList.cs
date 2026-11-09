/*
 * dsjob-net
 * Command-line utility for IBM DataStage using RKM DSAPI for .NET.
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research
 */

using System;
using System.Collections.Generic;

partial class DsJobExample
{
    // Prints out string list with indentation to the console
    private static void PrintStringList(int indentLevel, List<string> list)
    {
        string indent = new(' ', Math.Max(0, indentLevel));

        if (list != null && list.Count > 0)
        {
            foreach (string strItem in list)
            {
                Console.WriteLine($"{indent}{strItem}");
            }
        }
        else
        {
            Console.WriteLine($"{indent}<none>");
        }
    }
}
