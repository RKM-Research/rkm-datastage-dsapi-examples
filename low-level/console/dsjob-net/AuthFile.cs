/*
 * dsjob-net
 * Optional JSON authentication file support
 *
 * Copyright © 2022–2026 Richard K. Morris / RKM Research Ltd
 */

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

partial class DsJobExample
{
    private sealed class AuthFileSettings
    {
        public AuthFileSettings() { }

        public string Domain { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Read credentials without writing them to the console or log
    private static bool TryReadAuthFile(string path, out AuthFileSettings settings)
    {
        settings = null;

        try
        {
            // Unknown properties are rejected to catch misspelled field names
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            };

            settings = JsonSerializer.Deserialize<AuthFileSettings>(File.ReadAllText(path), options);
        }
        catch (JsonException)
        {
            Console.Error.WriteLine("ERROR: Invalid authentication file JSON or unknown property");
            return false;
        }
        catch (IOException)
        {
            Console.Error.WriteLine("ERROR: Cannot read authentication file; check its path and accessibility");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.Error.WriteLine("ERROR: Access to authentication file was denied");
            return false;
        }
        catch (ArgumentException)
        {
            Console.Error.WriteLine("ERROR: Invalid authentication file path");
            return false;
        }

        if (settings == null ||
            string.IsNullOrWhiteSpace(settings.Username) ||
            string.IsNullOrWhiteSpace(settings.Password))
        {
            Console.Error.WriteLine("ERROR: Authentication file must contain non-empty username and password strings");
            settings = null;
            return false;
        }

        return true;
    }
}
