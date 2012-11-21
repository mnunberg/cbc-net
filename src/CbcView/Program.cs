using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Couchbase;
using Couchbase.Configuration;
using System.Web.Script.Serialization;
using System.Net;
namespace CbcView
{

	class Options : CommandLineOptionsBase
	{
		[Option("H", "hostname", Required = true, HelpText = "host to connect to")]
		public string Hostname { get; set; }

		[Option("b", "bucket", Required = false, HelpText = "bucket to use", DefaultValue = "default")]
		public string Bucket { get; set; }

		[Option("d", "design", Required = true, HelpText = "design document")]
		public string Design { get; set; }

		[Option("V", "view", Required = true, HelpText = "view name to query")]
		public string View { get; set; }

		[Option("g", "group", Required = false, HelpText = "Group all keys")]
		public bool Group { get; set; }

		[Option("G", "group-level", Required = false, HelpText = "group by this level")]
		public int GroupLevel { get; set; }

		[Option("n", "limit", Required = false, HelpText = "limit by this number", DefaultValue = 10)]
		public int Limit { get; set; }

		[Option("F", "fresh", Required = false, HelpText = "Force 'fresh' (i.e. non-stale) results")]
		public bool Fresh { get; set; }

		[OptionList("r", "range", Required = false,
			HelpText = "range (in the format of start:end)", Separator = ':')]
		public IList<string> Range { get; set; }

		[OptionList("R", "ranges", Required = false,
			Separator = ',',
			HelpText = "compound ranges (in the format of sk1,sk2:ek1,ek2")]
		public IList<string> CompoundRanges { get; set; }

		[HelpOption("?", "help", HelpText = "display help screen")]
		public string GetUsage()
		{
			HelpText help = new HelpText("Couchbase View Client");
			help.Copyright = new CopyrightInfo("Couchbase", 2012);
			help.AddPreOptionsLine("This is a view agent for Couchbase");
			help.AddOptions(this);
			return help;
		}
	}

	class Program
	{
		static void DoExecuteView(IView<IViewRow> view)
		{
			var js = new JavaScriptSerializer();

			foreach (var row in view)
			{
				Console.WriteLine(js.Serialize(row.Info).ToString());
			}

			Console.Error.WriteLine("Got {0:D} items", view.TotalRows);
		}

		static void Main(string[] args)
		{
			Options options = new Options();
			CommandLineParserSettings parserSettings = new CommandLineParserSettings();
			parserSettings.CaseSensitive = true;
			CommandLineParser parser = new CommandLineParser(parserSettings);

			if (!parser.ParseArguments(args, options, Console.Error))
			{
				return;
			}

			var config = new CouchbaseClientConfiguration();
			config.Bucket = options.Bucket;
			var url = "http://";
			if (!options.Hostname.Contains(":"))
			{
				options.Hostname += ":8091";
			}
			url += options.Hostname + "/pools";
			config.Urls.Add(new Uri(url));

			Console.Error.WriteLine("URL: " + url + ", Bucket: " + config.Bucket);
			Console.Error.WriteLine("Design: " + options.Design + ", View: " + options.View);

			CouchbaseClient cli = new CouchbaseClient(config);
			var res = cli.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Set, "foo", "bar");
			Console.WriteLine("Store result ? {0}", res.Success);

			var view = cli.GetView(options.Design, options.View);
			if (options.Group)
			{
				view.Group(options.Group);
			}
			if (options.GroupLevel > 0)
			{
				view.GroupAt(options.GroupLevel);
			}

			if (options.Range != null)
			{
				if (options.Range.Count > 2)
				{
					Console.Error.WriteLine("Too many keys in range (use -R for compount keys)");
					return;
				}
				if (!String.IsNullOrEmpty(options.Range[0]))
				{
					view.StartKey(options.Range[0]);
				}
				if (!String.IsNullOrEmpty(options.Range[1]))
				{
					view.EndKey(options.Range[1]);
				}
			}
			else if (options.CompoundRanges != null)
			{
				IList<string> sk = null, ek = null;

				int firstIx = options.CompoundRanges.IndexOf(":");
				if (firstIx == -1)
				{
					Console.Error.WriteLine("Malformed compound range");
					return;
				}
				if (firstIx == 0)
				{
					ek = options.CompoundRanges.Skip(1).ToList();
				}
				else if (firstIx == options.CompoundRanges.Count - 1)
				{
					sk = options.CompoundRanges.Take(
						options.CompoundRanges.Count - 1).ToList();
				}
				else
				{
					sk = options.CompoundRanges.Take(firstIx).ToList();
					ek = options.CompoundRanges.Skip(firstIx + 1).ToList();
				}

				if (sk != null)
				{
					Console.Error.WriteLine("Using start key " +
						new JavaScriptSerializer().Serialize(sk));
					view.StartKey(sk);
				}
				if (ek != null)
				{
					if (ek[0].StartsWith("+"))
					{
						ek[0] = new String(ek[0].Skip(1).ToArray());
						view.WithInclusiveEnd(true);
					}

					Console.Error.WriteLine("Using end key " +
						new JavaScriptSerializer().Serialize(ek));

					view.EndKey(ek);
				}
			}

			if (options.Limit > 0)
			{
				view = view.Limit(options.Limit);
			}

			if (options.Fresh)
			{
				view.Stale(StaleMode.False);
			}

			try
			{
				DoExecuteView(view);
			}
			catch (WebException exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
