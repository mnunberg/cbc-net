using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Couchbase;
using Couchbase.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using Couchbase.Results;
using Couchbase.Operations;
using Couchbase.Cbc.Commands;

namespace Couchbase.Cbc
{
	class Program
	{
		static Dictionary<string, Type> commandMap = new Dictionary<string, Type>()
		{
			{ "set", typeof(CbcSet) },
			{ "add", typeof(CbcSet) },
			{ "replace", typeof(CbcSet) },
			{ "append", typeof(CbcSet) },
			{ "prepend", typeof(CbcSet) },
			{ "observe", typeof(CbcObserveWrap) },
			{ "get", typeof(CbcGet) }
		};

		static void Main(string[] args)
		{
			CbcOptions options = new CbcOptions();
			CommandLineParserSettings settings = new CommandLineParserSettings();
			settings.CaseSensitive = true;
			CommandLineParser parser = new CommandLineParser(settings);
			if (!parser.ParseArguments(args, options, System.Console.Error))
			{
				return;
			}


			options.Process();

			var config = new CouchbaseClientConfiguration();
			config.Bucket = options.Bucket;
			config.Username = options.Username;
			config.Password = options.Password;
			config.BucketPassword = options.BucketPassword;
			string uriString = "http://" + options.Hostname + "/pools";
			System.Console.WriteLine("URI: " + uriString);
			config.Urls.Add(new UriBuilder(uriString).Uri);

			DateTime begin = DateTime.Now;
			CouchbaseClient cli = new CouchbaseClient(config);
			System.Console.WriteLine("Created new client..");

			if (!commandMap.ContainsKey(options.Command))
			{
				throw new ArgumentException("Unknown command!");
			}

			Type t = commandMap[options.Command];
			Type[] proto = {
							   typeof(CouchbaseClient),
							   typeof(string),
							   typeof(CbcOptions)
						   };
			object[] cargs = {
								cli,
								options.Key,
								options
							};



			CommandBase cmd = (CommandBase) t.GetConstructor(proto).Invoke(cargs);
			cmd.Execute();

			var duration = DateTime.Now - begin;
			Console.WriteLine(
				String.Format("Duration was {0:F} Sec.", duration.TotalMilliseconds/1000));
		}
	}
}
