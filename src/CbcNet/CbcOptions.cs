using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Couchbase.Results;
using Couchbase.Operations;

namespace Couchbase.Cbc
{
	class CbcOptions : CommandLineOptionsBase
	{
		[Option("b", "bucket", Required = false, HelpText = "Bucket to connect to",
			DefaultValue = "default")]
		public string Bucket { get; set; }

		[Option("B", "bucket-password", Required = false, HelpText = "Bucket password")]
		public string BucketPassword { get; set; }

		[Option("H", "hostname", Required = false, HelpText = "Hostname to use",
			DefaultValue = "127.0.0.1:8091")]
		public string Hostname { get; set; }

		[Option("u", "username", Required = false, HelpText = "Username")]
		public string Username { get; set; }

		[Option("p", "password", Required = false, HelpText = "Password")]
		public string Password { get; set; }

		[Option("c", "command", Required = true, HelpText = "Command to use")]
		public string Command { get; set; }

		[Option("k", "key", HelpText = "Key to use", Required = true)]
		public string Key { get; set; }

		[Option("V", "value", HelpText = "Value for command (if applicable)")]
		public string Value { get; set; }

		[Option("C", "cas", HelpText = "CAS For command (if applicable)")]
		public UInt64 Cas { get; set; }

		[Option("E", "expiry", HelpText = "Expiration (if applicable)", DefaultValue = 0)]
		public int expiry { get; set; }
		public TimeSpan Expiry { get; set; }

		[Option("P", "persist", HelpText = "Persist to this many nodes")]
		public int persist { get; set; }
		public PersistTo Persist { get; set; }

		[Option("R", "replicate", HelpText = "Replicate to this many nodes")]
		public int replicate { get; set; }
		public ReplicateTo Replicate { get; set; }



		[HelpOption("?", "help", HelpText = "Display help screen")]
		public string GetUsage()
		{
			HelpText help = new HelpText("Couchbase Command Line Client");
			help.Copyright = new CopyrightInfo("Couchbase", 2012);
			help.AddPreOptionsLine("This emulates the 'cbc' command line as in libcouchbase");
			help.AddOptions(this);
			return help;
		}

		public void Process()
		{
			if (expiry != 0)
			{
				Expiry = new TimeSpan(0, 0, expiry);
			}
			else
			{
				Expiry = new TimeSpan(0);
			}

			if (persist != 0)
			{
				if (persist > 4)
				{
					System.Console.Error.WriteLine(
						"Detected too high of a persist value (will go ahead anyway");
				}
				Persist = (PersistTo)persist;
			}
			else
			{
				Persist = PersistTo.Zero;
			}

			if (replicate != 0)
			{
				if (replicate > 3)
				{
					System.Console.Error.WriteLine(
						"Detected too high a replicate value (will go ahead anyway..)");
				}
				Replicate = (ReplicateTo)replicate;
			}
			else
			{
				Replicate = ReplicateTo.Zero;
			}

			if (Hostname.IndexOf(':') == -1)
			{
				Hostname += ":8091";
			}
		}
	}
}
